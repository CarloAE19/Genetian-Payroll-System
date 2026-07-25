using System;

namespace GB_Payroll_System.Services
{
    public class PhilippineDeductionService
    {
        /// <summary>
        /// SSS Monthly Contribution (2025/2026 DOLE Schedule)
        /// Total Rate: 14% (Employee 4.5%, Employer 9.5%) capped at MSC ₱35,000
        /// </summary>
        public static (decimal Employee, decimal Employer) CalculateSss(decimal monthlyCompensation)
        {
            if (monthlyCompensation <= 0) return (0, 0);

            // Minimum Monthly Salary Credit = ₱5,000, Maximum = ₱35,000
            decimal msc = Math.Clamp(monthlyCompensation, 5000m, 35000m);

            decimal employeeShare = Math.Round(msc * 0.045m, 2);
            decimal employerShare = Math.Round(msc * 0.095m, 2);

            return (employeeShare, employerShare);
        }

        /// <summary>
        /// PhilHealth Monthly Contribution (2025/2026 Premium 5% shared 50/50)
        /// Minimum Floor: ₱10,000 MSC, Maximum Ceiling: ₱100,000 MSC
        /// </summary>
        public static (decimal Employee, decimal Employer) CalculatePhilHealth(decimal monthlyCompensation)
        {
            if (monthlyCompensation <= 0) return (0, 0);

            decimal msc = Math.Clamp(monthlyCompensation, 10000m, 100000m);
            decimal totalContribution = Math.Round(msc * 0.05m, 2);

            decimal shared = Math.Round(totalContribution / 2m, 2);
            return (shared, shared);
        }

        /// <summary>
        /// Pag-IBIG (HDMF) Contribution
        /// Standard Mandatory Employee Cap: ₱200/month (Matched ₱200 by Employer)
        /// </summary>
        public static (decimal Employee, decimal Employer) CalculatePagIbig(decimal monthlyCompensation)
        {
            if (monthlyCompensation <= 0) return (0, 0);

            decimal employee = 200m;
            decimal employer = 200m;

            return (employee, employer);
        }

        /// <summary>
        /// BIR Withholding Tax (TRAIN Law Semi-Monthly Graduated Tax Table)
        /// </summary>
        public static decimal CalculateSemiMonthlyWithholdingTax(decimal taxableIncome)
        {
            if (taxableIncome <= 10417m) return 0m; // ₱10,417 or below = Tax Exempt (₱250,000 annual exemption)

            if (taxableIncome <= 16666m)
            {
                // 15% in excess of 10,417
                return Math.Round((taxableIncome - 10417m) * 0.15m, 2);
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
