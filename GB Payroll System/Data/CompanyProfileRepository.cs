using System;
using Dapper;
using GB_Payroll_System.Models;

namespace GB_Payroll_System.Data
{
    public class CompanyProfileRepository
    {
        private static CompanyProfile? _cachedProfile;

        public CompanyProfile GetProfile()
        {
            if (_cachedProfile != null) return _cachedProfile;

            try
            {
                using var conn = DbConnectionFactory.CreateConnection();
                conn.Open();
                var profile = conn.QueryFirstOrDefault<CompanyProfile>("SELECT * FROM CompanyProfiles WHERE Id = 1;");
                if (profile != null)
                {
                    _cachedProfile = profile;
                    return profile;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Could not load company profile from DB: {ex.Message}");
            }

            _cachedProfile = new CompanyProfile();
            return _cachedProfile;
        }

        public void SaveProfile(CompanyProfile profile)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            string sql = @"
                INSERT INTO CompanyProfiles 
                    (Id, CompanyName, TradeName, CompanyAddress, ContactNumber, EmailAddress,
                     EmployerSssNumber, EmployerPhilHealthNumber, EmployerPagIbigNumber, EmployerTin,
                     AuthorizedSignatoryName, AuthorizedSignatoryTitle, UpdatedAt, UpdatedByUsername)
                VALUES 
                    (1, @CompanyName, @TradeName, @CompanyAddress, @ContactNumber, @EmailAddress,
                     @EmployerSssNumber, @EmployerPhilHealthNumber, @EmployerPagIbigNumber, @EmployerTin,
                     @AuthorizedSignatoryName, @AuthorizedSignatoryTitle, CURRENT_TIMESTAMP, @UpdatedByUsername)
                ON CONFLICT (Id) DO UPDATE SET
                    CompanyName = EXCLUDED.CompanyName,
                    TradeName = EXCLUDED.TradeName,
                    CompanyAddress = EXCLUDED.CompanyAddress,
                    ContactNumber = EXCLUDED.ContactNumber,
                    EmailAddress = EXCLUDED.EmailAddress,
                    EmployerSssNumber = EXCLUDED.EmployerSssNumber,
                    EmployerPhilHealthNumber = EXCLUDED.EmployerPhilHealthNumber,
                    EmployerPagIbigNumber = EXCLUDED.EmployerPagIbigNumber,
                    EmployerTin = EXCLUDED.EmployerTin,
                    AuthorizedSignatoryName = EXCLUDED.AuthorizedSignatoryName,
                    AuthorizedSignatoryTitle = EXCLUDED.AuthorizedSignatoryTitle,
                    UpdatedAt = CURRENT_TIMESTAMP,
                    UpdatedByUsername = EXCLUDED.UpdatedByUsername;";

            conn.Execute(sql, profile);
            _cachedProfile = profile;
        }
    }
}
