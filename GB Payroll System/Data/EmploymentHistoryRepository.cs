using System;
using System.Collections.Generic;
using Dapper;
using GB_Payroll_System.Models;

namespace GB_Payroll_System.Data
{
    public class EmploymentHistoryRepository
    {
        public List<EmploymentHistory> GetByEmployee(int employeeId)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            return [.. conn.Query<EmploymentHistory>(
                "SELECT * FROM EmploymentHistories WHERE EmployeeId = @EmployeeId ORDER BY StartDate DESC, Id DESC;",
                new { EmployeeId = employeeId })];
        }

        public int Insert(EmploymentHistory history)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            string sql = @"
                INSERT INTO EmploymentHistories
                    (EmployeeId, CompanyName, Department, Position, StartDate, EndDate,
                     EmploymentType, SeparationType, SeparationReason, IsRehireEligible,
                     RecordedByUsername)
                VALUES
                    (@EmployeeId, @CompanyName, @Department, @Position, @StartDate, @EndDate,
                     (@EmploymentType), @SeparationType, @SeparationReason, @IsRehireEligible,
                     @RecordedByUsername)
                RETURNING Id;";
            return conn.ExecuteScalar<int>(sql, history);
        }

        public void Update(EmploymentHistory history)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            string sql = @"
                UPDATE EmploymentHistories SET
                    CompanyName = @CompanyName,
                    Department = @Department,
                    Position = @Position,
                    StartDate = @StartDate,
                    EndDate = @EndDate,
                    EmploymentType = @EmploymentType,
                    SeparationType = @SeparationType,
                    SeparationReason = @SeparationReason,
                    IsRehireEligible = @IsRehireEligible,
                    RecordedByUsername = @RecordedByUsername
                WHERE Id = @Id;";
            conn.Execute(sql, history);
        }

        public void Delete(int id)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            conn.Execute("DELETE FROM EmploymentHistories WHERE Id = @Id;", new { Id = id });
        }
    }
}
