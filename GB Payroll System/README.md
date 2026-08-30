# Genetian Payroll System

<div align="center">

**Enterprise Philippine Payroll, HRIS, Biometric Timekeeping & Statutory Compliance Desktop Application**

Built with **.NET 10 WPF**, **PostgreSQL (Npgsql)**, and **Dapper** — Engineered for **Windows 10 & Windows 11**.

</div>

---

## 📌 Overview

The **Genetian GB Payroll System** is an enterprise-grade desktop solution tailored specifically to Philippine labor laws, tax regulations, and modern human resource workflows. It integrates employee records management (201 files), biometric punch ingestion, intelligent shift and attendance calculation, leave and holiday tracking, statutory compliance (SSS, PhilHealth, Pag-IBIG, BIR), and automated payslip and remittance generation into a single, cohesive desktop platform.

The system combines a **Modern Enterprise Fluent UI (Windows 10 & 11)** with **Advanced Human-Computer Interaction (HCI)** ergonomics and strict compliance with the **Philippine Data Privacy Act of 2012 (RA 10173)**.

---

## ✨ Core Features & System Modules

### 1. 🔐 Authentication & Role-Based Access Control (RBAC)
* **Pre-Configured Roles**: `Admin`, `HR`, `Accounting`, `Management`, and `Staff`.
* **Cryptographic Security**: Passwords protected using salted `BCrypt.Net` hashes.
* **Role Gating**: Navigation views and sensitive operational buttons (e.g. salary edits, user creation, payroll finalization) dynamically adjust based on user privileges.
* **Session Auditing**: Tracks active user sessions, last login timestamps, and system change history.

### 2. 👥 Employee HRIS & 201 File Repository
* **Complete Biographical Profile**: Employee Code, Biometric User ID, Full Name, Birthdate, Gender, Civil Status, Contact Info, Emergency Contacts, Branch, Department, Position, and Bank Account.
* **Flexible Pay Structures**: Supports both **Monthly-Paid** and **Daily-Paid** employees.
* **Configurable Working Days Factor**: Supports standard annual working days factors (`313`, `261`, or `365` days/year) to accurately calculate daily, hourly, and minute rates:
  $$\text{Daily Rate} = \frac{\text{Monthly Rate} \times 12}{\text{Working Days Factor}}$$
* **Employee Contracts Lifecycle**: Tracks contract types (Probationary, Regular, Contractual, Casual), start/end dates, position assignments, and digital document attachments.
* **Salary Promotion History**: Full audit trail of position changes, salary increments, effective dates, justifications, and approving officers.
* **Employment History**: Records past internal and external work history, separation reasons, and rehire eligibility flags.
* **Categorized 201 Documents**: Centralized digital repository for Resumes, Signed Contracts, NBI Clearances, Government IDs, Medical Records, and Certificates.

### 3. ⏱️ Timekeeping, Attendance & Biometric Ingestion
* **Biometric Log Ingestion**:
  - Direct network pull via **Biometric Device IP**.
  - Guided **CSV / Excel Import Wizard** with real-time preview and mapping.
* **Intelligent Shift Pairing**: Automatically pairs morning `TimeIn` and afternoon/evening `TimeOut` punches.
* **Precision Attendance Engine**:
  - Calculates `LateMinutes` (Tardiness) and `UndertimeMinutes` based on scheduled shifts.
  - Automatically calculates **Overtime Hours (125% regular OT premium)** beyond 8 hours.
  - Automatically calculates **Night Differential Hours (110% premium)** between 10:00 PM and 6:00 AM.
* **Manual Adjustment Audit**: Record manual punch corrections with mandatory reason notes and user timestamps (`AdjustedByUsername`).

### 4. 🏖️ Leave Management & Holiday Calendar
* **Annual Leave Balances**: Automatic yearly balance tracking for Vacation Leave (VL), Sick Leave (SL), and Emergency Leave (EL) credits.
* **Leave Application Workflow**: Multi-day and half-day applications with status tracking (`Pending`, `Approved`, `Rejected`).
* **Philippine Holiday Engine**:
  - **Regular Holidays**: 200% pay for worked shifts, 100% pay for unworked regular employees.
  - **Special Non-Working Holidays**: 130% pay for worked shifts.
  - **Branch / LGU Scope**: Differentiates between National holidays and local branch-specific declarations.

