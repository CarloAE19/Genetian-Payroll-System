---
name: gb-payroll-system
description: >-
  Definitive architectural guide, Modern Enterprise UI/UX Design System (Windows 10 & Windows 11 Fluent / SaaS standard), 
  Advanced HCI & User-Friendliness standards, code integrity patterns, and Philippine Data Privacy rules 
  for the Genetian GB Payroll System (.NET 10 WPF + PostgreSQL + Dapper).
---

# Genetian GB Payroll System — Modern Enterprise UI/UX, Advanced HCI, User-Friendliness & Architecture Blueprint

This document is the definitive engineering, UX, and operational standard for the **Genetian GB Payroll System**. It guarantees that any UI, code, or feature created in this repository is **exceptionally user-friendly**, meets **modern world-class desktop UI/UX standards (Windows 10 & Windows 11 Fluent / Modern Enterprise SaaS)**, adheres to **Advanced Human-Computer Interaction (HCI)** principles, ensures **non-breaking code stability**, and enforces **zero data leakage (Philippine RA 10173)**.

---

## 1. System Anatomy & Tech Stack

```
GB_Payroll_System/
├── Assets/                 # Branding (logo.png, favicon.ico, app_icon.ico)
├── Models/                 # Domain POCOs, Enums, and DTOs
├── Data/                   # Repositories (Dapper SQL queries & transactions)
├── Services/               # Business logic, statutory calculations, auth
├── Views/                  # XAML Windows, UserControls, and Dialogs
├── App.xaml / App.xaml.cs  # Global styles, color tokens, startup bootstrap
└── MainWindow.xaml / .cs   # Navigation hub, role-based dashboard, container
```

| Layer | Technology | Key Details |
| :--- | :--- | :--- |
| **Framework** | .NET 10.0 WPF (`net10.0-windows`) | High-DPI support, XAML resource dictionaries, MVVM / Repository |
| **OS Target** | Windows 10 (1809+) & Windows 11 | Cross-version Fluent UI styling, graceful font/effect fallbacks, high-DPI Per-Monitor V2 |
| **Database** | PostgreSQL via `Npgsql` (v10+) | Hosted on PostgreSQL server, relational schemas, foreign keys, timestamps |
| **Data Access** | `Dapper` (v2.1+) | 100% Parameterized SQL, mapped to domain POCOs |
| **Security** | `BCrypt.Net-Next` | Salted password hashing, Role-Based Access Control (RBAC) |
| **Domain** | Philippine Statutory Compliance | SSS, PhilHealth, Pag-IBIG (HDMF), BIR Withholding Tax (TRAIN Law) |

---

## 2. World-Class User-Friendliness & Operational Ergonomics

To make the system effortless, intuitive, and welcoming for HR officers, accountants, and non-technical staff:

### 2.1 Plain-Language UI & Contextual Guidance
* **Zero Jargon**: Use familiar, natural terminology across all screens (e.g. *"Regular Holiday (200% Pay)"*, *"Semi-Monthly 1st Cutoff (1st–15th)"*, *"Net Pay After Deductions"* instead of cryptic system codes).
* **Helpful Tooltips**: Every icon button, configuration field, and statutory toggle must include an informative `ToolTip` explaining what it does and why (e.g., `ToolTip="Clamps employee contributions to ₱35,000 maximum monthly salary credit under SSS 2025 table"`).
* **Actionable Empty States**: When lists, grids, or search results are empty, **never leave a stark blank table**. Display a friendly icon, a brief description, and a direct Call-To-Action button (e.g. *"No employees found matching 'Finance'. [Clear Filter] or [+ Add New Employee]"*).

### 2.2 Instant Feedback & Frictionless Search
* **Search-As-You-Type**: Filter tables instantly as the user types (name, department, employee code) without requiring an extra "Submit Search" click.
* **Smart Dropdowns & Branch Switchers**: Auto-select active branches and current payroll periods by default so users can complete daily tasks in minimal clicks.
* **One-Click Actions**: Provide dedicated one-click shortcuts for frequent tasks (e.g., *"Calculate All Drafts"*, *"Export All Payslips to PDF"*, *"Send SSS Summary to Excel"*).

