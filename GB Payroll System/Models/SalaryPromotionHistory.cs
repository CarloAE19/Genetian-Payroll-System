using System;

namespace GB_Payroll_System.Models
{
    public class SalaryPromotionHistory
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        
        // Joined Employee Details
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeFullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;

        public string PreviousPosition { get; set; } = string.Empty;
        public string NewPosition { get; set; } = string.Empty;

        public decimal PreviousRate { get; set; }
        public decimal NewRate { get; set; }

        public DateTime EffectiveDate { get; set; }
        public string Reason { get; set; } = string.Empty;

        public string ApprovedByUsername { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Display & Calculation Helpers
        public decimal RateIncreaseAmount => NewRate - PreviousRate;

        public decimal IncreasePercentage => PreviousRate > 0
            ? ((NewRate - PreviousRate) / PreviousRate) * 100m
            : 0m;

        public string IncreaseBadgeDisplay => RateIncreaseAmount >= 0
            ? $"+₱{RateIncreaseAmount:N2} (+{IncreasePercentage:F1}%)"
            : $"-₱{Math.Abs(RateIncreaseAmount):N2} ({IncreasePercentage:F1}%)";

        public string PositionProgressionDisplay => string.Equals(PreviousPosition, NewPosition, StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(PreviousPosition)
            ? NewPosition
            : $"{PreviousPosition}  ➔  {NewPosition}";
    }
}