### 5. 🇵🇭 Philippine Statutory Payroll Computation Engine
* **Automated Gross-to-Net Algorithm**:
  $$\text{Gross Pay} = \text{Basic Pay} + \text{Overtime Pay} + \text{Night Diff Pay} + \text{Holiday Pay} + \text{Allowances}$$
  $$\text{Total Deductions} = \text{Tardiness} + \text{Undertime} + \text{Absences} + \text{SSS} + \text{PhilHealth} + \text{Pag-IBIG} + \text{Tax} + \text{Other}$$
  $$\text{Net Pay} = \text{Gross Pay} - \text{Total Deductions}$$
* **SSS Deductions (2024–2026 Regulations)**: Clamps compensation within Monthly Salary Credit (MSC) floors (₱5,000) and ceilings (₱35,000) with automatic 4.5% employee / 9.5% employer share allocation.
* **PhilHealth Deductions**: Applies 5.0% total premium (2.5% Employee / 2.5% Employer) on compensation clamped between ₱10,000 and ₱100,000.
* **Pag-IBIG (HDMF) Deductions**: Standard ₱200/month employee and ₱200/month employer contributions (or customizable amounts).
* **BIR Withholding Tax (TRAIN Law)**: Dynamically computes graduated tax on taxable income ($15\%$, $20\%$, $25\%$, $30\%$, $35\%$) with semi-monthly ₱10,417 exempt thresholds.
* **Flexible Cutoff Scheduling**: Configurable deduction schedules (`Split Both Cutoffs`, `First Cutoff Only`, `Second Cutoff Only`).
* **Exemptions & Minimum Wage**: Fully supports Minimum Wage Earner (MWE) tax exemption and customized statutory exemptions.
* **Payslip Generation**: Instant printable and exportable digital payslips with detailed earnings and deductions breakdown.

### 6. 📊 Statutory Remittance Reports & Exports
* **SSS R-1A / R-3 Report**: Comprehensive contribution schedules ready for SSS portal submission.
* **PhilHealth RF-1 Report**: Remittance listings categorized by employee PIN.
* **Pag-IBIG MCRF**: Monthly collection remittance schedule.
* **BIR 1601-C & AlphaList**: Monthly compensation, non-taxable statutory deductions, and tax withheld reports.

### 7. ⚙️ Dynamic Statutory Configuration
* Admins can adjust statutory contribution rates, MSC floors/ceilings, and tax exemption caps directly in the application UI without requiring code changes or recompilations.

---

## 🎨 Modern UI/UX & Human-Computer Interface (HCI) Design

Designed specifically to provide a clean, modern, and ergonomic desktop experience across **Windows 10 & Windows 11**:

| Design Aspect | Implementation in Genetian GB Payroll |
| :--- | :--- |
| **Color Palette** | Deep Sapphire (`#0A4D9C`), Midnight Navy (`#06254A`), Vivid Blue (`#002CFA`), Canvas (`#F8FAFC`), Text (`#0F172A`) |
| **Geometry & Elevation** | Modern soft-rounded corners (`8px` to `12px`), drop-shadow card containers (`BlurRadius="18"`, `Opacity="0.06"`) |
| **Typography** | `Segoe UI Variable` with graceful fallback to `Segoe UI` on Windows 10 |
| **High-DPI Scaling** | `SnapsToDevicePixels="True"` and `UseLayoutRounding="True"` for crisp 100%–200% multi-monitor DPI scaling |
| **Semantic Pill Badges** | Soft pastel backgrounds with vibrant text (`#DCFCE7` Active, `#FEF3C7` Pending, `#FEE2E2` Absent/Closed) |
| **High-Precision Grids** | Prohibits unformatted grids; text left-aligned, status badges centered, currency right-aligned (`₱#,##0.00`) |
| **Operational Ergonomics** | Search-as-you-type instant filtering, helpful button tooltips, and informative empty states with Call-To-Action buttons |
| **Non-Blocking UI** | All database queries and batch computations run asynchronously (`async`/`await`) with loading indicators |
| **Error Handling** | Translates technical database errors into plain, helpful guidance on how to fix the issue |

