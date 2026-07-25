using System;

namespace GB_Payroll_System.Models
{
    public class SalaryPromotionHistory
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        
        public string PreviousPosition { get; set; } = string.Empty;
        public string NewPosition { get; set; } = string.Empty;

        public decimal PreviousRate { get; set; }
        public decimal NewRate { get; set; }

        public DateTime EffectiveDate { get; set; }
        public string Reason { get; set; } = string.Empty;

        public string ApprovedByUsername { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
