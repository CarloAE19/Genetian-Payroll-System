using System;
using System.Collections.Generic;
using Dapper;
using GB_Payroll_System.Models;

namespace GB_Payroll_System.Data
{
    public class ContractRepository
    {
        public List<EmployeeContract> GetByEmployeeId(int employeeId)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            string sql = "SELECT * FROM EmployeeContracts WHERE EmployeeId = @EmployeeId ORDER BY StartDate DESC;";
            return [.. conn.Query<EmployeeContract>(sql, new { EmployeeId = employeeId })];
        }

        public EmployeeContract? GetActiveContract(int employeeId)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            string sql = "SELECT * FROM EmployeeContracts WHERE EmployeeId = @EmployeeId AND Status = 1 ORDER BY StartDate DESC LIMIT 1;";
            return conn.QueryFirstOrDefault<EmployeeContract>(sql, new { EmployeeId = employeeId });
        }

        public int Insert(EmployeeContract contract)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            string sql = @"
                INSERT INTO EmployeeContracts
                    (EmployeeId, ContractType, PositionTitle, BasicRate, PayType, StartDate, EndDate, Status, DocumentPath, Remarks)
                VALUES
                    (@EmployeeId, @ContractType, @PositionTitle, @BasicRate, @PayType, @StartDate, @EndDate, @Status, @DocumentPath, @Remarks)
                RETURNING Id;";
            return conn.ExecuteScalar<int>(sql, contract);
        }

        public void Update(EmployeeContract contract)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            string sql = @"
                UPDATE EmployeeContracts SET
                    ContractType = @ContractType, PositionTitle = @PositionTitle, BasicRate = @BasicRate,
                    PayType = @PayType, StartDate = @StartDate, EndDate = @EndDate, Status = @Status,
                    DocumentPath = @DocumentPath, Remarks = @Remarks
                WHERE Id = @Id;";
            conn.Execute(sql, contract);
        }

        public void TerminateOrExpire(int contractId, ContractStatus newStatus)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            conn.Execute("UPDATE EmployeeContracts SET Status = @Status WHERE Id = @Id;", new { Status = (int)newStatus, Id = contractId });
        }

        public List<EmployeeContract> GetExpiringContracts(int withinDays = 30)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            DateTime today = DateTime.Today;
            DateTime targetDate = today.AddDays(withinDays);
            string sql = @"
                SELECT * FROM EmployeeContracts 
                WHERE Status = 1 AND EndDate IS NOT NULL AND EndDate >= @Today AND EndDate <= @TargetDate
                ORDER BY EndDate ASC;";
            return [.. conn.Query<EmployeeContract>(sql, new { Today = today, TargetDate = targetDate })];
        }
    }
}
