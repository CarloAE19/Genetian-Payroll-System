using System;

namespace GB_Payroll_System.Models
{
    public class CompanyProfile
    {
        public int Id { get; set; } = 1;
        public string CompanyName { get; set; } = "Genetian Enterprise Solutions";
        public string TradeName { get; set; } = "Genetian GB";
        public string CompanyAddress { get; set; } = "General Santos City, South Cotabato, Philippines";
        public string ContactNumber { get; set; } = "+63 (083) 552-0000";
        public string EmailAddress { get; set; } = "info@genetian.ph";

        // Employer Statutory Remittance Numbers
        public string EmployerSssNumber { get; set; } = "09-1234567-8";
        public string EmployerPhilHealthNumber { get; set; } = "12-345678901-2";
        public string EmployerPagIbigNumber { get; set; } = "1234-5678-9012";
        public string EmployerTin { get; set; } = "000-123-456-000";

        // Authorized Signatory / HR Head
        public string AuthorizedSignatoryName { get; set; } = "Maria Santos";
        public string AuthorizedSignatoryTitle { get; set; } = "HR & Administrative Director";

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string UpdatedByUsername { get; set; } = "system";
    }
}
