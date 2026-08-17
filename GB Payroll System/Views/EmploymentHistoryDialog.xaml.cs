using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GB_Payroll_System.Data;
using GB_Payroll_System.Models;
using GB_Payroll_System.Services;

namespace GB_Payroll_System.Views
{
    public partial class EmploymentHistoryDialog : Window
    {
        private readonly Employee _employee;
        private readonly EmploymentHistoryRepository _repo = new();
        private readonly EmployeeRepository _empRepo = new();

        public EmploymentHistoryDialog(Employee employee)
        {
            InitializeComponent();
            _employee = employee;

            PopulateEmployeeHeader();
            ResetForm();
            LoadHistory();
        }

        private void PopulateEmployeeHeader()
        {
            TxtDialogTitle.Text = $"Employment History & Re-employment — {_employee.FullName}";
            TxtEmployeeSubtitle.Text = $"{_employee.EmployeeCode} | {_employee.Department} | {_employee.Position}";

            TxtSummaryPosition.Text = string.IsNullOrWhiteSpace(_employee.Position) ? "—" : _employee.Position;
            TxtSummaryDept.Text = string.IsNullOrWhiteSpace(_employee.Department) ? "—" : _employee.Department;
            TxtSummaryDateHired.Text = _employee.DateHired.ToString("MMM dd, yyyy");
            TxtSummaryRate.Text = $"{_employee.PayType}: ₱{_employee.BasicRate:N2}";

            if (_employee.IsActive)
            {
                BadgeCurrentStatus.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C6F6D5"));
                TxtCurrentStatusBadge.Text = "ACTIVE";
                TxtCurrentStatusBadge.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#276749"));
            }
            else
            {
                BadgeCurrentStatus.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FED7D7"));
                TxtCurrentStatusBadge.Text = "INACTIVE / SEPARATED";
                TxtCurrentStatusBadge.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9B2C2C"));
            }
        }

        private void LoadHistory()
        {
            try
            {
                var history = _repo.GetByEmployee(_employee.Id);
                HistoryGrid.ItemsSource = history;
                TxtHistoryCount.Text = $"{history.Count} record(s)";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading employment history: {ex.Message}");
                HistoryGrid.ItemsSource = null;
                TxtHistoryCount.Text = "0 record(s)";
            }
        }

        private void ResetForm()
        {
            TxtCompanyName.Text = "Genetian";
            TxtDepartment.Text = _employee.Department;
            TxtPosition.Text = _employee.Position;
            DpStartDate.SelectedDate = _employee.DateHired;
            DpEndDate.SelectedDate = null;
            ChkCurrentlyOngoing.IsChecked = false;
            DpEndDate.IsEnabled = true;
            CboEmploymentType.SelectedIndex = 0;
            CboSeparationType.SelectedIndex = 0; // Retired
            ChkRehireEligible.IsChecked = true;
            ChkSyncEmployeeProfile.IsChecked = false;
            TxtReason.Text = string.Empty;
        }

        private void ChkCurrentlyOngoing_Checked(object sender, RoutedEventArgs e)
        {
            DpEndDate.SelectedDate = null;
            DpEndDate.IsEnabled = false;
        }

        private void ChkCurrentlyOngoing_Unchecked(object sender, RoutedEventArgs e)
        {
            DpEndDate.IsEnabled = true;
        }

        private void BtnPresetRetired_Click(object sender, RoutedEventArgs e)
        {
            TxtCompanyName.Text = "Genetian";
            TxtDepartment.Text = _employee.Department;
            TxtPosition.Text = _employee.Position;
            DpStartDate.SelectedDate = _employee.DateHired;
            DpEndDate.SelectedDate = DateTime.Today;
            ChkCurrentlyOngoing.IsChecked = false;
            CboEmploymentType.SelectedIndex = 0; // Regular
            CboSeparationType.SelectedIndex = 0; // Retired
            ChkRehireEligible.IsChecked = true;
            ChkSyncEmployeeProfile.IsChecked = false;
            TxtReason.Text = "Separated under retirement program / reached retirement age. Eligible for rehire / post-retirement engagement.";
        }

        private void BtnPresetRehire_Click(object sender, RoutedEventArgs e)
        {
            TxtCompanyName.Text = "Genetian (Rehired)";
            TxtDepartment.Text = _employee.Department;
            TxtPosition.Text = string.IsNullOrWhiteSpace(_employee.Position) ? "Consultant" : _employee.Position;
            DpStartDate.SelectedDate = DateTime.Today;
            DpEndDate.SelectedDate = null;
            ChkCurrentlyOngoing.IsChecked = true;
            CboEmploymentType.SelectedIndex = 5; // Consultant / Post-Retirement
            CboSeparationType.SelectedIndex = 1; // Rehired / Re-employed
            ChkRehireEligible.IsChecked = true;
            ChkSyncEmployeeProfile.IsChecked = true;
            TxtReason.Text = "Re-employed / Rehired after retirement as technical advisor / project consultant.";
        }

