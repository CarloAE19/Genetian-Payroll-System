using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using GB_Payroll_System.Data;
using GB_Payroll_System.Models;

namespace GB_Payroll_System.Views
{
    public partial class GovernmentReportsView : UserControl
    {
        private readonly GovernmentReportRepository _reportRepo = new();
        private readonly PayrollPeriodRepository    _periodRepo = new();

        private List<PayrollPeriod>       _periods        = [];
        private List<SssReportRow>        _sssRows        = [];
        private List<PhilHealthReportRow> _philHealthRows = [];
        private List<PagIbigReportRow>    _pagIbigRows    = [];
        private List<BirTaxReportRow>     _birRows        = [];

        private int _activeAgencyTab = 1; // 1=SSS, 2=PhilHealth, 3=PagIBIG, 4=BIR

        public GovernmentReportsView()
        {
            InitializeComponent();
            Loaded += (_, _) => InitView();
        }

        private void InitView()
        {
            LoadPeriods();
            LoadActiveReport();
        }

        private void LoadPeriods()
        {
            try { _periods = _periodRepo.GetAll(); }
            catch { _periods = []; }

            CboPeriodFilter.SelectionChanged -= PeriodFilter_Changed;
            CboPeriodFilter.Items.Clear();

            CboPeriodFilter.Items.Add(new ComboBoxItem { Content = "All Periods", Tag = 0 });
            foreach (var p in _periods)
            {
                CboPeriodFilter.Items.Add(new ComboBoxItem
                {
                    Content = $"{p.PeriodCode} ({p.StartDate:MMM dd}–{p.EndDate:MMM dd})",
                    Tag = p.Id
                });
            }
            CboPeriodFilter.SelectedIndex = CboPeriodFilter.Items.Count > 1 ? 1 : 0;
            CboPeriodFilter.SelectionChanged += PeriodFilter_Changed;
        }

        private int SelectedPeriodId
        {
            get
            {
                if (CboPeriodFilter.SelectedItem is ComboBoxItem item && item.Tag is int id)
                    return id;
                return 0;
            }
        }

        private void AgencyTab_Changed(object sender, RoutedEventArgs e)
        {
            if (SssGrid == null) return;

            if (TabSss.IsChecked == true)        _activeAgencyTab = 1;
            else if (TabPhilHealth.IsChecked == true) _activeAgencyTab = 2;
            else if (TabPagIbig.IsChecked == true)    _activeAgencyTab = 3;
            else if (TabBir.IsChecked == true)       _activeAgencyTab = 4;

            SssGrid.Visibility        = _activeAgencyTab == 1 ? Visibility.Visible : Visibility.Collapsed;
            PhilHealthGrid.Visibility = _activeAgencyTab == 2 ? Visibility.Visible : Visibility.Collapsed;
            PagIbigGrid.Visibility    = _activeAgencyTab == 3 ? Visibility.Visible : Visibility.Collapsed;
            BirTaxGrid.Visibility     = _activeAgencyTab == 4 ? Visibility.Visible : Visibility.Collapsed;

            LoadActiveReport();
        }

        private void PeriodFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            LoadActiveReport();
        }

        private void LoadActiveReport()
        {
            int pId = SelectedPeriodId;

            switch (_activeAgencyTab)
            {
                case 1: // SSS
                    try { _sssRows = _reportRepo.GetSssReport(pId > 0 ? pId : null); }
                    catch { _sssRows = GenerateSampleSss(); }
                    SssGrid.ItemsSource = _sssRows;
                    UpdateSssSummary();
                    break;

                case 2: // PhilHealth
                    try { _philHealthRows = _reportRepo.GetPhilHealthReport(pId > 0 ? pId : null); }
                    catch { _philHealthRows = GenerateSamplePhilHealth(); }
                    PhilHealthGrid.ItemsSource = _philHealthRows;
                    UpdatePhilHealthSummary();
                    break;

                case 3: // Pag-IBIG
                    try { _pagIbigRows = _reportRepo.GetPagIbigReport(pId > 0 ? pId : null); }
                    catch { _pagIbigRows = GenerateSamplePagIbig(); }
                    PagIbigGrid.ItemsSource = _pagIbigRows;
                    UpdatePagIbigSummary();
                    break;

                case 4: // BIR 1601-C
                    try { _birRows = _reportRepo.GetBirTaxReport(pId > 0 ? pId : null); }
                    catch { _birRows = GenerateSampleBir(); }
                    BirTaxGrid.ItemsSource = _birRows;
                    UpdateBirSummary();
                    break;
            }
        }

