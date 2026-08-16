using System;
using System.Collections.Generic;
using Dapper;
using GB_Payroll_System.Models;

namespace GB_Payroll_System.Data
{
    public class SssReportRow
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string SssNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public decimal BasicPay { get; set; }
        public decimal GrossPay { get; set; }
        public decimal EmployeeShare { get; set; }
        public decimal EmployerShare { get; set; }
        public decimal EcContribution => 10.00m; // Standard Mandatory Mandatory EC (Ec Share ₱10 or ₱30 depending on MSC)
        public decimal TotalContribution => EmployeeShare + EmployerShare + EcContribution;
    }

    public class PhilHealthReportRow
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string PhilHealthNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public decimal BasicPay { get; set; }
        public decimal EmployeeShare { get; set; }
        public decimal EmployerShare { get; set; }
        public decimal TotalContribution => EmployeeShare + EmployerShare;
    }

    public class PagIbigReportRow
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string PagIbigNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public decimal EmployeeShare { get; set; }
        public decimal EmployerShare { get; set; }
        public decimal TotalContribution => EmployeeShare + EmployerShare;
    }

    public class BirTaxReportRow
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string TinNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public decimal GrossPay { get; set; }
        public decimal StatutoryDeductions { get; set; } // SSS + PH + HDMF employee share
        public decimal TaxableIncome { get; set; }
        public decimal TaxWithheld { get; set; }
    }

    public class GovernmentReportRepository
    {
        public List<SssReportRow> GetSssReport(int? periodId = null, int? year = null, int? month = null)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();

            string whereClause = BuildWhereClause(periodId, year, month);
            string sql = $@"
                SELECT 
                    e.EmployeeCode,
                    COALESCE(e.SssNumber, '—') AS SssNumber,
                    CONCAT(e.LastName, ', ', e.FirstName) AS FullName,
                    SUM(pr.BasicPay) AS BasicPay,
                    SUM(pr.BasicPay + pr.PakyawPay + pr.OvertimePay + pr.NightDiffPay + pr.HolidayPay + pr.Allowances) AS GrossPay,
                    SUM(pr.SssEmployee) AS EmployeeShare,
                    SUM(pr.SssEmployer) AS EmployerShare
                FROM PayrollRecords pr
                JOIN Employees e ON e.Id = pr.EmployeeId
                JOIN PayrollPeriods pp ON pp.Id = pr.PayrollPeriodId
                WHERE {whereClause}
                GROUP BY e.Id, e.EmployeeCode, e.SssNumber, e.LastName, e.FirstName
                ORDER BY e.LastName, e.FirstName;";

            return [.. conn.Query<SssReportRow>(sql, new { PeriodId = periodId, Year = year, Month = month })];
        }

        public List<PhilHealthReportRow> GetPhilHealthReport(int? periodId = null, int? year = null, int? month = null)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();

            string whereClause = BuildWhereClause(periodId, year, month);
            string sql = $@"
                SELECT 
                    e.EmployeeCode,
                    COALESCE(e.PhilHealthNumber, '—') AS PhilHealthNumber,
                    CONCAT(e.LastName, ', ', e.FirstName) AS FullName,
                    SUM(pr.BasicPay) AS BasicPay,
                    SUM(pr.PhilHealthEmployee) AS EmployeeShare,
                    SUM(pr.PhilHealthEmployer) AS EmployerShare
                FROM PayrollRecords pr
                JOIN Employees e ON e.Id = pr.EmployeeId
                JOIN PayrollPeriods pp ON pp.Id = pr.PayrollPeriodId
                WHERE {whereClause}
                GROUP BY e.Id, e.EmployeeCode, e.PhilHealthNumber, e.LastName, e.FirstName
                ORDER BY e.LastName, e.FirstName;";

            return [.. conn.Query<PhilHealthReportRow>(sql, new { PeriodId = periodId, Year = year, Month = month })];
        }

        public List<PagIbigReportRow> GetPagIbigReport(int? periodId = null, int? year = null, int? month = null)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();

            string whereClause = BuildWhereClause(periodId, year, month);
            string sql = $@"
                SELECT 
                    e.EmployeeCode,
                    COALESCE(e.PagIbigNumber, '—') AS PagIbigNumber,
                    CONCAT(e.LastName, ', ', e.FirstName) AS FullName,
                    SUM(pr.PagIbigEmployee) AS EmployeeShare,
                    SUM(pr.PagIbigEmployer) AS EmployerShare
                FROM PayrollRecords pr
                JOIN Employees e ON e.Id = pr.EmployeeId
                JOIN PayrollPeriods pp ON pp.Id = pr.PayrollPeriodId
                WHERE {whereClause}
                GROUP BY e.Id, e.EmployeeCode, e.PagIbigNumber, e.LastName, e.FirstName
                ORDER BY e.LastName, e.FirstName;";

            return [.. conn.Query<PagIbigReportRow>(sql, new { PeriodId = periodId, Year = year, Month = month })];
        }

        public List<BirTaxReportRow> GetBirTaxReport(int? periodId = null, int? year = null, int? month = null)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();

            string whereClause = BuildWhereClause(periodId, year, month);
            string sql = $@"
                SELECT 
                    e.EmployeeCode,
                    COALESCE(e.TinNumber, '—') AS TinNumber,
                    CONCAT(e.LastName, ', ', e.FirstName) AS FullName,
                    SUM(pr.BasicPay + pr.PakyawPay + pr.OvertimePay + pr.NightDiffPay + pr.HolidayPay + pr.Allowances) AS GrossPay,
                    SUM(pr.SssEmployee + pr.PhilHealthEmployee + pr.PagIbigEmployee) AS StatutoryDeductions,
                    SUM((pr.BasicPay + pr.PakyawPay + pr.OvertimePay + pr.NightDiffPay + pr.HolidayPay + pr.Allowances) 
                        - (pr.SssEmployee + pr.PhilHealthEmployee + pr.PagIbigEmployee)) AS TaxableIncome,
                    SUM(pr.WithholdingTax) AS TaxWithheld
                FROM PayrollRecords pr
                JOIN Employees e ON e.Id = pr.EmployeeId
                JOIN PayrollPeriods pp ON pp.Id = pr.PayrollPeriodId
                WHERE {whereClause}
                GROUP BY e.Id, e.EmployeeCode, e.TinNumber, e.LastName, e.FirstName
                ORDER BY e.LastName, e.FirstName;";

            return [.. conn.Query<BirTaxReportRow>(sql, new { PeriodId = periodId, Year = year, Month = month })];
        }

        private static string BuildWhereClause(int? periodId, int? year, int? month)
        {
            if (periodId.HasValue && periodId.Value > 0)
                return "pr.PayrollPeriodId = @PeriodId";

            if (year.HasValue && month.HasValue)
                return "EXTRACT(YEAR FROM pp.StartDate) = @Year AND EXTRACT(MONTH FROM pp.StartDate) = @Month";

            return "1=1";
        }
    }
}
