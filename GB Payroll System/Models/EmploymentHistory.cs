using System;

namespace GB_Payroll_System.Models
{
    public class EmploymentHistory
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }

        public string CompanyName { get; set; } = "Genetian"; // Company / Employer Name
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;

        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime? EndDate { get; set; } // Null if currently active/re-employed

        public string EmploymentType { get; set; } = "Regular"; // Regular, Contractual, Pakyawan, Consultant / Post-Retirement
        public string SeparationType { get; set; } = "Active";  // Retired, Resigned, End of Contract, Rehired / Re-employed, Active
        public string SeparationReason { get; set; } = string.Empty; // Remarks / Reason for Separation or Rehire

        public bool IsRehireEligible { get; set; } = true;
        public string RecordedByUsername { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // UI Helpers
        public string PeriodDisplay => EndDate.HasValue
            ? $"{StartDate:MMM dd, yyyy} – {EndDate.Value:MMM dd, yyyy}"
            : $"{StartDate:MMM dd, yyyy} – Present (Active/Rehired)";

        public string StatusDisplay => string.IsNullOrWhiteSpace(SeparationType) ? "Active" : SeparationType;
    }
}
