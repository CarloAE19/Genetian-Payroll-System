using System;
using System.Data;
using Npgsql;

namespace GB_Payroll_System.Data
{
    public class DbConnectionFactory
    {
        public static string Server { get; set; } = "localhost";
        public static string Port { get; set; } = "5432";
        public static string Database { get; set; } = "genetian_payroll";
        public static string Username { get; set; } = "postgres";
        public static string Password { get; set; } = "postgres";

        public static string ConnectionString => 
            $"Host={Server};Port={Port};Database={Database};Username={Username};Password={Password};Timeout=5;CommandTimeout=10;";

        public static IDbConnection CreateConnection()
        {
            return new NpgsqlConnection(ConnectionString);
        }

        public static bool TestConnection(out string errorMessage)
        {
            try
            {
                using var conn = CreateConnection();
                conn.Open();
                errorMessage = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }
    }
}
