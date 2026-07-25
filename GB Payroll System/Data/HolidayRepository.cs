using System;
using System.Collections.Generic;
using Dapper;
using GB_Payroll_System.Models;

namespace GB_Payroll_System.Data
{
    public class HolidayRepository
    {
        public List<Holiday> GetAll()
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            return [.. conn.Query<Holiday>(
                "SELECT * FROM Holidays WHERE IsActive = TRUE ORDER BY Date DESC;")];
        }

        public List<Holiday> GetByYear(int year)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            return [.. conn.Query<Holiday>(
                "SELECT * FROM Holidays WHERE IsActive = TRUE AND EXTRACT(YEAR FROM Date) = @Year ORDER BY Date;",
                new { Year = year })];
        }

        public void Insert(Holiday holiday)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            string sql = @"
                INSERT INTO Holidays (Name, Date, Type, WorkedMultiplier, UnworkedMultiplier, BranchId, DeclaredBy, IsActive)
                VALUES (@Name, @Date, @Type, @WorkedMultiplier, @UnworkedMultiplier, @BranchId, @DeclaredBy, @IsActive);";
            conn.Execute(sql, holiday);
        }

        public void Update(Holiday holiday)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            string sql = @"
                UPDATE Holidays SET
                    Name = @Name, Date = @Date, Type = @Type,
                    WorkedMultiplier = @WorkedMultiplier, UnworkedMultiplier = @UnworkedMultiplier,
                    BranchId = @BranchId, DeclaredBy = @DeclaredBy, IsActive = @IsActive
                WHERE Id = @Id;";
            conn.Execute(sql, holiday);
        }

        public void Delete(int id)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            conn.Execute("UPDATE Holidays SET IsActive = FALSE WHERE Id = @Id;", new { Id = id });
        }
    }
}
