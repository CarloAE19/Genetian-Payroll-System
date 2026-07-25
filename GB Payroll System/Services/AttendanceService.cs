using System;
using GB_Payroll_System.Models;

namespace GB_Payroll_System.Services
{
    public class AttendanceService
    {
        public static (double LateMins, double UndertimeMins, double RegularHrs, double OtHrs, double NightDiffHrs) 
            CalculateShiftHours(TimeSpan timeIn, TimeSpan timeOut, TimeSpan shiftStart, TimeSpan shiftEnd)
        {
            double lateMins = 0;
            double undertimeMins = 0;
            double regularHrs = 0;
            double otHrs = 0;
            double nightDiffHrs = 0;

            // Late calculation
            if (timeIn > shiftStart)
            {
                lateMins = (timeIn - shiftStart).TotalMinutes;
            }

            // Undertime calculation
            if (timeOut < shiftEnd)
            {
                undertimeMins = (shiftEnd - timeOut).TotalMinutes;
            }

            // Total hours worked
            double totalWorked = (timeOut - timeIn).TotalHours - 1.0; // Subtract 1 hr lunch break
            if (totalWorked < 0) totalWorked = 0;

            double expectedShiftHrs = (shiftEnd - shiftStart).TotalHours - 1.0;

            if (totalWorked <= expectedShiftHrs)
            {
                regularHrs = totalWorked;
            }
            else
            {
                regularHrs = expectedShiftHrs;
                otHrs = totalWorked - expectedShiftHrs;
            }

            // Simple Night Diff check (10 PM to 6 AM)
            TimeSpan nightStart = new TimeSpan(22, 0, 0); // 10 PM
            TimeSpan nightEnd = new TimeSpan(6, 0, 0);   // 6 AM

            if (timeOut > nightStart)
            {
                nightDiffHrs = (timeOut - nightStart).TotalHours;
            }

            return (Math.Max(0, lateMins), Math.Max(0, undertimeMins), Math.Max(0, regularHrs), Math.Max(0, otHrs), Math.Max(0, nightDiffHrs));
        }

        public static decimal GetDailyRate(Employee emp)
        {
            if (emp.PayType == PayType.Daily)
            {
                return emp.BasicRate;
            }

            if (emp.PayType == PayType.Monthly)
            {
                decimal factor = emp.WorkingDaysFactor > 0 ? emp.WorkingDaysFactor : 313m;
                return Math.Round((emp.BasicRate * 12m) / factor, 2);
            }

            return emp.BasicRate;
        }

        public static decimal GetHourlyRate(Employee emp)
        {
            decimal dailyRate = GetDailyRate(emp);
            return Math.Round(dailyRate / 8m, 2); // 8 working hours per day
        }
    }
}
