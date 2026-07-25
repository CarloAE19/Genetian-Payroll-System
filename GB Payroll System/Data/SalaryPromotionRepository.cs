using System;
using System.Collections.Generic;
using Dapper;
using GB_Payroll_System.Models;

namespace GB_Payroll_System.Data
{
    public class SalaryPromotionRepository
    {
        public List<SalaryPromotionHistory> GetByEmployee(int employeeId)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            return [.. conn.Query<SalaryPromotionHistory>(
                "SELECT * FROM SalaryPromotionHistories WHERE EmployeeId = @Id ORDER BY EffectiveDate DESC;",
                new { Id = employeeId })];
        }

        public void Insert(SalaryPromotionHistory record)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            string sql = @"
                INSERT INTO SalaryPromotionHistories
                    (EmployeeId, PreviousPosition, NewPosition, PreviousRate, NewRate,
                     EffectiveDate, Reason, ApprovedByUsername)
                VALUES
                    (@EmployeeId, @PreviousPosition, @NewPosition, @PreviousRate, @NewRate,
                     @EffectiveDate, @Reason, @ApprovedByUsername);";
            conn.Execute(sql, record);
        }
    }
}
