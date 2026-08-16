using System;
using System.Collections.Generic;
using Dapper;
using GB_Payroll_System.Models;

namespace GB_Payroll_System.Data
{
    // A flat view model joining Attendance + Employee for display in the grid
    public class AttendanceViewModel
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public TimeSpan? TimeIn { get; set; }
        public TimeSpan? TimeOut { get; set; }
        public double LateMinutes { get; set; }
        public double UndertimeMinutes { get; set; }
        public double RegularHoursWorked { get; set; }
        public double OvertimeHours { get; set; }
        public double NightDiffHours { get; set; }
        public AttendanceStatus Status { get; set; }
        public bool IsManuallyAdjusted { get; set; }
        public string AdjustmentReason { get; set; } = string.Empty;
        public string AdjustedByUsername { get; set; } = string.Empty;

        // Display helpers
        public string TimeInDisplay => TimeIn.HasValue ? DateTime.Today.Add(TimeIn.Value).ToString("hh:mm tt") : "—";
        public string TimeOutDisplay => TimeOut.HasValue ? DateTime.Today.Add(TimeOut.Value).ToString("hh:mm tt") : "—";
        public string LateDisplay => LateMinutes > 0 ? $"{(int)LateMinutes} min" : "—";
        public string OTDisplay => OvertimeHours > 0 ? $"{OvertimeHours:F2} hrs" : "—";
        public string StatusDisplay => Status switch
        {
            AttendanceStatus.Present => "Present",
            AttendanceStatus.Absent => "Absent",
            AttendanceStatus.OnLeave => "On Leave",
            AttendanceStatus.Holiday => "Holiday",
            AttendanceStatus.HalfDay => "Half Day",
            _ => "—"
        };
    }

    public class AttendanceRepository
    {
        public List<AttendanceViewModel> GetByDateRange(DateTime from, DateTime to, int? employeeId = null)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();

            string filter = employeeId.HasValue ? "AND a.EmployeeId = @EmpId" : "";
            string sql = $@"
                SELECT 
                    a.Id, a.EmployeeId, e.EmployeeCode,
                    CONCAT(e.FirstName, ' ', e.LastName) AS FullName,
                    e.Department, a.Date, a.TimeIn, a.TimeOut,
                    a.LateMinutes, a.UndertimeMinutes, a.RegularHoursWorked,
                    a.OvertimeHours, a.NightDiffHours, a.Status,
                    a.IsManuallyAdjusted, a.AdjustmentReason, a.AdjustedByUsername
                FROM Attendances a
                JOIN Employees e ON e.Id = a.EmployeeId
                WHERE a.Date BETWEEN @From AND @To {filter}
                ORDER BY a.Date DESC, e.LastName, e.FirstName;";

            return [.. conn.Query<AttendanceViewModel>(sql, new { From = from, To = to, EmpId = employeeId })];
        }

        public Attendance? GetById(int id)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            return conn.QueryFirstOrDefault<Attendance>("SELECT * FROM Attendances WHERE Id = @Id;", new { Id = id });
        }

        public void Insert(Attendance att)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            string sql = @"
                INSERT INTO Attendances
                    (EmployeeId, Date, TimeIn, TimeOut, LateMinutes, UndertimeMinutes,
                     RegularHoursWorked, OvertimeHours, NightDiffHours, Status,
                     IsManuallyAdjusted, AdjustmentReason, AdjustedByUsername)
                VALUES
                    (@EmployeeId, @Date, @TimeIn, @TimeOut, @LateMinutes, @UndertimeMinutes,
                     @RegularHoursWorked, @OvertimeHours, @NightDiffHours, @Status,
                     @IsManuallyAdjusted, @AdjustmentReason, @AdjustedByUsername);";
            conn.Execute(sql, att);
        }

        public void Update(Attendance att)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            string sql = @"
                UPDATE Attendances SET
                    TimeIn = @TimeIn, TimeOut = @TimeOut,
                    LateMinutes = @LateMinutes, UndertimeMinutes = @UndertimeMinutes,
                    RegularHoursWorked = @RegularHoursWorked, OvertimeHours = @OvertimeHours,
                    NightDiffHours = @NightDiffHours, Status = @Status,
                    IsManuallyAdjusted = @IsManuallyAdjusted,
                    AdjustmentReason = @AdjustmentReason, AdjustedByUsername = @AdjustedByUsername
                WHERE Id = @Id;";
            conn.Execute(sql, att);
        }

        public int BulkInsert(List<Attendance> records)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            int inserted = 0;
            foreach (var att in records)
            {
                // Skip duplicates (same employee + date)
                int exists = conn.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Attendances WHERE EmployeeId = @EmployeeId AND Date = @Date;",
                    new { att.EmployeeId, att.Date });

                if (exists == 0)
                {
                    Insert(att);
                    inserted++;
                }
            }
            return inserted;
        }
    }
}
