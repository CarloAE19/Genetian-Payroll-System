using System;

namespace GB_Payroll_System.Models
{
    public enum AttendanceStatus
    {
        Present = 1,
        Absent = 2,
        OnLeave = 3,
        Holiday = 4,
        HalfDay = 5,
        AWOL = 6
    }

    public class Attendance
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }

        public TimeSpan? TimeIn { get; set; }
        public TimeSpan? TimeOut { get; set; }

        public double LateMinutes { get; set; }
        public double UndertimeMinutes { get; set; }
        
        public double RegularHoursWorked { get; set; }
        public double OvertimeHours { get; set; }
        public double NightDiffHours { get; set; }

        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
        public int? HolidayId { get; set; }

        public bool IsManuallyAdjusted { get; set; }
        public string AdjustmentReason { get; set; } = string.Empty;
        public string AdjustedByUsername { get; set; } = string.Empty;
    }
}