        private void UpdateSssSummary()
        {
            decimal ee    = _sssRows.Sum(r => r.EmployeeShare);
            decimal er    = _sssRows.Sum(r => r.EmployerShare);
            decimal ec    = _sssRows.Sum(r => r.EcContribution);
            decimal total = ee + er + ec;

            TxtSummaryEe.Text        = $"Employee Share: ₱{ee:N2}";
            TxtSummaryEr.Text        = $"Employer + EC: ₱{(er + ec):N2}";
            TxtSummaryTotal.Text     = $"Total SSS Remittance: ₱{total:N2}";
            TxtReportRecordCount.Text = $"{_sssRows.Count} employee(s)";
        }

        private void UpdatePhilHealthSummary()
        {
            decimal ee    = _philHealthRows.Sum(r => r.EmployeeShare);
            decimal er    = _philHealthRows.Sum(r => r.EmployerShare);
            decimal total = ee + er;

            TxtSummaryEe.Text        = $"Employee Share: ₱{ee:N2}";
            TxtSummaryEr.Text        = $"Employer Share: ₱{er:N2}";
            TxtSummaryTotal.Text     = $"Total PhilHealth Premium: ₱{total:N2}";
            TxtReportRecordCount.Text = $"{_philHealthRows.Count} employee(s)";
        }

        private void UpdatePagIbigSummary()
        {
            decimal ee    = _pagIbigRows.Sum(r => r.EmployeeShare);
            decimal er    = _pagIbigRows.Sum(r => r.EmployerShare);
            decimal total = ee + er;

            TxtSummaryEe.Text        = $"Employee Share: ₱{ee:N2}";
            TxtSummaryEr.Text        = $"Employer Share: ₱{er:N2}";
            TxtSummaryTotal.Text     = $"Total HDMF Contribution: ₱{total:N2}";
            TxtReportRecordCount.Text = $"{_pagIbigRows.Count} employee(s)";
        }

        private void UpdateBirSummary()
        {
            decimal gross = _birRows.Sum(r => r.GrossPay);
            decimal stat  = _birRows.Sum(r => r.StatutoryDeductions);
            decimal tax   = _birRows.Sum(r => r.TaxWithheld);

            TxtSummaryEe.Text        = $"Total Gross: ₱{gross:N2}";
            TxtSummaryEr.Text        = $"Non-Taxable: ₱{stat:N2}";
            TxtSummaryTotal.Text     = $"Total Tax Withheld (1601-C): ₱{tax:N2}";
            TxtReportRecordCount.Text = $"{_birRows.Count} employee(s)";
        }

        // ── CSV Export ───────────────────────────────────────────────────────
        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            string agencyName = _activeAgencyTab switch
            {
                1 => "SSS_R3_Report",
                2 => "PhilHealth_RF1_Report",
                3 => "PagIBIG_MCRF_Report",
                4 => "BIR_1601C_Tax_Report",
                _ => "Gov_Report"
            };

