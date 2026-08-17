using System;
using System.Collections.Generic;
using Dapper;
using GB_Payroll_System.Models;

namespace GB_Payroll_System.Data
{
    public class PayrollPeriodRepository
    {
        public List<PayrollPeriod> GetAll()
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            return [.. conn.Query<PayrollPeriod>("SELECT * FROM PayrollPeriods ORDER BY StartDate DESC;")];
        }

        public PayrollPeriod? GetById(int id)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            return conn.QueryFirstOrDefault<PayrollPeriod>("SELECT * FROM PayrollPeriods WHERE Id = @Id;", new { Id = id });
        }

        public int Insert(PayrollPeriod period)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            return conn.ExecuteScalar<int>(@"
                INSERT INTO PayrollPeriods (PeriodCode, StartDate, EndDate, PayoutDate, IsClosed, ProcessedByUsername, CreatedAt)
                VALUES (@PeriodCode, @StartDate, @EndDate, @PayoutDate, @IsClosed, @ProcessedByUsername, @CreatedAt)
                RETURNING Id;", period);
        }

        public void Close(int id, string username)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            conn.Execute("UPDATE PayrollPeriods SET IsClosed = TRUE, ProcessedByUsername = @User WHERE Id = @Id;",
                new { User = username, Id = id });
        }
    }

    public class PayrollRecordRepository
    {
        // Full joined view model for the payroll run grid
        public List<PayrollRunRow> GetByPeriod(int periodId)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            return [.. conn.Query<PayrollRunRow>(@"
                SELECT pr.*, e.EmployeeCode, e.Department,
                       CONCAT(e.FirstName,' ',e.LastName) AS FullName,
                       e.Position, e.PayType
                FROM PayrollRecords pr
                JOIN Employees e ON e.Id = pr.EmployeeId
                WHERE pr.PayrollPeriodId = @PeriodId
                ORDER BY e.LastName, e.FirstName;",
                new { PeriodId = periodId })];
        }

        public void BulkInsert(List<PayrollRecord> records)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            foreach (var r in records)
            {
                conn.Execute(@"
                    INSERT INTO PayrollRecords
                        (PayrollPeriodId, EmployeeId, BasicPay, OvertimePay, NightDiffPay,
                         HolidayPay, Allowances, TardinessDeduction, UndertimeDeduction, AbsenceDeduction,
                         SssEmployee, SssEmployer, PhilHealthEmployee, PhilHealthEmployer,
                         PagIbigEmployee, PagIbigEmployer, WithholdingTax, OtherDeductions, ComputedAt)
                    VALUES
                        (@PayrollPeriodId, @EmployeeId, @BasicPay, @OvertimePay, @NightDiffPay,
                         @HolidayPay, @Allowances, @TardinessDeduction, @UndertimeDeduction, @AbsenceDeduction,
                         @SssEmployee, @SssEmployer, @PhilHealthEmployee, @PhilHealthEmployer,
                         @PagIbigEmployee, @PagIbigEmployer, @WithholdingTax, @OtherDeductions, @ComputedAt);", r);
            }
        }

        public void DeleteByPeriod(int periodId)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            conn.Execute("DELETE FROM PayrollRecords WHERE PayrollPeriodId = @Id;", new { Id = periodId });
        }
    }

    // Flat view model: PayrollRecord + Employee info for the run grid and payslip
    public class PayrollRunRow
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public PayType PayType { get; set; }

        // Earnings
        public decimal BasicPay { get; set; }
        public decimal OvertimePay { get; set; }
        public decimal NightDiffPay { get; set; }
        public decimal HolidayPay { get; set; }
        public decimal Allowances { get; set; }
        public decimal GrossPay => BasicPay + OvertimePay + NightDiffPay + HolidayPay + Allowances;

        // Deductions
        public decimal TardinessDeduction { get; set; }
        public decimal UndertimeDeduction { get; set; }
        public decimal AbsenceDeduction { get; set; }
        public decimal SssEmployee { get; set; }
        public decimal SssEmployer { get; set; }
        public decimal PhilHealthEmployee { get; set; }
        public decimal PhilHealthEmployer { get; set; }
        public decimal PagIbigEmployee { get; set; }
        public decimal PagIbigEmployer { get; set; }
        public decimal WithholdingTax { get; set; }
        public decimal OtherDeductions { get; set; }
        public decimal TotalDeductions => TardinessDeduction + UndertimeDeduction + AbsenceDeduction
                                        + SssEmployee + PhilHealthEmployee + PagIbigEmployee
                                        + WithholdingTax + OtherDeductions;
        public decimal NetPay => GrossPay - TotalDeductions;
    }
}
