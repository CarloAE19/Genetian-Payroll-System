using System;

namespace GB_Payroll_System.Models
{
    public enum DocumentCategory
    {
        EmploymentContract = 1,
        ResumeCv = 2,
        GovernmentId = 3,
        NbiPoliceClearance = 4,
        MedicalCertificate = 5,
        BirthCertificate = 6,
        DisciplinaryOrMemo = 7,
        Other = 8
    }

    public class EmployeeDocument
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DocumentCategory Category { get; set; } = DocumentCategory.Other;
        public string Title { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public string CategoryDisplay => Category switch
        {
            DocumentCategory.EmploymentContract => "Employment Contract",
            DocumentCategory.ResumeCv           => "Resume / CV",
            DocumentCategory.GovernmentId       => "Government ID",
            DocumentCategory.NbiPoliceClearance => "NBI / Police Clearance",
            DocumentCategory.MedicalCertificate => "Medical / Fit to Work",
            DocumentCategory.BirthCertificate   => "Birth / PSA Certificate",
            DocumentCategory.DisciplinaryOrMemo => "Memo / Notice",
            _                                   => "Other Document"
        };

        public string FileSizeFormatted => FileSizeBytes > 1024 * 1024
            ? $"{FileSizeBytes / (1024.0 * 1024.0):F2} MB"
            : $"{FileSizeBytes / 1024.0:F1} KB";
    }
}
