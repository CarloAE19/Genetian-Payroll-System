using System;
using System.Collections.Generic;
using Dapper;
using GB_Payroll_System.Models;

namespace GB_Payroll_System.Data
{
    public class LeaveRepository
    {
        public List<LeaveApplicationViewModel> GetApplications(int? year = null, LeaveStatus? status = null, int? employeeId = null)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();

            var conditions = new List<string>();
            if (year.HasValue && year.Value > 0) conditions.Add("EXTRACT(YEAR FROM la.StartDate) = @Year");
            if (status.HasValue) conditions.Add("la.Status = @Status");
            if (employeeId.HasValue && employeeId.Value > 0) conditions.Add("la.EmployeeId = @EmpId");

            string whereStr = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";

            string sql = $@"
                SELECT 
                    la.Id, la.EmployeeId, e.EmployeeCode,
                    CONCAT(e.LastName, ', ', e.FirstName) AS FullName,
                    e.Department, la.Type, la.StartDate, la.EndDate,
                    la.DaysCount, la.Reason, la.Status, la.ApprovedByUsername,
                    la.ApprovedAt, la.CreatedAt
                FROM LeaveApplications la
                JOIN Employees e ON e.Id = la.EmployeeId
                {whereStr}
                ORDER BY la.CreatedAt DESC;";

            return [.. conn.Query<LeaveApplicationViewModel>(sql, new { Year = year, Status = status, EmpId = employeeId })];
        }

        public List<LeaveBalanceViewModel> GetBalances(int year, int? employeeId = null)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();

            string empFilter = employeeId.HasValue && employeeId.Value > 0 ? "AND e.Id = @EmpId" : "";
            string sql = $@"
                SELECT 
                    COALESCE(lb.Id, 0) AS Id,
                    e.Id AS EmployeeId, e.EmployeeCode,
                    CONCAT(e.LastName, ', ', e.FirstName) AS FullName,
                    e.Department,
                    @Year AS Year,
                    COALESCE(lb.VacationLeaveTotal, 15.00) AS VacationLeaveTotal,
                    COALESCE(lb.VacationLeaveUsed, 0.00) AS VacationLeaveUsed,
                    COALESCE(lb.SickLeaveTotal, 15.00) AS SickLeaveTotal,
                    COALESCE(lb.SickLeaveUsed, 0.00) AS SickLeaveUsed,
                    COALESCE(lb.EmergencyLeaveTotal, 5.00) AS EmergencyLeaveTotal,
                    COALESCE(lb.EmergencyLeaveUsed, 0.00) AS EmergencyLeaveUsed
                FROM Employees e
                LEFT JOIN EmployeeLeaveBalances lb ON lb.EmployeeId = e.Id AND lb.Year = @Year
                WHERE e.IsActive = TRUE {empFilter}
                ORDER BY e.LastName, e.FirstName;";

            return [.. conn.Query<LeaveBalanceViewModel>(sql, new { Year = year, EmpId = employeeId })];
        }

        public void InsertApplication(LeaveApplication app)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            conn.Execute(@"
                INSERT INTO LeaveApplications
                    (EmployeeId, Type, StartDate, EndDate, DaysCount, Reason, Status, ApprovedByUsername, CreatedAt)
                VALUES
                    (@EmployeeId, @Type, @StartDate, @EndDate, @DaysCount, @Reason, @Status, @ApprovedByUsername, @CreatedAt);",
                app);
        }

        public void UpdateApplicationStatus(int applicationId, LeaveStatus newStatus, string username)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();

            var app = conn.QueryFirstOrDefault<LeaveApplication>("SELECT * FROM LeaveApplications WHERE Id = @Id;", new { Id = applicationId });
            if (app == null) return;

            LeaveStatus oldStatus = app.Status;

            conn.Execute(@"
                UPDATE LeaveApplications SET
                    Status = @Status, ApprovedByUsername = @User, ApprovedAt = CURRENT_TIMESTAMP
                WHERE Id = @Id;", new { Status = newStatus, User = username, Id = applicationId });

            // If status changed to Approved, update EmployeeLeaveBalance used credits
            if (newStatus == LeaveStatus.Approved && oldStatus != LeaveStatus.Approved)
            {
                DeductLeaveCredits(conn, app);
            }
            // If status changed from Approved to Rejected/Cancelled, restore leave credits
            else if (oldStatus == LeaveStatus.Approved && (newStatus == LeaveStatus.Rejected || newStatus == LeaveStatus.Cancelled))
            {
                RestoreLeaveCredits(conn, app);
            }
        }

        private static void DeductLeaveCredits(System.Data.IDbConnection conn, LeaveApplication app)
        {
            int year = app.StartDate.Year;
            EnsureBalanceRecordExists(conn, app.EmployeeId, year);

            string columnToDeduct = app.Type switch
            {
                LeaveType.VacationLeave => "VacationLeaveUsed",
                LeaveType.SickLeave     => "SickLeaveUsed",
                LeaveType.EmergencyLeave => "EmergencyLeaveUsed",
                _ => ""
            };

            if (!string.IsNullOrEmpty(columnToDeduct))
            {
                conn.Execute($@"
                    UPDATE EmployeeLeaveBalances SET
                        {columnToDeduct} = {columnToDeduct} + @Days,
                        UpdatedAt = CURRENT_TIMESTAMP
                    WHERE EmployeeId = @EmpId AND Year = @Year;",
                    new { Days = app.DaysCount, EmpId = app.EmployeeId, Year = year });
            }
        }

        private static void RestoreLeaveCredits(System.Data.IDbConnection conn, LeaveApplication app)
        {
            int year = app.StartDate.Year;
            EnsureBalanceRecordExists(conn, app.EmployeeId, year);

            string columnToRestore = app.Type switch
            {
                LeaveType.VacationLeave => "VacationLeaveUsed",
                LeaveType.SickLeave     => "SickLeaveUsed",
                LeaveType.EmergencyLeave => "EmergencyLeaveUsed",
                _ => ""
            };

            if (!string.IsNullOrEmpty(columnToRestore))
            {
                conn.Execute($@"
                    UPDATE EmployeeLeaveBalances SET
                        {columnToRestore} = GREATEST(0, {columnToRestore} - @Days),
                        UpdatedAt = CURRENT_TIMESTAMP
                    WHERE EmployeeId = @EmpId AND Year = @Year;",
                    new { Days = app.DaysCount, EmpId = app.EmployeeId, Year = year });
            }
        }

        public static void EnsureBalanceRecordExists(System.Data.IDbConnection conn, int employeeId, int year)
        {
            int count = conn.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM EmployeeLeaveBalances WHERE EmployeeId = @EmpId AND Year = @Year;",
                new { EmpId = employeeId, Year = year });

            if (count == 0)
            {
                conn.Execute(@"
                    INSERT INTO EmployeeLeaveBalances (EmployeeId, Year, VacationLeaveTotal, VacationLeaveUsed, SickLeaveTotal, SickLeaveUsed, EmergencyLeaveTotal, EmergencyLeaveUsed, UpdatedAt)
                    VALUES (@EmpId, @Year, 15.00, 0.00, 15.00, 0.00, 5.00, 0.00, CURRENT_TIMESTAMP);",
                    new { EmpId = employeeId, Year = year });
            }
        }
    }
}
