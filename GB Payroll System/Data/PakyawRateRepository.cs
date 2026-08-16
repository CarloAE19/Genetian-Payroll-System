using System.Collections.Generic;
using Dapper;
using GB_Payroll_System.Models;

namespace GB_Payroll_System.Data
{
    public class PakyawRateRepository
    {
        public List<PakyawRate> GetAll(bool activeOnly = true)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            string sql = activeOnly
                ? "SELECT * FROM PakyawRates WHERE IsActive = TRUE ORDER BY TaskCode;"
                : "SELECT * FROM PakyawRates ORDER BY TaskCode;";
            return [.. conn.Query<PakyawRate>(sql)];
        }

        public void Insert(PakyawRate rate)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            conn.Execute(@"
                INSERT INTO PakyawRates (TaskCode, TaskName, UnitOfMeasure, RatePerUnit, IsActive)
                VALUES (@TaskCode, @TaskName, @UnitOfMeasure, @RatePerUnit, @IsActive);", rate);
        }

        public void Update(PakyawRate rate)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            conn.Execute(@"
                UPDATE PakyawRates SET
                    TaskCode = @TaskCode, TaskName = @TaskName,
                    UnitOfMeasure = @UnitOfMeasure, RatePerUnit = @RatePerUnit,
                    IsActive = @IsActive
                WHERE Id = @Id;", rate);
        }

        public void SetActive(int id, bool isActive)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            conn.Execute("UPDATE PakyawRates SET IsActive = @IsActive WHERE Id = @Id;",
                new { IsActive = isActive, Id = id });
        }
    }
}