        private void BtnPresetAwol_Click(object sender, RoutedEventArgs e)
        {
            TxtCompanyName.Text = "Genetian";
            TxtDepartment.Text = _employee.Department;
            TxtPosition.Text = _employee.Position;
            DpStartDate.SelectedDate = _employee.DateHired;
            DpEndDate.SelectedDate = DateTime.Today;
            ChkCurrentlyOngoing.IsChecked = false;
            CboEmploymentType.SelectedIndex = 0; // Regular
            CboSeparationType.SelectedIndex = 2; // AWOL / Abandonment of Work
            ChkRehireEligible.IsChecked = false; // Flagged / NOT recommended for rehire
            ChkSyncEmployeeProfile.IsChecked = true; // Auto-deactivate employee status
            TxtReason.Text = "Employee stopped reporting to work without notice or approved leave (AWOL). Deemed abandonment of work. Return-to-Work notice issued.";
        }

        private void BtnPresetExternal_Click(object sender, RoutedEventArgs e)
        {
            TxtCompanyName.Text = "";
            TxtDepartment.Text = "";
            TxtPosition.Text = "";
            DpStartDate.SelectedDate = DateTime.Today.AddYears(-5);
            DpEndDate.SelectedDate = DateTime.Today.AddYears(-2);
            ChkCurrentlyOngoing.IsChecked = false;
            CboEmploymentType.SelectedIndex = 0;
            CboSeparationType.SelectedIndex = 3; // Resigned
            ChkRehireEligible.IsChecked = true;
            ChkSyncEmployeeProfile.IsChecked = false;
            TxtReason.Text = "Previous experience prior to joining Genetian.";
        }

        private void BtnAddRecord_Click(object sender, RoutedEventArgs e)
        {
            BannerError.Visibility = Visibility.Collapsed;

            string company = TxtCompanyName.Text.Trim();
            string pos = TxtPosition.Text.Trim();

            if (string.IsNullOrWhiteSpace(company))
            {
                ShowError("Please enter the Company / Employer Name.");
                return;
            }
            if (string.IsNullOrWhiteSpace(pos))
            {
                ShowError("Please enter the Position / Job Title.");
                return;
            }
            if (DpStartDate.SelectedDate == null)
            {
                ShowError("Please select a Start Date.");
                return;
            }

            DateTime startDate = DpStartDate.SelectedDate.Value;
            DateTime? endDate = (ChkCurrentlyOngoing.IsChecked == true) ? null : DpEndDate.SelectedDate;

            if (endDate.HasValue && endDate.Value < startDate)
            {
                ShowError("End Date cannot be earlier than Start Date.");
                return;
            }

            string empType = (CboEmploymentType.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Regular";
            string sepType = (CboSeparationType.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Active";

            try
            {
                var record = new EmploymentHistory
                {
                    EmployeeId = _employee.Id,
                    CompanyName = company,
                    Department = TxtDepartment.Text.Trim(),
                    Position = pos,
                    StartDate = startDate,
                    EndDate = endDate,
                    EmploymentType = empType,
                    SeparationType = sepType,
                    SeparationReason = TxtReason.Text.Trim(),
                    IsRehireEligible = ChkRehireEligible.IsChecked ?? true,
                    RecordedByUsername = AuthService.CurrentUser?.Username ?? "admin"
                };

                _repo.Insert(record);

                // Check if user requested to synchronize with employee profile (e.g. rehire reactivation)
                if (ChkSyncEmployeeProfile.IsChecked == true)
                {
                    _employee.Position = pos;
                    if (!string.IsNullOrWhiteSpace(TxtDepartment.Text.Trim()))
                        _employee.Department = TxtDepartment.Text.Trim();
                    
                    // If re-hiring, update date hired and reactivate
                    if (sepType.Contains("Rehired", StringComparison.OrdinalIgnoreCase) || sepType.Contains("Active", StringComparison.OrdinalIgnoreCase))
                    {
                        _employee.DateHired = startDate;
                        _employee.IsActive = true;
                    }
                    else if (sepType.Contains("Retired", StringComparison.OrdinalIgnoreCase) || 
                             sepType.Contains("Resigned", StringComparison.OrdinalIgnoreCase) || 
                             sepType.Contains("Terminated", StringComparison.OrdinalIgnoreCase) ||
                             sepType.Contains("AWOL", StringComparison.OrdinalIgnoreCase))
                    {
                        // Separated / AWOL / Terminated
                        _employee.IsActive = false;
                    }

                    _empRepo.Update(_employee);
                    PopulateEmployeeHeader();
                }

                LoadHistory();
                ResetForm();
                MessageBox.Show("Employment history record saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowError($"Failed to save employment record: {ex.Message}");
            }
        }

        private void BtnDeleteHistory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var confirm = MessageBox.Show(
                    "Are you sure you want to delete this employment history record?",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (confirm != MessageBoxResult.Yes) return;

                try
                {
                    _repo.Delete(id);
                    LoadHistory();
                }
                catch (Exception ex)
                {
                    ShowError($"Failed to delete record: {ex.Message}");
                }
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void ShowError(string msg)
        {
            TxtError.Text = msg;
            BannerError.Visibility = Visibility.Visible;
        }
    }
}
