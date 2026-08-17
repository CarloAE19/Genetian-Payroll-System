using System;

namespace GB_Payroll_System.Models
{
    public class PayrollRecord
    {
        public int Id { get; set; }
        public int PayrollPeriodId { get; set; }
        public int EmployeeId { get; set; }

        // Earnings
        public decimal BasicPay { get; set; }
        public decimal OvertimePay { get; set; }
        public decimal NightDiffPay { get; set; }
        public decimal HolidayPay { get; set; }
        public decimal Allowances { get; set; }
        public decimal GrossPay => BasicPay + OvertimePay + NightDiffPay + HolidayPay + Allowances;

        // Deductions (DOLE Mandatory)
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

        public DateTime ComputedAt { get; set; } = DateTime.UtcNow;
    }
}
