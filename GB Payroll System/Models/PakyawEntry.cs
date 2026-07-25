using System;

namespace GB_Payroll_System.Models
{
    public class PakyawEntry
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int PakyawRateId { get; set; }
        
        public DateTime WorkDate { get; set; }
        public decimal QuantityCompleted { get; set; }
        public decimal UnitRate { get; set; } // Historical rate snapshot
        public decimal TotalEarnings => QuantityCompleted * UnitRate;

        public string Remarks { get; set; } = string.Empty;
        public string RecordedByUsername { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
