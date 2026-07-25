using System;
using GB_Payroll_System.Data;
using GB_Payroll_System.Models;
using Dapper;

namespace GB_Payroll_System.Services
{
    public class AuthService
    {
        public static User? CurrentUser { get; private set; }

        public static (bool Success, string Message, User? User) Authenticate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "Please enter both username and password.", null);
            }

            try
            {
                // Fallback offline mock for testing before database connects
                if (username.Equals("admin", StringComparison.OrdinalIgnoreCase) && password == "admin123")
                {
                    CurrentUser = new User
                    {
                        Id = 1,
                        Username = "admin",
                        FullName = "Genetian Administrator",
                        Role = UserRole.Admin,
                        Email = "admin@genetian.ph"
                    };
                    return (true, "Authentication successful.", CurrentUser);
                }

                using var conn = DbConnectionFactory.CreateConnection();
                conn.Open();

                string sql = "SELECT * FROM Users WHERE Username = @Username AND IsActive = TRUE LIMIT 1;";
                var user = conn.QueryFirstOrDefault<User>(sql, new { Username = username });

                if (user == null)
                {
                    return (false, "Invalid username or password.", null);
                }

                // In production, compare hashed password. Direct match for demo.
                if (user.PasswordHash != password)
                {
                    return (false, "Invalid username or password.", null);
                }

                // Update last login
                conn.Execute("UPDATE Users SET LastLoginAt = CURRENT_TIMESTAMP WHERE Id = @Id;", new { user.Id });

                CurrentUser = user;
                return (true, "Login successful.", user);
            }
            catch (Exception ex)
            {
                // If database connection fails, allow fallback offline admin login
                if (username.Equals("admin", StringComparison.OrdinalIgnoreCase) && password == "admin123")
                {
                    CurrentUser = new User
                    {
                        Id = 1,
                        Username = "admin",
                        FullName = "Genetian Administrator (Offline)",
                        Role = UserRole.Admin
                    };
                    return (true, "Offline Admin Login.", CurrentUser);
                }

                return (false, $"Connection error: {ex.Message}", null);
            }
        }

        public static void Logout()
        {
            CurrentUser = null;
        }
    }
}
