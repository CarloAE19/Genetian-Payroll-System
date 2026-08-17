using System;
using System.Collections.Generic;
using Dapper;
using GB_Payroll_System.Models;

namespace GB_Payroll_System.Data
{
    public class EmployeeRepository
    {
        public List<Employee> GetAll(bool activeOnly = true)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            string sql = activeOnly
                ? "SELECT * FROM Employees WHERE IsActive = TRUE ORDER BY LastName, FirstName;"
                : "SELECT * FROM Employees ORDER BY LastName, FirstName;";
            return [.. conn.Query<Employee>(sql)];
        }

        public Employee? GetById(int id)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            return conn.QueryFirstOrDefault<Employee>("SELECT * FROM Employees WHERE Id = @Id;", new { Id = id });
        }

        public int Insert(Employee emp)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            string sql = @"
                INSERT INTO Employees 
                    (EmployeeCode, BiometricUserId, FirstName, MiddleName, LastName, 
                     BirthDate, Gender, CivilStatus, ContactNumber, EmailAddress, Address,
                     EmergencyContactName, EmergencyContactPhone, Department, Position,
                     BranchId, ContractType, ContractStatus, ContractEndDate, PayType, 
                     BasicRate, WorkingDaysFactor, SssNumber, PhilHealthNumber,
                     PagIbigNumber, TinNumber, SssDeductionMode, CustomSssAmount,
                     PhilHealthDeductionMode, CustomPhilHealthAmount, PagIbigDeductionMode,
                     PagIbigEmployeeAmount, IsMinimumWageEarner, IsTaxExempt,
                     ContributionSchedule, DateHired, IsActive, BankAccountNumber)
                VALUES 
                    (@EmployeeCode, @BiometricUserId, @FirstName, @MiddleName, @LastName,
                     @BirthDate, @Gender, @CivilStatus, @ContactNumber, @EmailAddress, @Address,
                     @EmergencyContactName, @EmergencyContactPhone, @Department, @Position,
                     @BranchId, @ContractType, @ContractStatus, @ContractEndDate, @PayType,
                     @BasicRate, @WorkingDaysFactor, @SssNumber, @PhilHealthNumber,
                     @PagIbigNumber, @TinNumber, @SssDeductionMode, @CustomSssAmount,
                     @PhilHealthDeductionMode, @CustomPhilHealthAmount, @PagIbigDeductionMode,
                     @PagIbigEmployeeAmount, @IsMinimumWageEarner, @IsTaxExempt,
                     @ContributionSchedule, @DateHired, @IsActive, @BankAccountNumber)
                RETURNING Id;";
            return conn.ExecuteScalar<int>(sql, emp);
        }

        public void Update(Employee emp)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            string sql = @"
                UPDATE Employees SET
                    EmployeeCode = @EmployeeCode, BiometricUserId = @BiometricUserId,
                    FirstName = @FirstName, MiddleName = @MiddleName, LastName = @LastName,
                    BirthDate = @BirthDate, Gender = @Gender, CivilStatus = @CivilStatus,
                    ContactNumber = @ContactNumber, EmailAddress = @EmailAddress, Address = @Address,
                    EmergencyContactName = @EmergencyContactName, EmergencyContactPhone = @EmergencyContactPhone,
                    Department = @Department, Position = @Position, BranchId = @BranchId,
                    ContractType = @ContractType, ContractStatus = @ContractStatus, ContractEndDate = @ContractEndDate,
                    PayType = @PayType, BasicRate = @BasicRate, WorkingDaysFactor = @WorkingDaysFactor,
                    SssNumber = @SssNumber, PhilHealthNumber = @PhilHealthNumber,
                    PagIbigNumber = @PagIbigNumber, TinNumber = @TinNumber,
                    SssDeductionMode = @SssDeductionMode, CustomSssAmount = @CustomSssAmount,
                    PhilHealthDeductionMode = @PhilHealthDeductionMode, CustomPhilHealthAmount = @CustomPhilHealthAmount,
                    PagIbigDeductionMode = @PagIbigDeductionMode, PagIbigEmployeeAmount = @PagIbigEmployeeAmount,
                    IsMinimumWageEarner = @IsMinimumWageEarner, IsTaxExempt = @IsTaxExempt,
                    ContributionSchedule = @ContributionSchedule,
                    DateHired = @DateHired, IsActive = @IsActive, BankAccountNumber = @BankAccountNumber
                WHERE Id = @Id;";
            conn.Execute(sql, emp);
        }

        public void SetActive(int id, bool isActive)
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            conn.Execute("UPDATE Employees SET IsActive = @IsActive WHERE Id = @Id;", new { IsActive = isActive, Id = id });
        }

        public string GenerateNextEmployeeCode()
        {
            using var conn = DbConnectionFactory.CreateConnection();
            conn.Open();
            int count = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM Employees;");
            return $"EMP-{DateTime.Now.Year}-{(count + 1):D3}";
        }
    }
}
