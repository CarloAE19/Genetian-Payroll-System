using System;
using Dapper;
using GB_Payroll_System.Models;

namespace GB_Payroll_System.Data
{
    public class StatutorySettingsRepository
    {
        private static StatutorySettings? _cachedSettings;

        public StatutorySettings GetSettings()
        {
            if (_cachedSettings != null) return _cachedSettings;

            try
            {
                using var conn = DbConnectionFactory.CreateConnection();
                conn.Open();
                var settings = conn.QueryFirstOrDefault<StatutorySettings>("SELECT * FROM StatutorySettings WHERE Id = 1;");
                if (settings != null)
                {
                    _cachedSettings = settings;
                    return settings;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Could not load statutory settings from DB: {ex.Message}");
            }

            // Fallback default settings
            _cachedSettings = new StatutorySettings();
            return _cachedSettings;
        }

        public void SaveSettings(StatutorySettings settings)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            string sql = @"
                INSERT INTO StatutorySettings 
                    (Id, SssTotalRatePercent, SssEmployeeSharePercent, SssEmployerSharePercent,
                     SssMinSalaryCredit, SssMaxSalaryCredit, PhilHealthTotalRatePercent,
                     PhilHealthEmployeeSharePercent, PhilHealthEmployerSharePercent,
                     PhilHealthMinSalaryCredit, PhilHealthMaxSalaryCredit,
                     PagIbigEmployeeStandardMonthly, PagIbigEmployerStandardMonthly,
                     BirSemiMonthlyExemptCeiling, BirBonusExemptCap, UpdatedAt, UpdatedByUsername)
                VALUES 
                    (1, @SssTotalRatePercent, @SssEmployeeSharePercent, @SssEmployerSharePercent,
                     @SssMinSalaryCredit, @SssMaxSalaryCredit, @PhilHealthTotalRatePercent,
                     @PhilHealthEmployeeSharePercent, @PhilHealthEmployerSharePercent,
                     @PhilHealthMinSalaryCredit, @PhilHealthMaxSalaryCredit,
                     @PagIbigEmployeeStandardMonthly, @PagIbigEmployerStandardMonthly,
                     @BirSemiMonthlyExemptCeiling, @BirBonusExemptCap, CURRENT_TIMESTAMP, @UpdatedByUsername)
                ON CONFLICT (Id) DO UPDATE SET
                    SssTotalRatePercent = EXCLUDED.SssTotalRatePercent,
                    SssEmployeeSharePercent = EXCLUDED.SssEmployeeSharePercent,
                    SssEmployerSharePercent = EXCLUDED.SssEmployerSharePercent,
                    SssMinSalaryCredit = EXCLUDED.SssMinSalaryCredit,
                    SssMaxSalaryCredit = EXCLUDED.SssMaxSalaryCredit,
                    PhilHealthTotalRatePercent = EXCLUDED.PhilHealthTotalRatePercent,
                    PhilHealthEmployeeSharePercent = EXCLUDED.PhilHealthEmployeeSharePercent,
                    PhilHealthEmployerSharePercent = EXCLUDED.PhilHealthEmployerSharePercent,
                    PhilHealthMinSalaryCredit = EXCLUDED.PhilHealthMinSalaryCredit,
                    PhilHealthMaxSalaryCredit = EXCLUDED.PhilHealthMaxSalaryCredit,
                    PagIbigEmployeeStandardMonthly = EXCLUDED.PagIbigEmployeeStandardMonthly,
                    PagIbigEmployerStandardMonthly = EXCLUDED.PagIbigEmployerStandardMonthly,
                    BirSemiMonthlyExemptCeiling = EXCLUDED.BirSemiMonthlyExemptCeiling,
                    BirBonusExemptCap = EXCLUDED.BirBonusExemptCap,
                    UpdatedAt = CURRENT_TIMESTAMP,
                    UpdatedByUsername = EXCLUDED.UpdatedByUsername;";

            conn.Execute(sql, settings);
            _cachedSettings = settings;
        }

        public StatutorySettings ResetToDefaults(string username)
        {
            var def = new StatutorySettings
            {
                Id = 1,
                SssTotalRatePercent = 14.00m,
                SssEmployeeSharePercent = 4.50m,
                SssEmployerSharePercent = 9.50m,
                SssMinSalaryCredit = 5000.00m,
                SssMaxSalaryCredit = 35000.00m,
                PhilHealthTotalRatePercent = 5.00m,
                PhilHealthEmployeeSharePercent = 2.50m,
                PhilHealthEmployerSharePercent = 2.50m,
                PhilHealthMinSalaryCredit = 10000.00m,
                PhilHealthMaxSalaryCredit = 100000.00m,
                PagIbigEmployeeStandardMonthly = 200.00m,
                PagIbigEmployerStandardMonthly = 200.00m,
                BirSemiMonthlyExemptCeiling = 10417.00m,
                BirBonusExemptCap = 90000.00m,
                UpdatedByUsername = username,
                UpdatedAt = DateTime.UtcNow
            };

            SaveSettings(def);
            return def;
        }
    }
}
