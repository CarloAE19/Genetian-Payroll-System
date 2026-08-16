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
    public partial class PakyawOutputEntryDialog : Window
    {
        private readonly PakyawEntryRepository _entryRepo = new();
        private readonly EmployeeRepository    _empRepo   = new();

        private List<PakyawRate> _tasks;
        private List<Employee>   _employees = [];
        private PakyawRate?      _selectedTask;
        private Employee?        _selectedEmployee;

        public PakyawOutputEntryDialog(List<PakyawRate> activeTasks, int preselectedRateId = 0)
        {
            InitializeComponent();
            _tasks = activeTasks;

            CboTask.ItemsSource = _tasks;
            DpWorkDate.SelectedDate = DateTime.Today;

            // Load employees for lookup
            try { _employees = _empRepo.GetAll(); } catch { /* offline */ }

            // Preselect task if launched from catalog grid button
            if (preselectedRateId > 0)
            {
                var idx = _tasks.FindIndex(t => t.Id == preselectedRateId);
                if (idx >= 0) CboTask.SelectedIndex = idx;
            }
        }

        // ── Task selection ───────────────────────────────────────────────────
        private void CboTask_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedTask = CboTask.SelectedItem as PakyawRate;
            if (_selectedTask == null)
            {
                RateInfoCard.Visibility = Visibility.Collapsed;
                return;
            }
            TxtInfoCode.Text = _selectedTask.TaskCode;
            TxtInfoUnit.Text = _selectedTask.UnitOfMeasure;
            TxtInfoRate.Text = $"₱{_selectedTask.RatePerUnit:N2}";
            TxtQtyLabel.Text = $"QUANTITY COMPLETED ({_selectedTask.UnitOfMeasure}) *";
            RateInfoCard.Visibility = Visibility.Visible;
            UpdateEarningsPreview();
        }

        // ── Employee code lookup ─────────────────────────────────────────────
        private void TxtEmpCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            string code = TxtEmpCode.Text.Trim().ToUpper();
            _selectedEmployee = _employees.FirstOrDefault(emp =>
                emp.EmployeeCode.Equals(code, StringComparison.OrdinalIgnoreCase));

            TxtEmpName.Text = _selectedEmployee != null
                ? $"✅  {_selectedEmployee.FullName}  ({_selectedEmployee.Department})"
                : (string.IsNullOrEmpty(code) ? "" : "❌  Employee not found");
        }

        // ── Live earnings preview ────────────────────────────────────────────
        private void TxtQuantity_TextChanged(object sender, TextChangedEventArgs e) => UpdateEarningsPreview();

        private void UpdateEarningsPreview()
        {
            if (_selectedTask == null || !decimal.TryParse(TxtQuantity.Text, out decimal qty) || qty <= 0)
            {
                EarningsCard.Visibility = Visibility.Collapsed;
                return;
            }
            decimal earnings = qty * _selectedTask.RatePerUnit;
            TxtEarningsPreview.Text = $"₱{earnings:N2}";
            EarningsCard.Visibility = Visibility.Visible;
        }

        // ── Save ─────────────────────────────────────────────────────────────
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            BannerError.Visibility = Visibility.Collapsed;

            if (_selectedTask == null) { ShowError("Please select a task."); return; }
            if (_selectedEmployee == null) { ShowError("Please enter a valid Employee Code."); return; }
            if (DpWorkDate.SelectedDate == null) { ShowError("Please select the work date."); return; }
            if (!decimal.TryParse(TxtQuantity.Text, out decimal qty) || qty <= 0)
            { ShowError("Please enter a valid Quantity Completed (must be > 0)."); return; }

            try
            {
                var entry = new PakyawEntry
                {
                    EmployeeId          = _selectedEmployee.Id,
                    PakyawRateId        = _selectedTask.Id,
                    WorkDate            = DpWorkDate.SelectedDate.Value,
                    QuantityCompleted   = qty,
                    UnitRate            = _selectedTask.RatePerUnit, // snapshot current rate
                    Remarks             = TxtRemarks.Text.Trim(),
                    RecordedByUsername  = AuthService.CurrentUser?.Username ?? "admin",
                    CreatedAt           = DateTime.UtcNow
                };
                _entryRepo.Insert(entry);
                DialogResult = true;
                Close();
            }
            catch (Exception ex) { ShowError($"Save failed: {ex.Message}"); }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();

        private void ShowError(string msg) { TxtError.Text = msg; BannerError.Visibility = Visibility.Visible; }
    }
}
