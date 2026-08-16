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
    public partial class LeaveApplicationDialog : Window
    {
        private readonly LeaveRepository    _leaveRepo = new();
        private readonly EmployeeRepository _empRepo   = new();

        private List<Employee> _employees = [];
        private Employee?      _selectedEmployee;

        public LeaveApplicationDialog()
        {
            InitializeComponent();
            DpStart.SelectedDate = DateTime.Today;
            DpEnd.SelectedDate   = DateTime.Today;

            try { _employees = _empRepo.GetAll(activeOnly: true); }
            catch { _employees = []; }
        }

        private void TxtEmpCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            string code = TxtEmpCode.Text.Trim().ToUpper();
            _selectedEmployee = _employees.FirstOrDefault(emp =>
                emp.EmployeeCode.Equals(code, StringComparison.OrdinalIgnoreCase));

            TxtEmpName.Text = _selectedEmployee != null
                ? $"✅  {_selectedEmployee.FullName} ({_selectedEmployee.Department})"
                : (string.IsNullOrEmpty(code) ? "" : "❌  Employee not found");
        }

        private void Date_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (ChkHalfDay?.IsChecked == true)
            {
                TxtDaysCount.Text = "0.5";
                return;
            }

            if (DpStart.SelectedDate.HasValue && DpEnd.SelectedDate.HasValue)
            {
                var start = DpStart.SelectedDate.Value;
                var end   = DpEnd.SelectedDate.Value;

                if (end >= start)
                {
                    // Count business working days (excl. Sat/Sun)
                    int days = 0;
                    for (var d = start; d <= end; d = d.AddDays(1))
                    {
                        if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
                            days++;
                    }
                    TxtDaysCount.Text = Math.Max(1, days).ToString("F1");
                }
            }
        }

        private void ChkHalfDay_Checked(object sender, RoutedEventArgs e) => TxtDaysCount.Text = "0.5";
        private void ChkHalfDay_Unchecked(object sender, RoutedEventArgs e) => Date_Changed(sender, null!);

        private void CboLeaveType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Reset error if any
            BannerError.Visibility = Visibility.Collapsed;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            BannerError.Visibility = Visibility.Collapsed;

            if (_selectedEmployee == null) { ShowError("Please enter a valid Employee Code."); return; }
            if (DpStart.SelectedDate == null || DpEnd.SelectedDate == null) { ShowError("Please select start and end dates."); return; }
            if (DpEnd.SelectedDate < DpStart.SelectedDate) { ShowError("End Date cannot be before Start Date."); return; }

            if (!decimal.TryParse(TxtDaysCount.Text, out decimal days) || days <= 0)
            { ShowError("Please enter a valid Days Count (e.g. 1.0 or 0.5)."); return; }

            if (string.IsNullOrWhiteSpace(TxtReason.Text.Trim()))
            { ShowError("Please state the reason for this leave request."); return; }

            LeaveType type = CboLeaveType.SelectedIndex switch
            {
                0 => LeaveType.VacationLeave,
                1 => LeaveType.SickLeave,
                2 => LeaveType.EmergencyLeave,
                3 => LeaveType.MaternityLeave,
                4 => LeaveType.PaternityLeave,
                5 => LeaveType.SoloParentLeave,
                6 => LeaveType.Bereavement,
                _ => LeaveType.VacationLeave
            };

            try
            {
                var app = new LeaveApplication
                {
                    EmployeeId = _selectedEmployee.Id,
                    Type       = type,
                    StartDate  = DpStart.SelectedDate.Value,
                    EndDate    = DpEnd.SelectedDate.Value,
                    DaysCount  = days,
                    Reason     = TxtReason.Text.Trim(),
                    Status     = LeaveStatus.Pending,
                    CreatedAt  = DateTime.UtcNow
                };

                _leaveRepo.InsertApplication(app);
                DialogResult = true;
                Close();
            }
            catch (Exception ex) { ShowError($"Failed to submit application: {ex.Message}"); }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();

        private void ShowError(string msg) { TxtError.Text = msg; BannerError.Visibility = Visibility.Visible; }
    }
}
