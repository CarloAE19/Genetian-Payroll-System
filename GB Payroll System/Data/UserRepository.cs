using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using GB_Payroll_System.Models;

namespace GB_Payroll_System.Data
{
    public class UserRepository
    {
        public List<User> GetAll()
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            return [.. conn.Query<User>("SELECT * FROM Users ORDER BY FullName;")];
        }

        public User? GetByUsername(string username)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            return conn.QueryFirstOrDefault<User>(
                "SELECT * FROM Users WHERE Username = @Username LIMIT 1;",
                new { Username = username });
        }

        public bool UsernameExists(string username, int excludeId = 0)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            return conn.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM Users WHERE Username = @Username AND Id != @ExcludeId;",
                new { Username = username, ExcludeId = excludeId }) > 0;
        }

        public void Insert(User user)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            conn.Execute(@"
                INSERT INTO Users (Username, PasswordHash, FullName, Email, Role, IsActive, CreatedAt)
                VALUES (@Username, @PasswordHash, @FullName, @Email, @Role, @IsActive, @CreatedAt);", user);
        }

        public void Update(User user)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            conn.Execute(@"
                UPDATE Users SET
                    FullName = @FullName, Email = @Email,
                    Role = @Role, IsActive = @IsActive
                WHERE Id = @Id;", user);
        }

        public void ChangePassword(int userId, string newPasswordHash)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            conn.Execute("UPDATE Users SET PasswordHash = @Hash WHERE Id = @Id;",
                new { Hash = newPasswordHash, Id = userId });
        }

        public void SetActive(int id, bool isActive)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            conn.Execute("UPDATE Users SET IsActive = @IsActive WHERE Id = @Id;",
                new { IsActive = isActive, Id = id });
        }

        /// <summary>
        /// BCrypt password hashing with work factor 11 for production-grade security.
        /// </summary>
        public static string HashPassword(string plain)
        {
            return BCrypt.Net.BCrypt.HashPassword(plain, workFactor: 11);
        }

        /// <summary>
        /// Verifies plain text password against stored hash (supports BCrypt, legacy SHA-256, and plain text).
        /// </summary>
        public static bool VerifyPassword(string plain, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash)) return false;

            // BCrypt hash check (starts with $2a$, $2b$, or $2y$)
            if (storedHash.StartsWith("$2a$") || storedHash.StartsWith("$2b$") || storedHash.StartsWith("$2y$"))
            {
                try { return BCrypt.Net.BCrypt.Verify(plain, storedHash); }
                catch { return false; }
            }

            // Legacy SHA-256 check
            var sha256Bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plain));
            string sha256Hash = Convert.ToHexString(sha256Bytes).ToLowerInvariant();
            if (storedHash.Equals(sha256Hash, StringComparison.OrdinalIgnoreCase))
                return true;

            // Legacy plaintext fallback
            return storedHash == plain;
        }
    }
}
