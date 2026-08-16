# 🏗️ Philippine Construction Payroll System — System Context & Rules

## 1. Project Overview & Architecture
This is a multi-user desktop application designed for Philippine construction companies to manage attendance, multi-site job costing, and payroll calculations across multiple branch offices.

- **Framework:** .NET 10 (WPF - Windows Presentation Foundation)
- **Architecture Pattern:** MVVM (Model-View-ViewModel)
- **Database:** PostgreSQL (Networked / Local)
- **Data Access:** Dapper (Micro-ORM) & Npgsql
- **Target User Roles:** Owner, HR, Accounting

---

## 2. Directory & Namespace Structure
All generated code MUST follow this strict folder and namespace structure:

```text
ConstructionPayroll.Desktop/
├── Models/          # Pure C# POCOs (Data entities & database mappings)
├── Data/            # Database connection factories and Dapper repositories
├── Services/        # Business logic, Payroll calculation engines, Biometric network handlers
├── ViewModels/      # MVVM ViewModels (INotifyPropertyChanged, ICommand, async data loading)
└── Views/           # WPF XAML User Controls and Windows