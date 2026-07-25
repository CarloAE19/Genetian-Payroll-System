using System;
using System.Collections.Generic;
using GB_Payroll_System.Models;

namespace GB_Payroll_System.Services
{
    public class PayrollService
    {
        public static PayrollRecord ComputePayrollRecord(
            Employee employee, 
            PayrollPeriod period, 
            List<Attendance> attendances, 
            List<PakyawEntry> pakyawEntries)
        {
            var record = new PayrollRecord
            {
                PayrollPeriodId = period.Id,
                EmployeeId = employee.Id
            };

            decimal dailyRate = AttendanceService.GetDailyRate(employee);
            decimal hourlyRate = AttendanceService.GetHourlyRate(employee);
            decimal minuteRate = hourlyRate / 60m;

            // 1. Basic Pay Computation
            if (employee.PayType == PayType.Monthly)
            {
                // Semi-monthly basic pay (50% of monthly basic)
                record.BasicPay = Math.Round(employee.BasicRate / 2m, 2);
            }

            // 2. Pakyawan Pay Computation
            decimal totalPakyaw = 0m;
            foreach (var entry in pakyawEntries)
            {
                totalPakyaw += entry.TotalEarnings;
            }
            record.PakyawPay = totalPakyaw;

            // 3. Attendance, Tardiness, OT, Night Diff, Holiday Pay Calculations
            double totalLateMins = 0;
            double totalUndertimeMins = 0;
            double totalOtHrs = 0;
            double totalNightDiffHrs = 0;
            int presentDaysCount = 0;

            foreach (var att in attendances)
            {
                if (att.Status == AttendanceStatus.Present)
                {
                    presentDaysCount++;
                    totalLateMins += att.LateMinutes;
                    totalUndertimeMins += att.UndertimeMinutes;
                    totalOtHrs += att.OvertimeHours;
                    totalNightDiffHrs += att.NightDiffHours;
                }
            }

            if (employee.PayType == PayType.Daily)
            {
                record.BasicPay = Math.Round(dailyRate * presentDaysCount, 2);
            }

            // OT Pay (125% regular OT premium)
            record.OvertimePay = Math.Round((decimal)totalOtHrs * hourlyRate * 1.25m, 2);

            // Night Diff Pay (110% night diff premium)
            record.NightDiffPay = Math.Round((decimal)totalNightDiffHrs * hourlyRate * 0.10m, 2);

            // Tardiness & Undertime Deductions
            record.TardinessDeduction = Math.Round((decimal)totalLateMins * minuteRate, 2);
            record.UndertimeDeduction = Math.Round((decimal)totalUndertimeMins * minuteRate, 2);

            // Estimated Monthly Equivalent Compensation for Mandatory Government Deductions
            decimal monthlyCompensation = employee.PayType == PayType.Monthly 
                ? employee.BasicRate 
                : record.GrossPay * 2m; // Estimate for semi-monthly daily/pakyaw workers

            // 4. Government Deductions (Split for Semi-Monthly Cutoff: 50% per cutoff)
            var (sssEmp, sssEmpEr) = PhilippineDeductionService.CalculateSss(monthlyCompensation);
            record.SssEmployee = Math.Round(sssEmp / 2m, 2);
            record.SssEmployer = Math.Round(sssEmpEr / 2m, 2);

            var (phEmp, phEmpEr) = PhilippineDeductionService.CalculatePhilHealth(monthlyCompensation);
            record.PhilHealthEmployee = Math.Round(phEmp / 2m, 2);
            record.PhilHealthEmployer = Math.Round(phEmpEr / 2m, 2);

            var (pagIbigEmp, pagIbigEmpEr) = PhilippineDeductionService.CalculatePagIbig(monthlyCompensation);
            record.PagIbigEmployee = Math.Round(pagIbigEmp / 2m, 2);
            record.PagIbigEmployer = Math.Round(pagIbigEmpEr / 2m, 2);

            // 5. BIR Tax Computation (Taxable income = Gross - SSS - PhilHealth - PagIBIG)
            decimal taxableIncome = record.GrossPay - record.SssEmployee - record.PhilHealthEmployee - record.PagIbigEmployee;
            if (taxableIncome < 0) taxableIncome = 0;

            record.WithholdingTax = PhilippineDeductionService.CalculateSemiMonthlyWithholdingTax(taxableIncome);

            return record;
        }
    }
}
