using System;

namespace GB_Payroll_System.Models
{
    public enum ContractType
    {
        Regular = 1,
        Probationary = 2,
        FixedTerm = 3,       // Project-based / Fixed-term
        Seasonal = 4,
        Casual = 5
    }

    public enum ContractStatus
    {
        Active = 1,
        Renewed = 2,
        Expired = 3,
        Terminated = 4
    }

    public class EmployeeContract
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public ContractType ContractType { get; set; } = ContractType.Probationary;
        public string PositionTitle { get; set; } = string.Empty;
        public decimal BasicRate { get; set; }
        public PayType PayType { get; set; } = PayType.Monthly;
        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime? EndDate { get; set; }
        public ContractStatus Status { get; set; } = ContractStatus.Active;
        public string? DocumentPath { get; set; }
        public string? Remarks { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // UI Helpers
        public bool IsExpiringSoon => Status == ContractStatus.Active && 
                                      EndDate.HasValue && 
                                      EndDate.Value >= DateTime.Today && 
                                      EndDate.Value <= DateTime.Today.AddDays(30);

        public bool IsExpired => Status == ContractStatus.Active && 
                                 EndDate.HasValue && 
                                 EndDate.Value < DateTime.Today;
    }
}
