using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GB_Payroll_System.Data;
using GB_Payroll_System.Models;
using GB_Payroll_System.Services;

namespace GB_Payroll_System.Views
{
    public partial class LeaveManagementView : UserControl
    {
        private readonly LeaveRepository _leaveRepo = new();

        private List<LeaveApplicationViewModel> _applications = [];
        private List<LeaveBalanceViewModel>     _balances     = [];
        private bool _showingApplications = true;

        public LeaveManagementView()
        {
            InitializeComponent();
            Loaded += (_, _) => InitView();
        }

        private void InitView()
        {
            PopulateYearFilter();
            LoadData();
        }

        private void PopulateYearFilter()
        {
            int currentYear = DateTime.Now.Year;
            CboYearFilter.SelectionChanged -= YearFilter_Changed;
            CboYearFilter.Items.Clear();
            for (int y = currentYear + 1; y >= currentYear - 2; y--)
                CboYearFilter.Items.Add(new ComboBoxItem { Content = y.ToString(), Tag = y });
            CboYearFilter.SelectedIndex = 1; // current year
            CboYearFilter.SelectionChanged += YearFilter_Changed;
        }

        private void LeaveTab_Changed(object sender, RoutedEventArgs e)
        {
            if (ApplicationsGrid == null || TabApplications == null) return;
            _showingApplications = TabApplications.IsChecked == true;

            ApplicationsGrid.Visibility  = _showingApplications ? Visibility.Visible   : Visibility.Collapsed;
            BalancesGrid.Visibility      = _showingApplications ? Visibility.Collapsed : Visibility.Visible;

            if (StatusFilterPanel != null) StatusFilterPanel.Visibility = _showingApplications ? Visibility.Visible   : Visibility.Collapsed;
            if (YearFilterPanel != null)   YearFilterPanel.Visibility   = _showingApplications ? Visibility.Collapsed : Visibility.Visible;
            if (BtnCarryOver != null)      BtnCarryOver.Visibility      = _showingApplications ? Visibility.Collapsed : Visibility.Visible;

            LoadData();
        }

        private void LoadData()
        {
            if (_showingApplications) LoadApplications();
            else LoadBalances();
        }

        private void LoadApplications()
        {
            if (ApplicationsGrid == null || CboStatusFilter == null || TxtPendingCount == null) return;

            LeaveStatus? status = CboStatusFilter.SelectedIndex switch
            {
                1 => LeaveStatus.Pending,
                2 => LeaveStatus.Approved,
                3 => LeaveStatus.Rejected,
                _ => null
            };

            try { _applications = _leaveRepo.GetApplications(status: status); }
            catch { _applications = GenerateSampleApplications(); }

            ApplicationsGrid.ItemsSource = _applications;

            TxtPendingCount.Text  = $"Pending: {_applications.Count(a => a.Status == LeaveStatus.Pending)}";
            TxtApprovedCount.Text = $"Approved: {_applications.Count(a => a.Status == LeaveStatus.Approved)}";
            TxtRejectedCount.Text = $"Rejected: {_applications.Count(a => a.Status == LeaveStatus.Rejected)}";
            TxtRecordCount.Text   = $"{_applications.Count} application(s)";
        }

        private void LoadBalances()
        {
            if (BalancesGrid == null || CboYearFilter == null || TxtRecordCount == null) return;

            int year = CboYearFilter.SelectedItem is ComboBoxItem item && item.Tag is int y ? y : DateTime.Now.Year;

            try { _balances = _leaveRepo.GetBalances(year); }
            catch { _balances = GenerateSampleBalances(year); }

            BalancesGrid.ItemsSource = _balances;
            TxtRecordCount.Text      = $"{_balances.Count} employee balance(s)";
        }

        private void StatusFilter_Changed(object sender, SelectionChangedEventArgs e) => LoadApplications();
        private void YearFilter_Changed(object sender, SelectionChangedEventArgs e) => LoadBalances();

        private void BtnFileLeave_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new LeaveApplicationDialog();
            if (dialog.ShowDialog() == true) LoadApplications();
        }

        private void BtnApprove_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var app = _applications.FirstOrDefault(a => a.Id == id);
                if (app == null) return;

