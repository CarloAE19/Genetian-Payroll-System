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

        public List<SalaryPromotionHistory> GetAllWithEmployeeDetails()
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            string sql = @"
                SELECT 
                    s.Id,
                    s.EmployeeId,
                    e.EmployeeCode,
                    CONCAT(e.FirstName, ' ', CASE WHEN e.MiddleName IS NOT NULL AND e.MiddleName != '' THEN CONCAT(SUBSTRING(e.MiddleName, 1, 1), '. ') ELSE '' END, e.LastName) AS EmployeeFullName,
                    e.Department,
                    s.PreviousPosition,
                    s.NewPosition,
                    s.PreviousRate,
                    s.NewRate,
                    s.EffectiveDate,
                    s.Reason,
                    s.ApprovedByUsername,
                    s.CreatedAt
                FROM SalaryPromotionHistories s
                JOIN Employees e ON s.EmployeeId = e.Id
                ORDER BY s.EffectiveDate DESC, s.CreatedAt DESC;";
            return [.. conn.Query<SalaryPromotionHistory>(sql)];
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
