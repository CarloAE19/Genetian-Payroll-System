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
    public partial class SalaryPromotionDialog : Window
    {
        private Employee? _employee;
        private readonly SalaryPromotionRepository _repo = new();
        private readonly EmployeeRepository _empRepo = new();
        private List<Employee> _allEmployees = [];

        public SalaryPromotionDialog(Employee? employee = null)
        {
            InitializeComponent();
            _employee = employee;

            // Attach Currency Auto-formatting
            CurrencyInputHelper.Attach(TxtNewRate);

            if (_employee != null)
            {
                SetSelectedEmployee(_employee);
            }
            else
            {
                TxtDialogTitle.Text = "Record Salary Promotion & Adjustment";
                TxtEmployeeSubtitle.Text = "Select an employee to apply a salary increase or position change";
                PanelEmployeePicker.Visibility = Visibility.Visible;
                LoadEmployeesDropdown();
            }
        }

        private void LoadEmployeesDropdown()
        {
            try
            {
                _allEmployees = _empRepo.GetAll(activeOnly: true);
                CboSelectEmployee.Items.Clear();
                foreach (var emp in _allEmployees)
                {
                    CboSelectEmployee.Items.Add(new ComboBoxItem
                    {
                        Content = $"{emp.FullName} ({emp.EmployeeCode}) — {emp.Position} [{emp.Department}]",
                        Tag = emp
                    });
                }
                if (CboSelectEmployee.Items.Count > 0)
                {
                    CboSelectEmployee.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load employee list: {ex.Message}");
            }
        }

        private void CboSelectEmployee_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CboSelectEmployee.SelectedItem is ComboBoxItem item && item.Tag is Employee emp)
            {
                SetSelectedEmployee(emp);
            }
        }

        private void SetSelectedEmployee(Employee employee)
        {
            _employee = employee;
            TxtDialogTitle.Text = $"Salary Promotion — {employee.FullName}";
            TxtEmployeeSubtitle.Text = $"{employee.EmployeeCode} | {employee.Department}";
            TxtCurrentPosition.Text = string.IsNullOrWhiteSpace(employee.Position) ? "—" : employee.Position;
            TxtCurrentRate.Text = $"₱{employee.BasicRate:N2}";
            TxtNewRateLabel.Text = employee.PayType == PayType.Daily
                ? "NEW DAILY RATE (₱) *"
                : "NEW MONTHLY BASIC SALARY (₱) *";

            DpEffectiveDate.SelectedDate = DateTime.Today;
            TxtNewPosition.Text = employee.Position;
            TxtNewRate.Text = employee.BasicRate.ToString("N2");

            LoadHistory();
        }

        private void LoadHistory()
        {
            if (_employee == null) return;
            try
            {
                var history = _repo.GetByEmployee(_employee.Id);
                HistoryGrid.ItemsSource = history;
            }
            catch
            {
                HistoryGrid.ItemsSource = null;
            }
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            BannerError.Visibility = Visibility.Collapsed;

            if (_employee == null)
            {
                ShowError("Please select an employee first.");
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtNewPosition.Text))
            {
                ShowError("Please enter the new position/job title.");
                return;
            }
            if (!decimal.TryParse(TxtNewRate.Text.Replace(",", ""), out decimal newRate) || newRate <= 0)
            {
                ShowError("Please enter a valid new rate amount.");
                return;
            }
            if (DpEffectiveDate.SelectedDate == null)
            {
                ShowError("Please select an effective date.");
                return;
            }

            var confirm = MessageBox.Show(
                $"Apply salary promotion for {_employee.FullName}?\n\n" +
                $"Position: {_employee.Position} ➔ {TxtNewPosition.Text.Trim()}\n" +
                $"Rate: ₱{_employee.BasicRate:N2} ➔ ₱{newRate:N2}\n" +
                $"Effective: {DpEffectiveDate.SelectedDate:MMMM dd, yyyy}",
                "Confirm Promotion", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                var record = new SalaryPromotionHistory
                {
                    EmployeeId = _employee.Id,
                    PreviousPosition = _employee.Position,
                    NewPosition = TxtNewPosition.Text.Trim(),
                    PreviousRate = _employee.BasicRate,
                    NewRate = newRate,
                    EffectiveDate = DpEffectiveDate.SelectedDate!.Value,
                    Reason = TxtReason.Text.Trim(),
                    ApprovedByUsername = AuthService.CurrentUser?.Username ?? "admin"
                };

                _repo.Insert(record);

                // Apply new rate and position to employee record
                _employee.Position = record.NewPosition;
                _employee.BasicRate = newRate;
                _empRepo.Update(_employee);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowError($"Failed to save promotion: {ex.Message}");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();

        private void ShowError(string msg)
        {
            TxtError.Text = msg;
            BannerError.Visibility = Visibility.Visible;
        }
    }
}
