using System;
using GB_Payroll_System.Data;
using GB_Payroll_System.Models;
using Dapper;

namespace GB_Payroll_System.Services
{
    public class AuthService
    {
        public static User? CurrentUser { get; private set; }

        // RBAC Permissions Helpers: HR has full access to all HR, Employee, Attendance, Pakyaw, Holiday, & Payroll management
        public static bool HasFullAccess => CurrentUser?.Role == UserRole.Admin || CurrentUser?.Role == UserRole.HR;
        public static bool CanManageEmployees => HasFullAccess;
        public static bool CanManagePromotions => HasFullAccess;
        public static bool CanManageHolidays => HasFullAccess || CurrentUser?.Role == UserRole.Management;
        public static bool CanManageAttendance => HasFullAccess || CurrentUser?.Role == UserRole.Accounting;
        public static bool CanManagePakyaw => HasFullAccess || CurrentUser?.Role == UserRole.Accounting;
        public static bool CanManagePayroll => HasFullAccess || CurrentUser?.Role == UserRole.Accounting;
        public static bool CanManageSettings => HasFullAccess;

        public static (bool Success, string Message, User? User) Authenticate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "Please enter both username and password.", null);
            }

            try
            {
                // Fallback offline mock for testing HR & Admin credentials before database connects
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

                if (username.Equals("hr", StringComparison.OrdinalIgnoreCase) && password == "hr123")
                {
                    CurrentUser = new User
                    {
                        Id = 2,
                        Username = "hr",
                        FullName = "Genetian HR Manager",
                        Role = UserRole.HR,
                        Email = "hr@genetian.ph"
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

                // Verify password with BCrypt (with automatic legacy SHA-256 & plaintext fallback)
                if (!UserRepository.VerifyPassword(password, user.PasswordHash))
                {
                    return (false, "Invalid username or password.", null);
                }

                // Transparently upgrade legacy SHA-256 / plaintext passwords to BCrypt upon successful login
                if (!user.PasswordHash.StartsWith("$2a$") && !user.PasswordHash.StartsWith("$2b$") && !user.PasswordHash.StartsWith("$2y$"))
                {
                    string bcryptHash = UserRepository.HashPassword(password);
                    conn.Execute("UPDATE Users SET PasswordHash = @Hash WHERE Id = @Id;", new { Hash = bcryptHash, user.Id });
                    user.PasswordHash = bcryptHash;
                }

                // Update last login timestamp
                conn.Execute("UPDATE Users SET LastLoginAt = CURRENT_TIMESTAMP WHERE Id = @Id;", new { user.Id });

                CurrentUser = user;
                return (true, "Login successful.", user);
            }
            catch (Exception ex)
            {
                // Offline login fallback if DB server is unreachable
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

                if (username.Equals("hr", StringComparison.OrdinalIgnoreCase) && password == "hr123")
                {
                    CurrentUser = new User
                    {
                        Id = 2,
                        Username = "hr",
                        FullName = "Genetian HR Manager (Offline)",
                        Role = UserRole.HR
                    };
                    return (true, "Offline HR Login.", CurrentUser);
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
