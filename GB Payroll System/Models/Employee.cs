using System;

namespace GB_Payroll_System.Models
{
    public enum DeductionMode
    {
        Auto = 1,
        FixedAmount = 2,
        Exempt = 3
    }

    public enum ContributionSchedule
    {
        SplitBothCutoffs = 1, // 50% on 15th, 50% on 30th
        FirstCutoffOnly = 2,  // 100% on 15th
        SecondCutoffOnly = 3  // 100% on 30th
    }

    public class Employee
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty; // e.g. EMP-2026-001
        public string BiometricUserId { get; set; } = string.Empty; // ID mapped in Biometric Device

        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => string.IsNullOrWhiteSpace(MiddleName) 
            ? $"{FirstName} {LastName}" 
            : $"{FirstName} {MiddleName[0]}. {LastName}";

        // 201 Personal Details
        public DateTime? BirthDate { get; set; }
        public string Gender { get; set; } = "Male";
        public string CivilStatus { get; set; } = "Single";
        public string ContactNumber { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string EmergencyContactName { get; set; } = string.Empty;
        public string EmergencyContactPhone { get; set; } = string.Empty;

        // Employment & Contract Details
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public int? BranchId { get; set; }

        public ContractType ContractType { get; set; } = ContractType.Probationary;
        public ContractStatus ContractStatus { get; set; } = ContractStatus.Active;
        public DateTime? ContractEndDate { get; set; }

        public PayType PayType { get; set; } = PayType.Monthly;
        public decimal BasicRate { get; set; } // Monthly salary or Daily rate
        public decimal WorkingDaysFactor { get; set; } = 313m; // 261, 313, 365 days factor for DOLE daily rate calculation

        // Government Mandated IDs
        public string SssNumber { get; set; } = string.Empty;
        public string PhilHealthNumber { get; set; } = string.Empty;
        public string PagIbigNumber { get; set; } = string.Empty;
        public string TinNumber { get; set; } = string.Empty;

        // Per-Employee Custom Contribution Rules
        public DeductionMode SssDeductionMode { get; set; } = DeductionMode.Auto;
        public decimal CustomSssAmount { get; set; } // Used if SssDeductionMode == FixedAmount

        public DeductionMode PhilHealthDeductionMode { get; set; } = DeductionMode.Auto;
        public decimal CustomPhilHealthAmount { get; set; } // Used if PhilHealthDeductionMode == FixedAmount

        public DeductionMode PagIbigDeductionMode { get; set; } = DeductionMode.Auto;
        public decimal PagIbigEmployeeAmount { get; set; } = 200m; // Default statutory minimum (can be voluntary 500, 1000, etc.)

        public bool IsMinimumWageEarner { get; set; } = false; // If true, Tax Exempt from BIR Withholding
        public bool IsTaxExempt { get; set; } = false;

        public ContributionSchedule ContributionSchedule { get; set; } = ContributionSchedule.SplitBothCutoffs;

        // Status & Dates
        public DateTime DateHired { get; set; } = DateTime.Today;
        public bool IsActive { get; set; } = true;
        public string BankAccountNumber { get; set; } = string.Empty;

        // Contract Expiration Helper
        public bool HasExpiringContract => ContractStatus == ContractStatus.Active &&
                                           ContractEndDate.HasValue &&
                                           ContractEndDate.Value >= DateTime.Today &&
                                           ContractEndDate.Value <= DateTime.Today.AddDays(30);
    }
}
