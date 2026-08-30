using System;
using Dapper;

namespace GB_Payroll_System.Data
{
    public static class DatabaseInitializer
    {
        public static void Initialize()
        {
            try
            {
                using var conn = DbConnectionFactory.CreateConnection();
                conn.Open();

                string sql = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id SERIAL PRIMARY KEY,
                    Username VARCHAR(50) UNIQUE NOT NULL,
                    PasswordHash VARCHAR(255) NOT NULL,
                    FullName VARCHAR(100) NOT NULL,
                    Email VARCHAR(100),
                    Role INT NOT NULL,
                    IsActive BOOLEAN DEFAULT TRUE,
                    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
                    LastLoginAt TIMESTAMP WITH TIME ZONE
                );

                CREATE TABLE IF NOT EXISTS Branches (
                    Id SERIAL PRIMARY KEY,
                    Code VARCHAR(20) NOT NULL,
                    Name VARCHAR(100) NOT NULL,
                    Location VARCHAR(150),
                    IsActive BOOLEAN DEFAULT TRUE
                );

                CREATE TABLE IF NOT EXISTS CompanyProfiles (
                    Id INT PRIMARY KEY DEFAULT 1,
                    CompanyName VARCHAR(150) NOT NULL,
                    TradeName VARCHAR(100),
                    CompanyAddress TEXT,
                    ContactNumber VARCHAR(50),
                    EmailAddress VARCHAR(100),
                    EmployerSssNumber VARCHAR(30),
                    EmployerPhilHealthNumber VARCHAR(30),
                    EmployerPagIbigNumber VARCHAR(30),
                    EmployerTin VARCHAR(30),
                    AuthorizedSignatoryName VARCHAR(100),
                    AuthorizedSignatoryTitle VARCHAR(100),
                    UpdatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
                    UpdatedByUsername VARCHAR(50) DEFAULT 'system'
                );

                CREATE TABLE IF NOT EXISTS Employees (
                    Id SERIAL PRIMARY KEY,
                    EmployeeCode VARCHAR(50) UNIQUE NOT NULL,
                    BiometricUserId VARCHAR(50),
                    FirstName VARCHAR(50) NOT NULL,
                    MiddleName VARCHAR(50),
                    LastName VARCHAR(50) NOT NULL,
                    BirthDate DATE,
                    Gender VARCHAR(20) DEFAULT 'Male',
                    CivilStatus VARCHAR(30) DEFAULT 'Single',
                    ContactNumber VARCHAR(50),
                    EmailAddress VARCHAR(100),
                    Address TEXT,
                    EmergencyContactName VARCHAR(100),
                    EmergencyContactPhone VARCHAR(50),
                    Department VARCHAR(50),
                    Position VARCHAR(50),
                    BranchId INT REFERENCES Branches(Id),
                    ContractType INT NOT NULL DEFAULT 2,
                    ContractStatus INT NOT NULL DEFAULT 1,
                    ContractEndDate DATE,
                    PayType INT NOT NULL DEFAULT 1,
                    BasicRate NUMERIC(12,2) NOT NULL DEFAULT 0.00,
                    WorkingDaysFactor NUMERIC(5,2) DEFAULT 313.00,
                    SssNumber VARCHAR(20),
                    PhilHealthNumber VARCHAR(20),
                    PagIbigNumber VARCHAR(20),
                    TinNumber VARCHAR(20),
                    SssDeductionMode INT DEFAULT 1,
                    CustomSssAmount NUMERIC(12,2) DEFAULT 0.00,
                    PhilHealthDeductionMode INT DEFAULT 1,
                    CustomPhilHealthAmount NUMERIC(12,2) DEFAULT 0.00,
                    PagIbigDeductionMode INT DEFAULT 1,
                    PagIbigEmployeeAmount NUMERIC(12,2) DEFAULT 200.00,
                    IsMinimumWageEarner BOOLEAN DEFAULT FALSE,
                    IsTaxExempt BOOLEAN DEFAULT FALSE,
                    ContributionSchedule INT DEFAULT 1,
                    DateHired DATE DEFAULT CURRENT_DATE,
                    IsActive BOOLEAN DEFAULT TRUE,
                    BankAccountNumber VARCHAR(50)
                );

                CREATE TABLE IF NOT EXISTS EmployeeContracts (
                    Id SERIAL PRIMARY KEY,
                    EmployeeId INT REFERENCES Employees(Id),
                    ContractType INT NOT NULL DEFAULT 2,
                    PositionTitle VARCHAR(50),
                    BasicRate NUMERIC(12,2) NOT NULL DEFAULT 0.00,
                    PayType INT NOT NULL DEFAULT 1,
                    StartDate DATE NOT NULL,
                    EndDate DATE,
                    Status INT NOT NULL DEFAULT 1,
                    DocumentPath TEXT,
                    Remarks TEXT,
                    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS EmployeeDocuments (
                    Id SERIAL PRIMARY KEY,
                    EmployeeId INT REFERENCES Employees(Id),
                    Category INT NOT NULL DEFAULT 8,
                    Title VARCHAR(150) NOT NULL,
                    FileName VARCHAR(255) NOT NULL,
                    FilePath TEXT NOT NULL,
                    FileSizeBytes BIGINT DEFAULT 0,
                    UploadedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS SalaryPromotionHistories (
                    Id SERIAL PRIMARY KEY,
                    EmployeeId INT REFERENCES Employees(Id),
                    PreviousPosition VARCHAR(50),
                    NewPosition VARCHAR(50),
                    PreviousRate NUMERIC(12,2),
                    NewRate NUMERIC(12,2),
                    EffectiveDate DATE NOT NULL,
                    Reason TEXT,
                    ApprovedByUsername VARCHAR(50),
                    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS EmploymentHistories (
                    Id SERIAL PRIMARY KEY,
                    EmployeeId INT REFERENCES Employees(Id),
                    CompanyName VARCHAR(150) NOT NULL DEFAULT 'Genetian',
                    Department VARCHAR(100),
                    Position VARCHAR(100),
                    StartDate DATE NOT NULL,
                    EndDate DATE,
                    EmploymentType VARCHAR(50) DEFAULT 'Regular',
                    SeparationType VARCHAR(50) DEFAULT 'Active',
                    SeparationReason TEXT,
                    IsRehireEligible BOOLEAN DEFAULT TRUE,
                    RecordedByUsername VARCHAR(50),
                    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS Holidays (
                    Id SERIAL PRIMARY KEY,
                    Name VARCHAR(100) NOT NULL,
                    Date DATE NOT NULL,
                    Type INT NOT NULL,
                    WorkedMultiplier NUMERIC(4,2) DEFAULT 2.00,
                    UnworkedMultiplier NUMERIC(4,2) DEFAULT 1.00,
                    BranchId INT REFERENCES Branches(Id),
                    DeclaredBy VARCHAR(100),
                    IsActive BOOLEAN DEFAULT TRUE
                );

                CREATE TABLE IF NOT EXISTS BiometricLogs (
                    Id SERIAL PRIMARY KEY,
                    BiometricUserId VARCHAR(50) NOT NULL,
                    Timestamp TIMESTAMP WITH TIME ZONE NOT NULL,
                    PunchType VARCHAR(20) DEFAULT 'IN',
                    DeviceIp VARCHAR(50),
                    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS Attendances (
                    Id SERIAL PRIMARY KEY,
                    EmployeeId INT REFERENCES Employees(Id),
                    Date DATE NOT NULL,
                    TimeIn TIME,
                    TimeOut TIME,
                    LateMinutes DOUBLE PRECISION DEFAULT 0,
                    UndertimeMinutes DOUBLE PRECISION DEFAULT 0,
                    RegularHoursWorked DOUBLE PRECISION DEFAULT 0,
                    OvertimeHours DOUBLE PRECISION DEFAULT 0,
                    NightDiffHours DOUBLE PRECISION DEFAULT 0,
                    Status INT NOT NULL DEFAULT 1,
                    HolidayId INT REFERENCES Holidays(Id),
                    IsManuallyAdjusted BOOLEAN DEFAULT FALSE,
                    AdjustmentReason TEXT,
                    AdjustedByUsername VARCHAR(50)
                );

                CREATE TABLE IF NOT EXISTS EmployeeLeaveBalances (
                    Id SERIAL PRIMARY KEY,
                    EmployeeId INT REFERENCES Employees(Id),
                    Year INT NOT NULL,
                    VacationLeaveTotal NUMERIC(5,2) DEFAULT 15.00,
                    VacationLeaveUsed NUMERIC(5,2) DEFAULT 0.00,
                    SickLeaveTotal NUMERIC(5,2) DEFAULT 15.00,
                    SickLeaveUsed NUMERIC(5,2) DEFAULT 0.00,
                    EmergencyLeaveTotal NUMERIC(5,2) DEFAULT 5.00,
                    EmergencyLeaveUsed NUMERIC(5,2) DEFAULT 0.00,
                    UpdatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
                    CONSTRAINT uq_emp_leave_year UNIQUE(EmployeeId, Year)
                );

                CREATE TABLE IF NOT EXISTS LeaveApplications (
                    Id SERIAL PRIMARY KEY,
                    EmployeeId INT REFERENCES Employees(Id),
                    Type INT NOT NULL,
                    StartDate DATE NOT NULL,
                    EndDate DATE NOT NULL,
                    DaysCount NUMERIC(4,1) NOT NULL,
                    Reason TEXT,
                    Status INT NOT NULL DEFAULT 1,
                    ApprovedByUsername VARCHAR(50),
                    ApprovedAt TIMESTAMP WITH TIME ZONE,
                    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS PayrollPeriods (
                    Id SERIAL PRIMARY KEY,
                    PeriodCode VARCHAR(50) UNIQUE NOT NULL,
                    StartDate DATE NOT NULL,
                    EndDate DATE NOT NULL,
                    PayoutDate DATE NOT NULL,
                    IsClosed BOOLEAN DEFAULT FALSE,
                    ProcessedByUsername VARCHAR(50),
                    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS PayrollRecords (
                    Id SERIAL PRIMARY KEY,
                    PayrollPeriodId INT REFERENCES PayrollPeriods(Id),
                    EmployeeId INT REFERENCES Employees(Id),
                    BasicPay NUMERIC(12,2) DEFAULT 0,
                    OvertimePay NUMERIC(12,2) DEFAULT 0,
                    NightDiffPay NUMERIC(12,2) DEFAULT 0,
                    HolidayPay NUMERIC(12,2) DEFAULT 0,
                    Allowances NUMERIC(12,2) DEFAULT 0,
                    TardinessDeduction NUMERIC(12,2) DEFAULT 0,
                    UndertimeDeduction NUMERIC(12,2) DEFAULT 0,
                    AbsenceDeduction NUMERIC(12,2) DEFAULT 0,
                    SssEmployee NUMERIC(12,2) DEFAULT 0,
                    SssEmployer NUMERIC(12,2) DEFAULT 0,
                    PhilHealthEmployee NUMERIC(12,2) DEFAULT 0,
                    PhilHealthEmployer NUMERIC(12,2) DEFAULT 0,
                    PagIbigEmployee NUMERIC(12,2) DEFAULT 0,
                    PagIbigEmployer NUMERIC(12,2) DEFAULT 0,
                    WithholdingTax NUMERIC(12,2) DEFAULT 0,
                    OtherDeductions NUMERIC(12,2) DEFAULT 0,
                    ComputedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS StatutorySettings (
                    Id INT PRIMARY KEY DEFAULT 1,
                    SssTotalRatePercent NUMERIC(5,2) DEFAULT 14.00,
                    SssEmployeeSharePercent NUMERIC(5,2) DEFAULT 4.50,
                    SssEmployerSharePercent NUMERIC(5,2) DEFAULT 9.50,
                    SssMinSalaryCredit NUMERIC(12,2) DEFAULT 5000.00,
                    SssMaxSalaryCredit NUMERIC(12,2) DEFAULT 35000.00,
                    PhilHealthTotalRatePercent NUMERIC(5,2) DEFAULT 5.00,
                    PhilHealthEmployeeSharePercent NUMERIC(5,2) DEFAULT 2.50,
                    PhilHealthEmployerSharePercent NUMERIC(5,2) DEFAULT 2.50,
                    PhilHealthMinSalaryCredit NUMERIC(12,2) DEFAULT 10000.00,
                    PhilHealthMaxSalaryCredit NUMERIC(12,2) DEFAULT 100000.00,
                    PagIbigEmployeeStandardMonthly NUMERIC(12,2) DEFAULT 200.00,
                    PagIbigEmployerStandardMonthly NUMERIC(12,2) DEFAULT 200.00,
                    BirSemiMonthlyExemptCeiling NUMERIC(12,2) DEFAULT 10417.00,
                    BirBonusExemptCap NUMERIC(12,2) DEFAULT 90000.00,
                    UpdatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
                    UpdatedByUsername VARCHAR(50) DEFAULT 'system'
                );
                ";

                conn.Execute(sql);

                // Auto-migrate column additions to Employees if table already existed
                string migrationSql = @"
                DO $$ 
                BEGIN 
                    BEGIN ALTER TABLE Employees ADD COLUMN BirthDate DATE; EXCEPTION WHEN duplicate_column THEN END;
                    BEGIN ALTER TABLE Employees ADD COLUMN Gender VARCHAR(20) DEFAULT 'Male'; EXCEPTION WHEN duplicate_column THEN END;
                    BEGIN ALTER TABLE Employees ADD COLUMN CivilStatus VARCHAR(30) DEFAULT 'Single'; EXCEPTION WHEN duplicate_column THEN END;
                    BEGIN ALTER TABLE Employees ADD COLUMN ContactNumber VARCHAR(50); EXCEPTION WHEN duplicate_column THEN END;
                    BEGIN ALTER TABLE Employees ADD COLUMN EmailAddress VARCHAR(100); EXCEPTION WHEN duplicate_column THEN END;
                    BEGIN ALTER TABLE Employees ADD COLUMN Address TEXT; EXCEPTION WHEN duplicate_column THEN END;
                    BEGIN ALTER TABLE Employees ADD COLUMN EmergencyContactName VARCHAR(100); EXCEPTION WHEN duplicate_column THEN END;
                    BEGIN ALTER TABLE Employees ADD COLUMN EmergencyContactPhone VARCHAR(50); EXCEPTION WHEN duplicate_column THEN END;
                    BEGIN ALTER TABLE Employees ADD COLUMN ContractType INT DEFAULT 2; EXCEPTION WHEN duplicate_column THEN END;
                    BEGIN ALTER TABLE Employees ADD COLUMN ContractStatus INT DEFAULT 1; EXCEPTION WHEN duplicate_column THEN END;
                    BEGIN ALTER TABLE Employees ADD COLUMN ContractEndDate DATE; EXCEPTION WHEN duplicate_column THEN END;
                    BEGIN ALTER TABLE Employees ADD COLUMN SssDeductionMode INT DEFAULT 1; EXCEPTION WHEN duplicate_column THEN END;
                    BEGIN ALTER TABLE Employees ADD COLUMN CustomSssAmount NUMERIC(12,2) DEFAULT 0.00; EXCEPTION WHEN duplicate_column THEN END;
                    BEGIN ALTER TABLE Employees ADD COLUMN PhilHealthDeductionMode INT DEFAULT 1; EXCEPTION WHEN duplicate_column THEN END;
                    BEGIN ALTER TABLE Employees ADD COLUMN CustomPhilHealthAmount NUMERIC(12,2) DEFAULT 0.00; EXCEPTION WHEN duplicate_column THEN END;
                    BEGIN ALTER TABLE Employees ADD COLUMN PagIbigDeductionMode INT DEFAULT 1; EXCEPTION WHEN duplicate_column THEN END;
                    BEGIN ALTER TABLE Employees ADD COLUMN PagIbigEmployeeAmount NUMERIC(12,2) DEFAULT 200.00; EXCEPTION WHEN duplicate_column THEN END;
                    BEGIN ALTER TABLE Employees ADD COLUMN IsMinimumWageEarner BOOLEAN DEFAULT FALSE; EXCEPTION WHEN duplicate_column THEN END;
                    BEGIN ALTER TABLE Employees ADD COLUMN IsTaxExempt BOOLEAN DEFAULT FALSE; EXCEPTION WHEN duplicate_column THEN END;
                    BEGIN ALTER TABLE Employees ADD COLUMN ContributionSchedule INT DEFAULT 1; EXCEPTION WHEN duplicate_column THEN END;
                END $$;
                ";
                conn.Execute(migrationSql);

                // Seed Default Company Profile
                int compCount = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM CompanyProfiles;");
                if (compCount == 0)
                {
                    conn.Execute(@"
                    INSERT INTO CompanyProfiles (Id, CompanyName, TradeName, CompanyAddress, ContactNumber, EmailAddress,
                                               EmployerSssNumber, EmployerPhilHealthNumber, EmployerPagIbigNumber, EmployerTin,
                                               AuthorizedSignatoryName, AuthorizedSignatoryTitle, UpdatedByUsername)
                    VALUES (1, 'Genetian Enterprise Solutions', 'Genetian GB', 'General Santos City, South Cotabato, Philippines',
                            '+63 (083) 552-0000', 'info@genetian.ph', '09-1234567-8', '12-345678901-2', '1234-5678-9012',
                            '000-123-456-000', 'Maria Santos', 'HR & Administrative Director', 'system');");
                }

                // Seed Default Branches if empty
                int branchCount = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM Branches;");
                if (branchCount == 0)
                {
                    conn.Execute(@"
                    INSERT INTO Branches (Code, Name, Location, IsActive)
                    VALUES 
                        ('MAIN', 'Main Office - Headquarter', 'General Santos City', TRUE),
                        ('DVO', 'Davao City Branch', 'Davao City', TRUE),
                        ('CEB', 'Cebu Operations Hub', 'Cebu City', TRUE);");
                }

                // Seed Default Statutory Settings if not present
                int statCount = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM StatutorySettings;");
                if (statCount == 0)
                {
                    conn.Execute(@"
                    INSERT INTO StatutorySettings (Id, SssTotalRatePercent, SssEmployeeSharePercent, SssEmployerSharePercent,
                                                  SssMinSalaryCredit, SssMaxSalaryCredit, PhilHealthTotalRatePercent,
                                                  PhilHealthEmployeeSharePercent, PhilHealthEmployerSharePercent,
                                                  PhilHealthMinSalaryCredit, PhilHealthMaxSalaryCredit,
                                                  PagIbigEmployeeStandardMonthly, PagIbigEmployerStandardMonthly,
                                                  BirSemiMonthlyExemptCeiling, BirBonusExemptCap, UpdatedByUsername)
                    VALUES (1, 14.00, 4.50, 9.50, 5000.00, 35000.00, 5.00, 2.50, 2.50, 10000.00, 100000.00, 200.00, 200.00, 10417.00, 90000.00, 'system');");
                }

                // Seed Default Accounts if no users exist
                int userCount = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM Users;");
                if (userCount == 0)
                {
                    string seedUsers = @"
                    INSERT INTO Users (Username, PasswordHash, FullName, Email, Role)
                    VALUES 
                        ('admin', 'admin123', 'Genetian Administrator', 'admin@genetian.ph', 1),
                        ('hr', 'hr123', 'Genetian HR Manager', 'hr@genetian.ph', 2),
                        ('acct', 'acct123', 'Genetian Accountant', 'acct@genetian.ph', 3);
                    ";
                    conn.Execute(seedUsers);
                }
            }
            catch (Exception ex)
            {
                // Silently handle if server is not available yet at application start
                System.Diagnostics.Debug.WriteLine($"Database initialization note: {ex.Message}");
            }
        }
    }
}