                var confirm = MessageBox.Show(
                    $"Approve leave application for {app.FullName} ({app.TypeDisplay}) for {app.DaysCount:F1} day(s)?",
                    "Confirm Approval", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (confirm != MessageBoxResult.Yes) return;

                try
                {
                    _leaveRepo.UpdateApplicationStatus(id, LeaveStatus.Approved, AuthService.CurrentUser?.Username ?? "admin");
                    LoadApplications();
                    MessageBox.Show("Leave application approved and credits deducted.", "Approved", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void BtnReject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var app = _applications.FirstOrDefault(a => a.Id == id);
                if (app == null) return;

                var confirm = MessageBox.Show(
                    $"Reject leave application for {app.FullName}?",
                    "Confirm Rejection", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (confirm != MessageBoxResult.Yes) return;

                try
                {
                    _leaveRepo.UpdateApplicationStatus(id, LeaveStatus.Rejected, AuthService.CurrentUser?.Username ?? "admin");
                    LoadApplications();
                }
                catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void BtnCarryOver_Click(object sender, RoutedEventArgs e)
        {
            int currentYear = DateTime.Now.Year;
            int prevYear    = currentYear - 1;

            var confirm = MessageBox.Show(
                $"Run Year-End Carry-Over from {prevYear} to {currentYear}?\n\nThis will carry over up to 5 days of unused Vacation Leave (VL) per employee into the new year's balance.",
                "Confirm Carry-Over", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                int count = _leaveRepo.PerformYearEndCarryOver(prevYear, currentYear, maxCarryOver: 5.0m);
                LoadBalances();
                MessageBox.Show($"✅ Carry-over completed for {count} employee balance(s).",
                    "Carry-Over Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) { MessageBox.Show($"Carry-over failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        // ── Demo Offline Data ────────────────────────────────────────────────
        private static List<LeaveApplicationViewModel> GenerateSampleApplications() =>
        [
            new LeaveApplicationViewModel { Id = 1, EmployeeCode = "EMP-2026-001", FullName = "Dela Cruz, Juan", Department = "Construction", Type = LeaveType.VacationLeave,  StartDate = DateTime.Today.AddDays(2), EndDate = DateTime.Today.AddDays(4), DaysCount = 3.0m, Reason = "Family trip", Status = LeaveStatus.Pending, CreatedAt = DateTime.Today.AddDays(-1) },
            new LeaveApplicationViewModel { Id = 2, EmployeeCode = "EMP-2026-002", FullName = "Santos, Maria",    Department = "HR",           Type = LeaveType.SickLeave,      StartDate = DateTime.Today.AddDays(-2), EndDate = DateTime.Today.AddDays(-2), DaysCount = 1.0m, Reason = "Flu / Fever", Status = LeaveStatus.Approved, ApprovedByUsername = "admin", ApprovedAt = DateTime.Today.AddDays(-2), CreatedAt = DateTime.Today.AddDays(-3) },
            new LeaveApplicationViewModel { Id = 3, EmployeeCode = "EMP-2026-003", FullName = "Reyes, Pedro",     Department = "Accounting",   Type = LeaveType.EmergencyLeave, StartDate = DateTime.Today.AddDays(-5), EndDate = DateTime.Today.AddDays(-5), DaysCount = 1.0m, Reason = "Personal matter", Status = LeaveStatus.Rejected, ApprovedByUsername = "admin", ApprovedAt = DateTime.Today.AddDays(-5), CreatedAt = DateTime.Today.AddDays(-6) },
        ];

        private static List<LeaveBalanceViewModel> GenerateSampleBalances(int year) =>
        [
            new LeaveBalanceViewModel { Id = 1, EmployeeId = 1, EmployeeCode = "EMP-2026-001", FullName = "Dela Cruz, Juan", Department = "Construction", Year = year, VacationLeaveTotal = 15.0m, VacationLeaveUsed = 3.0m, CarryOverDays = 5.0m, SickLeaveTotal = 15.0m, SickLeaveUsed = 1.0m, EmergencyLeaveTotal = 5.0m, EmergencyLeaveUsed = 0.0m },
            new LeaveBalanceViewModel { Id = 2, EmployeeId = 2, EmployeeCode = "EMP-2026-002", FullName = "Santos, Maria",    Department = "HR",           Year = year, VacationLeaveTotal = 15.0m, VacationLeaveUsed = 5.0m, CarryOverDays = 2.0m, SickLeaveTotal = 15.0m, SickLeaveUsed = 2.0m, EmergencyLeaveTotal = 5.0m, EmergencyLeaveUsed = 1.0m },
            new LeaveBalanceViewModel { Id = 3, EmployeeId = 3, EmployeeCode = "EMP-2026-003", FullName = "Reyes, Pedro",     Department = "Accounting",   Year = year, VacationLeaveTotal = 15.0m, VacationLeaveUsed = 0.0m, CarryOverDays = 0.0m, SickLeaveTotal = 15.0m, SickLeaveUsed = 0.0m, EmergencyLeaveTotal = 5.0m, EmergencyLeaveUsed = 0.0m },
        ];
    }
}