### 2.3 Forgiving Design & User Confidence (Zero Anxiety)
* **Safe Draft vs. Finalize Modes**: Users can compute, inspect, adjust, and re-run draft payroll calculations with zero penalty. Finalizing/locking a period is an explicit, clear milestone.
* **Friendly, Actionable Error Messages**: Catch exceptions and translate them into clear human guidance with steps to fix:
  - *Poor:* `"NpgsqlException: foreign key violation constraint fk_emp_branch"`
  - *User-Friendly:* `"Unable to save employee: The selected Branch no longer exists. Please choose an active branch from the list."`
* **Confirmation Dialogs with Impact Summaries**: Before deleting a record or resetting a cutoff, show a modal specifying the exact items affected (e.g. *"Deleting this employee contract will not delete past historical payslips. Do you want to proceed?"*).

---

## 3. Modern World-Class UI/UX Design System (Windows 10 & Windows 11)

### 3.1 Cross-Windows Compatibility & Visual Refinement
* **Unified Fluent Aesthetic**: Soft rounded geometry (`CornerRadius="8"` to `12"`), subtle elevations, and clean cards across both Windows 10 and Windows 11.
* **Font Fallback Stack**: `FontFamily="Segoe UI Variable, Segoe UI, -apple-system, Arial, sans-serif"`, ensuring native typography on Windows 10 (`Segoe UI`) and Windows 11 (`Segoe UI Variable`).
* **High-DPI Scaling**: `SnapsToDevicePixels="True"` and `UseLayoutRounding="True"` on all Windows and UserControls for razor-sharp rendering on 100%–200% displays.

### 3.2 Color Palette & Modern Design Tokens (`App.xaml`)
* **Signature Brand Gradients**:
  - Primary Window Background: Deep Sapphire (`#0A4D9C`) $\to$ Midnight Navy (`#06254A`)
  - Accent Primary Blue: `#002CFA` (Hover: `#0022C8`, Active/Pressed: `#001CA3`)
* **Surfaces, Elevation & Cards**:
  - Main Background: `#F8FAFC` (Slate Canvas)
  - Card Surfaces: `#FFFFFF` with smooth drop shadows (`BlurRadius="18"`, `Opacity="0.06"`, `Direction="270"`, `ShadowDepth="3"`, `Color="#0F172A"`)
  - Container Borders: `1px` solid `#E2E8F0` or `#EDF2F7`
* **Modern Semantic Pill Badges (Soft Pastel Background + Vibrant Ink)**:
  - **Success / Active / Present / Approved**: Background `#DCFCE7`, Foreground `#15803D`, Border `#BBF7D0`
  - **Warning / Pending / Probationary / On-Leave**: Background `#FEF3C7`, Foreground `#B45309`, Border `#FDE68A`
  - **Danger / Absent / Rejected / Closed**: Background `#FEE2E2`, Foreground `#B91C1C`, Border `#FECACA`
  - **Info / Regular / Draft**: Background `#DBEAFE`, Foreground `#1D4ED8`, Border `#BFDBFE`
  - **Neutral / Inactive**: Background `#F1F5F9`, Foreground `#475569`, Border `#E2E8F0`

### 3.3 High-Precision DataGrid Standards
Every data table must look modern, spacious, and readable:
1. **Never allow default auto-generated columns**: Always set `AutoGenerateColumns="False"`.
2. **Column Alignment & Currency Formatting**:
   - Descriptive text columns: **Left-aligned**.
   - Identifiers, Dates, and Status Badges: **Centered**.
   - Numeric quantities, hours, and monetary values: **Always Right-Aligned** with `StringFormat='₱{0:N2}'` or `StringFormat='{}{0:N2}'`.
3. **Table Ergonomics**:
   - Header style: Subtle slate background (`#F1F5F9`), bold muted text (`#475569`), `12px` height padding, clear sorting glyphs.
   - Row styling: Alternating subtle row tint (`#FFFFFF` / `#F8FAFC`), smooth hover state (`#F0F4F8`).
   - Cell padding: Minimum `8px,10px` vertical/horizontal padding to avoid dense, cramped spreadsheet appearance.

