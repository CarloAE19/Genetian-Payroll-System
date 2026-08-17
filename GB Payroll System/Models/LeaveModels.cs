using System;

namespace GB_Payroll_System.Models
{
    public enum LeaveType
    {
        VacationLeave = 1,   // VL
        SickLeave = 2,       // SL
        EmergencyLeave = 3,  // EL
        MaternityLeave = 4,  // ML
        PaternityLeave = 5,  // PL
        SoloParentLeave = 6, // SPL
        Bereavement = 7      // BL
    }

    public enum LeaveStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Cancelled = 4
    }

    public class EmployeeLeaveBalance
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int Year { get; set; }

        public decimal VacationLeaveTotal { get; set; } = 15.0m;
        public decimal VacationLeaveUsed { get; set; }
        public decimal VacationLeaveBalance => VacationLeaveTotal - VacationLeaveUsed;

        public decimal SickLeaveTotal { get; set; } = 15.0m;
        public decimal SickLeaveUsed { get; set; }
        public decimal SickLeaveBalance => SickLeaveTotal - SickLeaveUsed;

        public decimal EmergencyLeaveTotal { get; set; } = 5.0m;
        public decimal EmergencyLeaveUsed { get; set; }
        public decimal EmergencyLeaveBalance => EmergencyLeaveTotal - EmergencyLeaveUsed;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class LeaveApplication
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public LeaveType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal DaysCount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
        public string ApprovedByUsername { get; set; } = string.Empty;
        public DateTime? ApprovedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Flat ViewModel for Leave Applications Grid
    public class LeaveApplicationViewModel
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;

        public LeaveType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal DaysCount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public LeaveStatus Status { get; set; }
        public string ApprovedByUsername { get; set; } = string.Empty;
        public DateTime? ApprovedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public string TypeDisplay => Type switch
        {
            LeaveType.VacationLeave   => "Vacation Leave (VL)",
            LeaveType.SickLeave       => "Sick Leave (SL)",
            LeaveType.EmergencyLeave  => "Emergency Leave (EL)",
            LeaveType.MaternityLeave  => "Maternity Leave (ML)",
            LeaveType.PaternityLeave  => "Paternity Leave (PL)",
            LeaveType.SoloParentLeave => "Solo Parent Leave",
            LeaveType.Bereavement     => "Bereavement Leave",
            _ => Type.ToString()
        };

        public string DateRangeDisplay => StartDate.Date == EndDate.Date
            ? StartDate.ToString("MMM dd, yyyy")
            : $"{StartDate:MMM dd} – {EndDate:MMM dd, yyyy}";
    }

    // Flat ViewModel for Leave Balances Grid
    public class LeaveBalanceViewModel
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int Year { get; set; }

        public decimal VacationLeaveTotal { get; set; }
        public decimal VacationLeaveUsed { get; set; }
        public decimal VacationLeaveBalance => VacationLeaveTotal - VacationLeaveUsed;

        public decimal SickLeaveTotal { get; set; }
        public decimal SickLeaveUsed { get; set; }
        public decimal SickLeaveBalance => SickLeaveTotal - SickLeaveUsed;

        public decimal EmergencyLeaveTotal { get; set; }
        public decimal EmergencyLeaveUsed { get; set; }
        public decimal EmergencyLeaveBalance => EmergencyLeaveTotal - EmergencyLeaveUsed;
    }
}