---

## 🛡️ Security & Philippine Data Privacy (RA 10173)

1. **PII Masking**: Sensitive identification numbers (TIN, SSS, PhilHealth, Pag-IBIG, and bank accounts) are automatically masked (`000-***-***-000` / `****-****-1234`) in public overviews.
2. **Salary Confidentiality**: Compensation rates and net pays are strictly role-gated.
3. **100% Parameterized Queries**: All database interactions use Dapper parameters to eliminate SQL injection risks.
4. **Transaction Safety**: Multi-table payroll calculations and data updates execute within explicit database transactions.
5. **Sanitized Diagnostics**: Internal database connection strings and raw stack traces are never exposed in user dialogs.

---

## 📁 System Architecture & Directory Structure

```
GB_Payroll_System/
├── Assets/                               # Application icons, logos, and branding
│   ├── favicon.ico
│   └── logo.png
├── Models/                               # Domain POCOs, Enums, and Data Transfer Objects
│   ├── Attendance.cs                     # Attendance records, tardiness, OT, night diff
│   ├── BiometricLog.cs                   # Raw biometric punch logs
│   ├── Branch.cs                         # Company branch locations
│   ├── Employee.cs                       # Employee master profile & 201 information
│   ├── EmployeeContract.cs               # Contract records (Probationary, Regular, etc.)
│   ├── EmployeeDocument.cs               # 201 digital document metadata
│   ├── EmploymentHistory.cs              # Past company & internal role history
│   ├── Holiday.cs                        # Regular, Special, and Local holidays
│   ├── LeaveModels.cs                    # Leave balances and applications
│   ├── PayType.cs                        # Pay types (Monthly vs. Daily) & deduction modes
│   ├── PayrollPeriod.cs                  # Cutoff period definitions
│   ├── PayrollRecord.cs                  # Detailed computed payroll line items
│   ├── SalaryPromotionHistory.cs         # Salary & position promotion audit log
│   ├── StatutorySettings.cs              # Statutory contribution brackets and ceilings
│   ├── User.cs                           # User accounts
│   └── UserRole.cs                       # RBAC Roles (Admin, HR, Accounting, etc.)
├── Data/                                 # Repositories executing Dapper SQL Queries
│   ├── AttendanceRepository.cs
│   ├── ContractRepository.cs
│   ├── DatabaseInitializer.cs            # Automatic schema generation & seeding
│   ├── DbConnectionFactory.cs            # PostgreSQL connection management
│   ├── DocumentRepository.cs
│   ├── EmployeeRepository.cs
│   ├── EmploymentHistoryRepository.cs
│   ├── GovernmentReportRepository.cs
│   ├── HolidayRepository.cs
│   ├── LeaveRepository.cs
│   ├── PayrollRepository.cs
│   ├── SalaryPromotionRepository.cs
│   ├── StatutorySettingsRepository.cs
│   └── UserRepository.cs
├── Services/                             # Business Logic & Calculation Engines
│   ├── AttendanceService.cs              # Rate conversions, shifts, tardiness, OT logic
│   ├── AuthService.cs                    # Session state, BCrypt verification
│   ├── CurrencyInputHelper.cs            # Live currency formatting helper
│   ├── PayrollService.cs                 # Gross-to-net computation engine
│   └── PhilippineDeductionService.cs     # SSS, PhilHealth, Pag-IBIG, and BIR tax calculators
├── Views/                                # XAML Presentation Layer & Code-Behind
│   ├── AttendanceCorrectionDialog.xaml   # Manual attendance adjustment modal
│   ├── AttendanceView.xaml               # Daily attendance & timekeeping view
│   ├── BiometricImportWizard.xaml        # Step-by-step biometric log import wizard
│   ├── EmployeeFormDialog.xaml           # Employee add/edit dialog with tabbed 201 sections
│   ├── EmployeeView.xaml                 # Employee directory, cards, and grid
│   ├── EmploymentHistoryDialog.xaml      # Past work experience dialog
│   ├── GovernmentReportsView.xaml        # SSS, PhilHealth, Pag-IBIG, BIR report generator
│   ├── HolidayFormDialog.xaml            # Holiday declaration dialog
│   ├── HolidayView.xaml                  # Holiday calendar management
│   ├── LeaveApplicationDialog.xaml       # Leave filing dialog
│   ├── LeaveManagementView.xaml          # Leave balances & approval queue
│   ├── LoginWindow.xaml                  # Modern login window
│   ├── PayrollPeriodDialog.xaml          # New cutoff period dialog
│   ├── PayrollView.xaml                  # Payroll computation, review, and finalize view
│   ├── PayslipWindow.xaml                # Digital printable payslip window
│   ├── ResetPasswordDialog.xaml          # Password update dialog
│   ├── SalaryPromotionDialog.xaml        # Salary promotion record dialog
│   ├── SettingsView.xaml                 # Statutory tables & system user settings
│   └── UserFormDialog.xaml               # System user creation dialog
├── App.xaml / App.xaml.cs                # Design tokens, global resources, app startup
├── MainWindow.xaml / MainWindow.xaml.cs  # Main dashboard navigation container
└── GB Payroll System.csproj              # .NET 10.0 WPF Project File
```

