using System;
using System.Collections.Generic;
using System.IO;
using Dapper;
using GB_Payroll_System.Models;

namespace GB_Payroll_System.Data
{
    public class DocumentRepository
    {
        public static string GetVaultDirectory(int employeeId)
        {
            string baseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "201_Documents", employeeId.ToString());
            if (!Directory.Exists(baseFolder))
            {
                Directory.CreateDirectory(baseFolder);
            }
            return baseFolder;
        }

        public List<EmployeeDocument> GetByEmployeeId(int employeeId)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            string sql = "SELECT * FROM EmployeeDocuments WHERE EmployeeId = @EmployeeId ORDER BY UploadedAt DESC;";
            return [.. conn.Query<EmployeeDocument>(sql, new { EmployeeId = employeeId })];
        }

        public int SaveDocument(int employeeId, DocumentCategory category, string title, string sourceFilePath)
        {
            if (!File.Exists(sourceFilePath))
                throw new FileNotFoundException("Source document file not found.", sourceFilePath);

            string vaultDir = GetVaultDirectory(employeeId);
            string originalFileName = Path.GetFileName(sourceFilePath);
            string uniqueFileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{originalFileName}";
            string destPath = Path.Combine(vaultDir, uniqueFileName);

            File.Copy(sourceFilePath, destPath, true);

            var fileInfo = new FileInfo(destPath);

            var doc = new EmployeeDocument
            {
                EmployeeId = employeeId,
                Category = category,
                Title = string.IsNullOrWhiteSpace(title) ? Path.GetFileNameWithoutExtension(originalFileName) : title,
                FileName = originalFileName,
                FilePath = destPath,
                FileSizeBytes = fileInfo.Length,
                UploadedAt = DateTime.UtcNow
            };

            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            string sql = @"
                INSERT INTO EmployeeDocuments
                    (EmployeeId, Category, Title, FileName, FilePath, FileSizeBytes, UploadedAt)
                VALUES
                    (@EmployeeId, @Category, @Title, @FileName, @FilePath, @FileSizeBytes, @UploadedAt)
                RETURNING Id;";
            return conn.ExecuteScalar<int>(sql, doc);
        }

        public void DeleteDocument(int documentId)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            var doc = conn.QueryFirstOrDefault<EmployeeDocument>("SELECT * FROM EmployeeDocuments WHERE Id = @Id;", new { Id = documentId });
            if (doc != null)
            {
                conn.Execute("DELETE FROM EmployeeDocuments WHERE Id = @Id;", new { Id = documentId });
                try
                {
                    if (File.Exists(doc.FilePath))
                    {
                        File.Delete(doc.FilePath);
                    }
                }
                catch
                {
                    // Ignore file lock during delete
                }
            }
        }
    }
}
