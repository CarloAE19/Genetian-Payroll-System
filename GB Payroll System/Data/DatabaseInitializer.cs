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

                CREATE TABLE IF NOT EXISTS Employees (
                    Id SERIAL PRIMARY KEY,
                    EmployeeCode VARCHAR(50) UNIQUE NOT NULL,
                    BiometricUserId VARCHAR(50),
                    FirstName VARCHAR(50) NOT NULL,
                    MiddleName VARCHAR(50),
                    LastName VARCHAR(50) NOT NULL,
                    Department VARCHAR(50),
                    Position VARCHAR(50),
                    BranchId INT REFERENCES Branches(Id),
                    PayType INT NOT NULL DEFAULT 1,
                    BasicRate NUMERIC(12,2) NOT NULL DEFAULT 0.00,
                    WorkingDaysFactor NUMERIC(5,2) DEFAULT 313.00,
                    SssNumber VARCHAR(20),
                    PhilHealthNumber VARCHAR(20),
                    PagIbigNumber VARCHAR(20),
                    TinNumber VARCHAR(20),
                    DateHired DATE DEFAULT CURRENT_DATE,
                    IsActive BOOLEAN DEFAULT TRUE,
                    BankAccountNumber VARCHAR(50)
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

                CREATE TABLE IF NOT EXISTS PakyawRates (
                    Id SERIAL PRIMARY KEY,
                    TaskCode VARCHAR(50) UNIQUE NOT NULL,
                    TaskName VARCHAR(100) NOT NULL,
                    UnitOfMeasure VARCHAR(20) NOT NULL,
                    RatePerUnit NUMERIC(10,2) NOT NULL,
                    IsActive BOOLEAN DEFAULT TRUE
                );

                CREATE TABLE IF NOT EXISTS PakyawEntries (
                    Id SERIAL PRIMARY KEY,
                    EmployeeId INT REFERENCES Employees(Id),
                    PakyawRateId INT REFERENCES PakyawRates(Id),
                    WorkDate DATE NOT NULL,
                    QuantityCompleted NUMERIC(10,2) NOT NULL,
                    UnitRate NUMERIC(10,2) NOT NULL,
                    Remarks TEXT,
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
                    PakyawPay NUMERIC(12,2) DEFAULT 0,
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
                ";

                conn.Execute(sql);

                // Seed Default Admin User if no users exist
                int userCount = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM Users;");
                if (userCount == 0)
                {
                    // Default admin: admin / admin123
                    string seedAdmin = @"
                    INSERT INTO Users (Username, PasswordHash, FullName, Email, Role)
                    VALUES ('admin', 'admin123', 'Genetian Administrator', 'admin@genetian.ph', 1);
                    ";
                    conn.Execute(seedAdmin);
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
