using System;

namespace GB_Payroll_System.Models
{
    public class PayrollPeriod
    {
        public int Id { get; set; }
        public string PeriodCode { get; set; } = string.Empty; // e.g. 2026-07-1
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime PayoutDate { get; set; }
        public bool IsClosed { get; set; }
        public string ProcessedByUsername { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
