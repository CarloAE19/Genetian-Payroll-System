using System;

namespace GB_Payroll_System.Models
{
    public class StatutorySettings
    {
        public int Id { get; set; } = 1;

        // SSS (Social Security System)
        public decimal SssTotalRatePercent { get; set; } = 14.00m;
        public decimal SssEmployeeSharePercent { get; set; } = 4.50m;
        public decimal SssEmployerSharePercent { get; set; } = 9.50m;
        public decimal SssMinSalaryCredit { get; set; } = 5000.00m;
        public decimal SssMaxSalaryCredit { get; set; } = 35000.00m;

        // PhilHealth
        public decimal PhilHealthTotalRatePercent { get; set; } = 5.00m;
        public decimal PhilHealthEmployeeSharePercent { get; set; } = 2.50m;
        public decimal PhilHealthEmployerSharePercent { get; set; } = 2.50m;
        public decimal PhilHealthMinSalaryCredit { get; set; } = 10000.00m;
        public decimal PhilHealthMaxSalaryCredit { get; set; } = 100000.00m;

        // Pag-IBIG (HDMF)
        public decimal PagIbigEmployeeStandardMonthly { get; set; } = 200.00m;
        public decimal PagIbigEmployerStandardMonthly { get; set; } = 200.00m;

        // BIR Withholding Tax & Exemptions
        public decimal BirSemiMonthlyExemptCeiling { get; set; } = 10417.00m; // ₱250,000 / 24 cutoffs
        public decimal BirBonusExemptCap { get; set; } = 90000.00m;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string UpdatedByUsername { get; set; } = "system";
    }
}
