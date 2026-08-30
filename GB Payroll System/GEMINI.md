# Agent Guidelines for Genetian GB Payroll System

This repository contains the **Genetian GB Payroll System**, an enterprise Philippine Payroll, HRIS, Biometric Timekeeping, and Statutory Compliance desktop application built with **.NET 10 WPF**, **PostgreSQL (Npgsql)**, and **Dapper**, designed for **Windows 10 & Windows 11**.

## System Architecture & What's Inside

```
GB_Payroll_System/
├── Models/                 # Domain POCOs (Employee, Attendance, Payroll, Leave, Contracts, Documents)
├── Data/                   # Repositories executing 100% Parameterized Dapper SQL
├── Services/               # Business logic, statutory calculations, auth, payroll rules
├── Views/                  # XAML Windows, UserControls, Dialogs, Wizards
├── Assets/                 # Application branding, logos, icons
└── App.xaml / App.xaml.cs  # Design tokens, global resources, app entrypoint
```

### Core System Modules:
1. **Auth & RBAC**: Admin, HR, Accounting, Management, Staff with BCrypt password hashing.
2. **Employee HRIS & 201 Files**: Personal details, monthly/daily pay types, working days factor (313/261/365), statutory IDs, deduction modes, bank info.
3. **Contracts & History**: Probationary/regular contracts, salary promotion audit history, past employment records, categorized document repository.
4. **Timekeeping & Biometrics**: Device IP & CSV/Excel biometric log ingestion, shift pairing, tardiness/undertime/overtime/night-diff engine, manual adjustment audit.
5. **Leave & Holidays**: Vacation/Sick/Emergency leave balances & workflows; Regular (200%), Special Non-Working (130%), and branch-level holiday multipliers.
6. **Philippine Payroll Computation**: Dynamic semi-monthly/monthly calculations, gross-to-net algorithms, SSS/PhilHealth/Pag-IBIG/BIR TRAIN withholding tax brackets.
7. **Statutory Remittance Reports**: SSS R-1A/R-3, PhilHealth RF-1, Pag-IBIG MCRF, BIR 1601-C/AlphaList exports.
8. **Dynamic Settings**: In-app runtime configuration of statutory contribution ceilings and deduction rates.

---

## Core Development & Quality Mandates

### 1. Modern Enterprise UI/UX & Human-Computer Interface (HCI) Excellence (Windows 10 & 11)
* **Cross-Windows Theme & Elevation**: Strictly use the Genetian color tokens (`#0A4D9C`, `#06254A`, `#002CFA`, `#FFFFFF`, `#F8FAFC`, `#E2E8F0`, `#0F172A`, `#64748B`), soft card elevations (`BlurRadius="18"`, `Opacity="0.06"`), and rounded geometry (`CornerRadius="8"` to `12"`), with graceful font fallbacks (`Segoe UI Variable, Segoe UI`) for Windows 10 and 11.
* **Modern Semantic Pill Badges**: Render status enums as modern pill badges with soft pastel backgrounds and high-contrast text (`#DCFCE7` / `#15803D` for Active/Approved, `#FEF3C7` / `#B45309` for Pending/Warning, `#FEE2E2` / `#B91C1C` for Danger/Absent).
* **High-Precision DataGrids**: Prohibit default unformatted grids (`AutoGenerateColumns="False"`). Right-align all numeric/currency values (`₱#,##0.00`), left-align text, and center dates/badges with spacious cell padding (`8px,10px`).
* **Layout Integrity & Ergonomics**: Always wrap variable content in responsive grids or scrollable containers (`ScrollViewer`). Ensure touch/mouse target heights of at least `38px-46px` and persistent input labels.
* **Non-Blocking UI & Micro-interactions**: Asynchronous operations (`async`/`await`) for all database operations, imports, and heavy computations. Provide progress rings with descriptive actions and hover/click feedback.
* **Accessibility & Keyboard Navigation**: Ensure logical `TabIndex`, `IsDefault`, and `IsCancel` button bindings, and crisp high-DPI rendering (`SnapsToDevicePixels="True"`, `UseLayoutRounding="True"`).

### 2. Code Architecture & Non-Breaking Changes
* **Separation of Concerns**:
  - `Models/`: Domain entities and DTOs only.
  - `Data/`: Dapper SQL data access, repository methods, transactions.
  - `Services/`: Payroll algorithms, Philippine statutory deductions (SSS, PhilHealth, Pag-IBIG, BIR tax), authentication.
  - `Views/`: XAML UI and code-behind presentation logic.
* **100% Parameterized Queries**: Never concatenate or interpolate user input into SQL queries.
* **Transaction Safety**: Multi-table payroll calculations and data mutations must be executed inside database transactions.

### 3. Data Privacy & Enterprise Security
* **Philippine Data Privacy Act (RA 10173)**: Mask sensitive PII (TIN, SSS, PhilHealth, Pag-IBIG, bank accounts) in public and overview grids.
* **Password Security**: Store passwords exclusively using salted `BCrypt.Net` hashes.
* **Sanitized Diagnostics**: Never expose database connection strings or raw stack traces in user-facing dialogs.

Refer to the full blueprint and guidelines in [.agents/skills/gb-payroll-system/SKILL.md](file:///c:/Users/H0me/source/repos/Genetian%20Payroll/GB%20Payroll%20System/.agents/skills/gb-payroll-system/SKILL.md).
