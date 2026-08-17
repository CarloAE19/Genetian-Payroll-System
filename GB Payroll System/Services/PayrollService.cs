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
            List<Attendance> attendances)
        {
            var record = new PayrollRecord
            {
                PayrollPeriodId = period.Id,
                EmployeeId = employee.Id
            };

            decimal dailyRate = AttendanceService.GetDailyRate(employee);
            decimal hourlyRate = AttendanceService.GetHourlyRate(employee);
            decimal minuteRate = hourlyRate / 60m;

            // 1. Attendance, Tardiness, OT, Night Diff, Holiday Pay Calculations
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

            // 2. Basic Pay Computation
            if (employee.PayType == PayType.Monthly)
            {
                // Semi-monthly basic pay (50% of monthly basic)
                record.BasicPay = Math.Round(employee.BasicRate / 2m, 2);
            }
            else // PayType.Daily
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

            // Estimated Monthly Equivalent Compensation for Statutory Base
            decimal monthlyCompensation = employee.PayType == PayType.Monthly 
                ? employee.BasicRate 
                : record.GrossPay * 2m;

            // Determine Cutoff Deduction Factor based on employee schedule
            bool isFirstCutoff = period.EndDate.Day <= 15;
            decimal deductionFactor = employee.ContributionSchedule switch
            {
                ContributionSchedule.FirstCutoffOnly => isFirstCutoff ? 1.0m : 0.0m,
                ContributionSchedule.SecondCutoffOnly => isFirstCutoff ? 0.0m : 1.0m,
                _ => 0.5m // SplitBothCutoffs (50% per cutoff)
            };

            // 3. SSS Deduction
            if (employee.SssDeductionMode == DeductionMode.Exempt || deductionFactor == 0m)
            {
                record.SssEmployee = 0m;
                record.SssEmployer = 0m;
            }
            else if (employee.SssDeductionMode == DeductionMode.FixedAmount)
            {
                record.SssEmployee = Math.Round(employee.CustomSssAmount * deductionFactor, 2);
                record.SssEmployer = Math.Round(employee.CustomSssAmount * 2.11m * deductionFactor, 2); // Approximate standard ER ratio
            }
            else // DeductionMode.Auto
            {
                var (sssEmp, sssEmpEr) = PhilippineDeductionService.CalculateSss(monthlyCompensation);
                record.SssEmployee = Math.Round(sssEmp * deductionFactor, 2);
                record.SssEmployer = Math.Round(sssEmpEr * deductionFactor, 2);
            }

            // 4. PhilHealth Deduction
            if (employee.PhilHealthDeductionMode == DeductionMode.Exempt || deductionFactor == 0m)
            {
                record.PhilHealthEmployee = 0m;
                record.PhilHealthEmployer = 0m;
            }
            else if (employee.PhilHealthDeductionMode == DeductionMode.FixedAmount)
            {
                record.PhilHealthEmployee = Math.Round(employee.CustomPhilHealthAmount * deductionFactor, 2);
                record.PhilHealthEmployer = Math.Round(employee.CustomPhilHealthAmount * deductionFactor, 2);
            }
            else // DeductionMode.Auto
            {
                var (phEmp, phEmpEr) = PhilippineDeductionService.CalculatePhilHealth(monthlyCompensation);
                record.PhilHealthEmployee = Math.Round(phEmp * deductionFactor, 2);
                record.PhilHealthEmployer = Math.Round(phEmpEr * deductionFactor, 2);
            }

            // 5. Pag-IBIG (HDMF) Deduction
            if (employee.PagIbigDeductionMode == DeductionMode.Exempt || deductionFactor == 0m)
            {
                record.PagIbigEmployee = 0m;
                record.PagIbigEmployer = 0m;
            }
            else
            {
                decimal monthlyPagIbig = employee.PagIbigEmployeeAmount > 0 ? employee.PagIbigEmployeeAmount : 200m;
                record.PagIbigEmployee = Math.Round(monthlyPagIbig * deductionFactor, 2);
                record.PagIbigEmployer = Math.Round(200m * deductionFactor, 2); // Mandatory employer counterpart
            }

            // 6. BIR Withholding Tax
            if (employee.IsMinimumWageEarner || employee.IsTaxExempt)
            {
                record.WithholdingTax = 0m; // Minimum Wage Earners and Tax Exempt are zero tax
            }
            else
            {
                // Taxable income = Gross - SSS - PhilHealth - PagIBIG
                decimal taxableIncome = record.GrossPay - record.SssEmployee - record.PhilHealthEmployee - record.PagIbigEmployee;
                if (taxableIncome < 0) taxableIncome = 0;

                record.WithholdingTax = PhilippineDeductionService.CalculateSemiMonthlyWithholdingTax(taxableIncome);
            }

            return record;
        }
    }
}
