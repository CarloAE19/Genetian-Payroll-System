using System;
using System.Collections.Generic;
using Dapper;
using GB_Payroll_System.Models;

namespace GB_Payroll_System.Data
{
    // Flat view model joining PakyawEntry + Employee + PakyawRate for the output grid
    public class PakyawEntryViewModel
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int PakyawRateId { get; set; }
        public string TaskCode { get; set; } = string.Empty;
        public string TaskName { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;
        public DateTime WorkDate { get; set; }
        public decimal QuantityCompleted { get; set; }
        public decimal UnitRate { get; set; }
        public decimal TotalEarnings => QuantityCompleted * UnitRate;
        public string Remarks { get; set; } = string.Empty;
        public string RecordedByUsername { get; set; } = string.Empty;
    }

    public class PakyawEntryRepository
    {
        public List<PakyawEntryViewModel> GetByDateRange(DateTime from, DateTime to, int? employeeId = null)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            string filter = employeeId.HasValue ? "AND pe.EmployeeId = @EmpId" : "";
            string sql = $@"
                SELECT
                    pe.Id, pe.EmployeeId, e.EmployeeCode,
                    CONCAT(e.FirstName, ' ', e.LastName) AS FullName,
                    e.Department, pe.PakyawRateId,
                    pr.TaskCode, pr.TaskName, pr.UnitOfMeasure,
                    pe.WorkDate, pe.QuantityCompleted, pe.UnitRate,
                    pe.Remarks, pe.RecordedByUsername
                FROM PakyawEntries pe
                JOIN Employees e  ON e.Id  = pe.EmployeeId
                JOIN PakyawRates pr ON pr.Id = pe.PakyawRateId
                WHERE pe.WorkDate BETWEEN @From AND @To {filter}
                ORDER BY pe.WorkDate DESC, e.LastName, e.FirstName;";
            return [.. conn.Query<PakyawEntryViewModel>(sql, new { From = from, To = to, EmpId = employeeId })];
        }

        public void Insert(PakyawEntry entry)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            conn.Execute(@"
                INSERT INTO PakyawEntries
                    (EmployeeId, PakyawRateId, WorkDate, QuantityCompleted, UnitRate, Remarks, RecordedByUsername, CreatedAt)
                VALUES
                    (@EmployeeId, @PakyawRateId, @WorkDate, @QuantityCompleted, @UnitRate, @Remarks, @RecordedByUsername, @CreatedAt);",
                entry);
        }

        public void Delete(int id)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            conn.Execute("DELETE FROM PakyawEntries WHERE Id = @Id;", new { Id = id });
        }
    }
}