### 3.4 Modern Form Controls & Ergonomics
* **Modern Inputs (`ModernInputStyle`)**: Height `42-46px`, soft background (`#F8FAFC`), subtle border (`#E2E8F0`), focus ring (`#002CFA`, `1.5px` border thickness with gentle glow).
* **Live Formatted Inputs**: Always format currency fields on blur using `CurrencyInputHelper` or structured masked inputs.
* **Persistent Visual Labels**: Every input must feature an explicit label above the box (never rely solely on disappearing placeholder text).

---

## 4. Advanced Human-Computer Interaction (HCI) Principles

### 4.1 Cognitive Load Reduction & Hick's Law
* **Progressive Disclosure**: Break complex or multi-parameter workflows (e.g. Biometric Import, New Payroll Run, Employee Onboarding) into step-by-step wizards or categorized tab views with breadcrumbs.
* **Visual Hierarchy**: Group related inputs into clearly bounded card panels with subtitles. Clear contrast between primary actions (Solid Blue `#002CFA`), secondary actions (Outlined Gray `#E2E8F0`), and destructive actions (Crimson Outline/Fill `#DC2626`).

### 4.2 Fitts's Law & Touch/Pointer Ergonomics
* **Generous Target Sizes**: Interactive buttons, dropdowns, and clickable rows must have a minimum height of `38px-46px` and sufficient horizontal padding (`16px-24px`).
* **Visual Affordance & Micro-interactions**: Hover transitions (`IsMouseOver`) and active press feedback on buttons and interactive rows.

### 4.3 Zero-Lag Non-Blocking UI (Nielsen's Responsiveness Principle)
* **Async Database & Processing**: All database queries, batch payroll computations, biometric log parsing, and report exports **must** execute asynchronously (`await Task.Run(...)` or async Dapper calls) off the UI thread.
* **Loading Skeletons & Progress Rings**: Display indeterminate progress rings or skeleton placeholders with informative status messages ("Ingesting 2,400 biometric punches...", "Computing PhilHealth 5% contribution brackets...").
* **Non-Disruptive Feedback**: Lightweight in-app toast/banner alerts for non-critical confirmations.

### 4.4 Error Prevention & Keyboard Accessibility (Norman's Principles)
* **Real-Time Input Validation**: Mark erroneous inputs with a clear crimson border (`#E53935`) and an inline validation tooltip/helper label.
* **Full Keyboard Accessibility**:
  - Explicit `TabIndex` sequence across all form fields.
  - `IsDefault="True"` bound to primary confirm buttons; `IsCancel="True"` bound to cancel/escape buttons.
  - `Enter` key triggers search in filter boxes.

---

## 5. What Is Inside This System (Domain & Architectural Blueprint)

```mermaid
graph LR
    subgraph "HRIS & 201 Filing"
        EMP[Employees & 201 Files]
        CON[Contracts & Pay Rates]
        PRO[Salary Promotions]
        DOC[Document Storage]
    end

    subgraph "Timekeeping & Biometrics"
        BIO[Biometric Log Ingestion]
        ATT[Attendance & Overtime Engine]
        HOL[Holiday Multipliers 200%/130%]
        LV[Leave Balances & Approvals]
    end

    subgraph "Philippine Payroll Engine"
        PAY[Semi-Monthly / Monthly Engine]
        SSS[SSS MSC Brackets]
        PH[PhilHealth 5% Split]
        PAG[Pag-IBIG Monthly Share]
        BIR[BIR TRAIN Law Withholding Tax]
    end

    subgraph "Compliance & Operations"
        GOV[SSS R-1A / PhilHealth RF-1 / MCRF / AlphaList]
        SET[Dynamic Statutory Settings]
        USR[Role-Based Access Control BCrypt]
    end

    EMP --> ATT --> PAY --> GOV
    CON --> PAY
    LV --> PAY
    HOL --> PAY
    BIO --> ATT
    SET --> PAY
    USR --> EMP
```

### 5.1 Module Directory
1. **Auth & RBAC (`AuthService.cs`, `UserRepository.cs`)**:
   - Roles: `Admin`, `HR`, `Accounting`, `Management`, and `Staff`.
   - Salted `BCrypt.Net` hash storage and dynamic role-based navigation rendering.
