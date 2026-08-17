using System;
using GB_Payroll_System.Data;
using GB_Payroll_System.Models;

namespace GB_Payroll_System.Services
{
    public class PhilippineDeductionService
    {
        private static readonly StatutorySettingsRepository _settingsRepo = new();

        /// <summary>
        /// SSS Monthly Contribution computed dynamically based on configured Statutory Settings
        /// </summary>
        public static (decimal Employee, decimal Employer) CalculateSss(decimal monthlyCompensation, StatutorySettings? customSettings = null)
        {
            if (monthlyCompensation <= 0) return (0, 0);

            var settings = customSettings ?? _settingsRepo.GetSettings();

            decimal minMsc = settings.SssMinSalaryCredit > 0 ? settings.SssMinSalaryCredit : 5000m;
            decimal maxMsc = settings.SssMaxSalaryCredit > 0 ? settings.SssMaxSalaryCredit : 35000m;
            decimal eePercent = settings.SssEmployeeSharePercent > 0 ? settings.SssEmployeeSharePercent / 100m : 0.045m;
            decimal erPercent = settings.SssEmployerSharePercent > 0 ? settings.SssEmployerSharePercent / 100m : 0.095m;

            decimal msc = Math.Clamp(monthlyCompensation, minMsc, maxMsc);

            decimal employeeShare = Math.Round(msc * eePercent, 2);
            decimal employerShare = Math.Round(msc * erPercent, 2);

            return (employeeShare, employerShare);
        }

        /// <summary>
        /// PhilHealth Monthly Contribution computed dynamically based on configured Statutory Settings
        /// </summary>
        public static (decimal Employee, decimal Employer) CalculatePhilHealth(decimal monthlyCompensation, StatutorySettings? customSettings = null)
        {
            if (monthlyCompensation <= 0) return (0, 0);

            var settings = customSettings ?? _settingsRepo.GetSettings();

            decimal minMsc = settings.PhilHealthMinSalaryCredit > 0 ? settings.PhilHealthMinSalaryCredit : 10000m;
            decimal maxMsc = settings.PhilHealthMaxSalaryCredit > 0 ? settings.PhilHealthMaxSalaryCredit : 100000m;
            decimal eePercent = settings.PhilHealthEmployeeSharePercent > 0 ? settings.PhilHealthEmployeeSharePercent / 100m : 0.025m;
            decimal erPercent = settings.PhilHealthEmployerSharePercent > 0 ? settings.PhilHealthEmployerSharePercent / 100m : 0.025m;

            decimal msc = Math.Clamp(monthlyCompensation, minMsc, maxMsc);

            decimal employeeShare = Math.Round(msc * eePercent, 2);
            decimal employerShare = Math.Round(msc * erPercent, 2);

            return (employeeShare, employerShare);
        }

        /// <summary>
        /// Pag-IBIG (HDMF) Contribution computed dynamically based on configured Statutory Settings
        /// </summary>
        public static (decimal Employee, decimal Employer) CalculatePagIbig(decimal monthlyCompensation, StatutorySettings? customSettings = null)
        {
            if (monthlyCompensation <= 0) return (0, 0);

            var settings = customSettings ?? _settingsRepo.GetSettings();

            decimal employee = settings.PagIbigEmployeeStandardMonthly > 0 ? settings.PagIbigEmployeeStandardMonthly : 200m;
            decimal employer = settings.PagIbigEmployerStandardMonthly > 0 ? settings.PagIbigEmployerStandardMonthly : 200m;

            return (employee, employer);
        }

        /// <summary>
        /// BIR Withholding Tax computed dynamically based on configured Statutory Settings and TRAIN Law brackets
        /// </summary>
        public static decimal CalculateSemiMonthlyWithholdingTax(decimal taxableIncome, StatutorySettings? customSettings = null)
        {
            var settings = customSettings ?? _settingsRepo.GetSettings();
            decimal exemptFloor = settings.BirSemiMonthlyExemptCeiling > 0 ? settings.BirSemiMonthlyExemptCeiling : 10417m;

            if (taxableIncome <= exemptFloor) return 0m; // Tax Exempt threshold

            if (taxableIncome <= 16666m)
            {
                // 15% in excess of exempt floor
                return Math.Round((taxableIncome - exemptFloor) * 0.15m, 2);
            }
            if (taxableIncome <= 33333m)
            {
                // 937.50 + 20% in excess of 16,666
                return Math.Round(937.50m + ((taxableIncome - 16666m) * 0.20m), 2);
            }
            if (taxableIncome <= 83333m)
            {
                // 4,270.83 + 25% in excess of 33,333
                return Math.Round(4270.83m + ((taxableIncome - 33333m) * 0.25m), 2);
            }
            if (taxableIncome <= 333333m)
            {
                // 16,770.83 + 30% in excess of 83,333
                return Math.Round(16770.83m + ((taxableIncome - 83333m) * 0.30m), 2);
            }

            // Above 333,333: 91,770.83 + 35% in excess of 333,333
            return Math.Round(91770.83m + ((taxableIncome - 333333m) * 0.35m), 2);
        }
    }
}