            var dialog = new SaveFileDialog
            {
                Title    = $"Export {agencyName}",
                FileName = $"{agencyName}_{DateTime.Now:yyyyMMdd}.csv",
                Filter   = "CSV File (*.csv)|*.csv|All Files (*.*)|*.*"
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                var sb = new StringBuilder();
                switch (_activeAgencyTab)
                {
                    case 1:
                        sb.AppendLine("Employee Code,SSS Number,Employee Name,Gross Pay,Employee Share,Employer Share,EC Contribution,Total SSS Contribution");
                        foreach (var r in _sssRows)
                            sb.AppendLine($"\"{r.EmployeeCode}\",\"{r.SssNumber}\",\"{r.FullName}\",{r.GrossPay:F2},{r.EmployeeShare:F2},{r.EmployerShare:F2},{r.EcContribution:F2},{r.TotalContribution:F2}");
                        break;

                    case 2:
                        sb.AppendLine("Employee Code,PhilHealth PIN,Employee Name,Basic Pay,Employee Share,Employer Share,Total Contribution");
                        foreach (var r in _philHealthRows)
                            sb.AppendLine($"\"{r.EmployeeCode}\",\"{r.PhilHealthNumber}\",\"{r.FullName}\",{r.BasicPay:F2},{r.EmployeeShare:F2},{r.EmployerShare:F2},{r.TotalContribution:F2}");
                        break;

                    case 3:
                        sb.AppendLine("Employee Code,Pag-IBIG ID (MID),Employee Name,Employee Contribution,Employer Contribution,Total Contribution");
                        foreach (var r in _pagIbigRows)
                            sb.AppendLine($"\"{r.EmployeeCode}\",\"{r.PagIbigNumber}\",\"{r.FullName}\",{r.EmployeeShare:F2},{r.EmployerShare:F2},{r.TotalContribution:F2}");
                        break;

                    case 4:
                        sb.AppendLine("Employee Code,TIN,Employee Name,Gross Compensation,Statutory Deductions,Taxable Income,Tax Withheld");
                        foreach (var r in _birRows)
                            sb.AppendLine($"\"{r.EmployeeCode}\",\"{r.TinNumber}\",\"{r.FullName}\",{r.GrossPay:F2},{r.StatutoryDeductions:F2},{r.TaxableIncome:F2},{r.TaxWithheld:F2}");
                        break;
                }

                File.WriteAllText(dialog.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show($"✅ Report exported successfully to:\n{dialog.FileName}",
                    "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Demo Offline Data ────────────────────────────────────────────────
        private static List<SssReportRow> GenerateSampleSss() =>
        [
            new SssReportRow { EmployeeCode = "EMP-2026-001", SssNumber = "34-1234567-8", FullName = "Dela Cruz, Juan", BasicPay = 13900m, GrossPay = 13900m, EmployeeShare = 625.50m, EmployerShare = 1320.50m },
            new SssReportRow { EmployeeCode = "EMP-2026-002", SssNumber = "34-8765432-1", FullName = "Santos, Maria",    BasicPay = 12500m, GrossPay = 12500m, EmployeeShare = 562.50m, EmployerShare = 1187.50m },
            new SssReportRow { EmployeeCode = "EMP-2026-003", SssNumber = "34-5556667-2", FullName = "Reyes, Pedro",     BasicPay = 14750m, GrossPay = 14750m, EmployeeShare = 663.75m, EmployerShare = 1401.25m },
        ];

        private static List<PhilHealthReportRow> GenerateSamplePhilHealth() =>
        [
            new PhilHealthReportRow { EmployeeCode = "EMP-2026-001", PhilHealthNumber = "12-345678901-2", FullName = "Dela Cruz, Juan", BasicPay = 13900m, EmployeeShare = 347.50m, EmployerShare = 347.50m },
            new PhilHealthReportRow { EmployeeCode = "EMP-2026-002", PhilHealthNumber = "12-987654321-0", FullName = "Santos, Maria",    BasicPay = 12500m, EmployeeShare = 312.50m, EmployerShare = 312.50m },
            new PhilHealthReportRow { EmployeeCode = "EMP-2026-003", PhilHealthNumber = "12-444555666-8", FullName = "Reyes, Pedro",     BasicPay = 14750m, EmployeeShare = 368.75m, EmployerShare = 368.75m },
        ];

        private static List<PagIbigReportRow> GenerateSamplePagIbig() =>
        [
            new PagIbigReportRow { EmployeeCode = "EMP-2026-001", PagIbigNumber = "1210-4567-8901", FullName = "Dela Cruz, Juan", EmployeeShare = 200m, EmployerShare = 200m },
            new PagIbigReportRow { EmployeeCode = "EMP-2026-002", PagIbigNumber = "1210-9876-5432", FullName = "Santos, Maria",    EmployeeShare = 200m, EmployerShare = 200m },
            new PagIbigReportRow { EmployeeCode = "EMP-2026-003", PagIbigNumber = "1210-3333-4444", FullName = "Reyes, Pedro",     EmployeeShare = 200m, EmployerShare = 200m },
        ];

        private static List<BirTaxReportRow> GenerateSampleBir() =>
        [
            new BirTaxReportRow { EmployeeCode = "EMP-2026-001", TinNumber = "123-456-789-000", FullName = "Dela Cruz, Juan", GrossPay = 13900m, StatutoryDeductions = 1173m, TaxableIncome = 12727m, TaxWithheld = 346.50m },
            new BirTaxReportRow { EmployeeCode = "EMP-2026-002", TinNumber = "987-654-321-000", FullName = "Santos, Maria",    GrossPay = 12500m, StatutoryDeductions = 1075m, TaxableIncome = 11425m, TaxWithheld = 151.20m },
            new BirTaxReportRow { EmployeeCode = "EMP-2026-003", TinNumber = "456-789-123-000", FullName = "Reyes, Pedro",     GrossPay = 14750m, StatutoryDeductions = 1232.50m, TaxableIncome = 13517.50m, TaxWithheld = 465.10m },
        ];
    }
}