2. **Employee HRIS & 201 Filing (`EmployeeRepository.cs`, `Models/Employee.cs`)**:
   - Personal biographics, branch assignment, daily/monthly pay rates, working days factors (`313`, `261`, `365`), bank accounts, and statutory numbers.
   - Statutory deduction modes (`Auto`, `FixedAmount`, `Exempt`) and deduction schedules (`SplitBothCutoffs`, `FirstCutoffOnly`, `SecondCutoffOnly`).
3. **Contracts, Promotions & Document Management (`ContractRepository.cs`, `SalaryPromotionRepository.cs`, `DocumentRepository.cs`)**:
   - Tracks contract progression (Probationary, Regular, Contractual).
   - Audit trail of salary increases and position adjustments with approval metadata.
   - Categorized document storage for 201 requirements (Resume, NBI, IDs, Medical).
4. **Timekeeping, Attendance & Biometrics (`AttendanceService.cs`, `BiometricImportWizard.xaml`)**:
   - Direct device IP sync and CSV/Excel file import wizard.
   - Shift pairing logic, tardiness & undertime calculation against shift schedules.
   - Overtime ($125\%$) and Night Differential ($110\%$ from 10:00 PM to 6:00 AM).
   - Manual attendance adjustments with audit tracking.
5. **Leave & Holiday Management (`LeaveRepository.cs`, `HolidayRepository.cs`)**:
   - Yearly leave credits (VL, SL, EL) and approval workflows.
   - Regular (200% worked, 100% unworked) and Special Non-Working (130% worked) national and branch-level holiday multipliers.
6. **Philippine Payroll Computation Engine (`PayrollService.cs`, `PhilippineDeductionService.cs`)**:
   - Complete gross-to-net calculation algorithm.
   - **SSS**: MSC clamped between ₱5,000 and ₱35,000 ($4.5\%$ EE / $9.5\%$ ER).
   - **PhilHealth**: $5\%$ premium ($2.5\%$ EE / $2.5\%$ ER) on compensation clamped between ₱10,000 and ₱100,000.
   - **Pag-IBIG (HDMF)**: Standard ₱200/month employee and employer contributions.
   - **BIR Withholding Tax**: Semi-monthly bracket calculation under the TRAIN Law with minimum wage earner exemptions.
7. **Government Remittance Reports (`GovernmentReportRepository.cs`)**:
   - Export formats for SSS (R-1A, R-3), PhilHealth (RF-1), Pag-IBIG (MCRF), and BIR (1601-C / AlphaList).
8. **Dynamic Statutory Configuration (`StatutorySettingsRepository.cs`)**:
   - In-app runtime configuration of statutory contribution ceilings and deduction rates.

---

## 6. Security & Zero Data Leakage Rules (RA 10173 Compliance)

1. **PII Masking**: TIN, SSS, PhilHealth, Pag-IBIG, and bank account numbers must be masked in general overview tables (e.g., `000-***-***-000` or `****-****-1234`).
2. **Salary Confidentiality**: Basic rates and net pays are strictly gated behind role permissions (`Admin`, `HR`, `Accounting`).
3. **100% Parameterized Queries**: All database queries must use Dapper parameters. Never concatenate strings into SQL commands.
4. **Transaction Integrity**: Multi-table data mutations must run inside explicit database transactions.
5. **Sanitized Diagnostics**: Never expose database connection strings or raw exception stack traces in user-facing dialogs.

---

## 7. User-Friendliness & Quality Checklist

Before completing any task or code change in this repository, verify:
- [ ] **Is it user-friendly?** Are labels in clear plain language? Are tooltips present on buttons/inputs?
- [ ] **Are empty states informative?** Do empty tables show a friendly message and a "+ Add" or "Import" button?
- [ ] **Are error messages helpful?** Do error messages give clear guidance instead of technical stack traces?
- [ ] **Is the layout responsive?** Is content wrapped in `<ScrollViewer>` with zero clipping or text cutoff?
- [ ] **Are visuals modern?** Does the UI use soft corner radiuses (`8px-12px`), pastel pill badges, and Genetian tokens on Windows 10 & 11?
- [ ] **Are numbers formatted?** Are monetary amounts right-aligned and formatted as `₱#,##0.00`?
- [ ] **Is data secure?** Are sensitive statutory IDs masked and SQL queries 100% parameterized with Dapper?
- [ ] **Is UI non-blocking?** Are database queries and heavy computations asynchronous with loading progress indicators?