---

## 🚀 Getting Started

### Prerequisites
* **Operating System**: Windows 10 (Build 1809 or higher) or Windows 11
* **Runtime / SDK**: [.NET 10.0 SDK](https://dotnet.microsoft.com/download) (or .NET 10 Desktop Runtime)
* **Database**: [PostgreSQL](https://www.postgresql.org/) (Version 12+)

### Database Configuration
Ensure PostgreSQL is running, then verify or adjust the connection string in [`Data/DbConnectionFactory.cs`](file:///c:/Users/H0me/source/repos/Genetian%20Payroll/GB%20Payroll%20System/Data/DbConnectionFactory.cs).

On application launch, `DatabaseInitializer.Initialize()` automatically:
1. Creates all required tables (`Employees`, `Attendances`, `PayrollRecords`, `StatutorySettings`, etc.) if they do not exist.
2. Applies necessary column migrations.
3. Seeds default Philippine statutory tables and initial administrative users.

### Default Login Accounts

| Username | Password | Role | Access Scope |
| :--- | :--- | :--- | :--- |
| `admin` | `admin123` | **Admin** | Full system access, users, database, statutory settings |
| `hr` | `hr123` | **HR** | Full HRIS, 201 files, attendance, leave, holidays, payroll |
| `acct` | `acct123` | **Accounting** | Payroll processing, attendance, government remittance reports |

> *Note: Please change the default passwords after your initial login via **Settings $\to$ User Management**.*

---

## 🛠️ Building & Running

To build and run the application from the command line:

```powershell
# Restore dependencies and build
dotnet build "GB Payroll System.csproj"

# Run application
dotnet run --project "GB Payroll System.csproj"
```

---

## 📖 Developer & AI Agent Guidelines

For developers and AI coding agents working on this codebase:
* Full architectural patterns, HCI mandates, and design tokens are detailed in [`.agents/skills/gb-payroll-system/SKILL.md`](file:///c:/Users/H0me/source/repos/Genetian%20Payroll/GB%20Payroll%20System/.agents/skills/gb-payroll-system/SKILL.md).
* Core coding standards are summarized in [`AGENTS.md`](file:///c:/Users/H0me/source/repos/Genetian%20Payroll/GB%20Payroll%20System/AGENTS.md) and [`GEMINI.md`](file:///c:/Users/H0me/source/repos/Genetian%20Payroll/GB%20Payroll%20System/GEMINI.md).

---

## 📄 License & Confidentiality
© 2026 Genetian GB Payroll System. All rights reserved. Built for enterprise Philippine HR and Payroll operations.
