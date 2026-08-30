using System;
using System.Collections.Generic;
using Dapper;
using GB_Payroll_System.Models;

namespace GB_Payroll_System.Data
{
    public class BranchRepository
    {
        public List<Branch> GetAll(bool includeInactive = false)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            string sql = includeInactive
                ? "SELECT * FROM Branches ORDER BY Name ASC;"
                : "SELECT * FROM Branches WHERE IsActive = TRUE ORDER BY Name ASC;";
            return [.. conn.Query<Branch>(sql)];
        }

        public Branch? GetById(int id)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            return conn.QueryFirstOrDefault<Branch>("SELECT * FROM Branches WHERE Id = @Id;", new { Id = id });
        }

        public int Insert(Branch branch)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            string sql = @"
                INSERT INTO Branches (Code, Name, Location, IsActive)
                VALUES (@Code, @Name, @Location, @IsActive)
                RETURNING Id;";
            return conn.ExecuteScalar<int>(sql, branch);
        }

        public void Update(Branch branch)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            string sql = @"
                UPDATE Branches 
                SET Code = @Code, Name = @Name, Location = @Location, IsActive = @IsActive
                WHERE Id = @Id;";
            conn.Execute(sql, branch);
        }

        public void SetActive(int id, bool isActive)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            conn.Execute("UPDATE Branches SET IsActive = @IsActive WHERE Id = @Id;", new { Id = id, IsActive = isActive });
        }
    }
}
