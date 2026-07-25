using System;

namespace GB_Payroll_System.Models
{
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

        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public int? BranchId { get; set; }

        public PayType PayType { get; set; } = PayType.Monthly;
        public decimal BasicRate { get; set; } // Monthly salary or Daily rate or Base Pakyaw rate
        public decimal WorkingDaysFactor { get; set; } = 313m; // 261, 313, 365 days factor for DOLE daily rate calculation

        // Government Mandated IDs
        public string SssNumber { get; set; } = string.Empty;
        public string PhilHealthNumber { get; set; } = string.Empty;
        public string PagIbigNumber { get; set; } = string.Empty;
        public string TinNumber { get; set; } = string.Empty;

        // Status & Dates
        public DateTime DateHired { get; set; } = DateTime.Today;
        public bool IsActive { get; set; } = true;
        public string BankAccountNumber { get; set; } = string.Empty;
    }
}
