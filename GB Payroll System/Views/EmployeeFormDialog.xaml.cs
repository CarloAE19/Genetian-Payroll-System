using System;
using System.Windows;
using System.Windows.Controls;
using GB_Payroll_System.Data;
using GB_Payroll_System.Models;
using GB_Payroll_System.Services;

namespace GB_Payroll_System.Views
{
    public partial class EmployeeFormDialog : Window
    {
        private readonly EmployeeRepository _repo = new();
        private Employee? _existing;
        private bool _isEditMode;

        public EmployeeFormDialog(Employee? employee)
        {
            InitializeComponent();
            _existing = employee;
            _isEditMode = employee != null;

            if (_isEditMode)
            {
                TxtDialogTitle.Text = "Edit Employee 201 File";
                TxtEmployeeCodeHeader.Text = employee!.EmployeeCode;
                BtnToggleActive.Visibility = Visibility.Visible;
                BtnToggleActive.Content = employee.IsActive ? "⛔ Deactivate Employee" : "✅ Reactivate Employee";
                BtnSave.Content = "💾  Save Changes";
                PopulateFields(employee);
            }
            else
            {
                // Set auto-generated employee code
                try { TxtEmployeeCode.Text = _repo.GenerateNextEmployeeCode(); }
                catch { TxtEmployeeCode.Text = $"EMP-{DateTime.Now.Year}-001"; }

                DpDateHired.SelectedDate = DateTime.Today;
                CboPayType.SelectedIndex = 0;
            }
        }

        private void PopulateFields(Employee emp)
        {
            TxtLastName.Text = emp.LastName;
            TxtFirstName.Text = emp.FirstName;
            TxtMiddleName.Text = emp.MiddleName;
            TxtEmployeeCode.Text = emp.EmployeeCode;
            TxtBiometricId.Text = emp.BiometricUserId;
            DpDateHired.SelectedDate = emp.DateHired;

            TxtDepartment.Text = emp.Department;
            TxtPosition.Text = emp.Position;
            CboPayType.SelectedIndex = (int)emp.PayType - 1;
            TxtBasicRate.Text = emp.BasicRate.ToString("N2");

            TxtSss.Text = emp.SssNumber;
            TxtPhilHealth.Text = emp.PhilHealthNumber;
            TxtPagIbig.Text = emp.PagIbigNumber;
            TxtTin.Text = emp.TinNumber;
            TxtBankAccount.Text = emp.BankAccountNumber;
        }

        private void CboPayType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TxtBasicRateLabel == null) return;
            TxtBasicRateLabel.Text = CboPayType.SelectedIndex switch
            {
                1 => "DAILY RATE (₱)",
                2 => "BASE PAKYAW RATE (₱)",
                3 => "BASE DAILY + PAKYAW RATE (₱)",
                _ => "MONTHLY BASIC SALARY (₱)"
            };
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            BannerError.Visibility = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(TxtLastName.Text) || string.IsNullOrWhiteSpace(TxtFirstName.Text))
            {
                ShowError("Last Name and First Name are required.");
                return;
            }
            if (string.IsNullOrWhiteSpace(TxtEmployeeCode.Text))
            {
                ShowError("Employee Code is required.");
                return;
            }
            if (!decimal.TryParse(TxtBasicRate.Text.Replace(",", ""), out decimal rate) || rate < 0)
            {
                ShowError("Please enter a valid Basic Rate.");
                return;
            }

            try
            {
                var emp = _existing ?? new Employee();
                emp.LastName = TxtLastName.Text.Trim();
                emp.FirstName = TxtFirstName.Text.Trim();
                emp.MiddleName = TxtMiddleName.Text.Trim();
                emp.EmployeeCode = TxtEmployeeCode.Text.Trim();
                emp.BiometricUserId = TxtBiometricId.Text.Trim();
                emp.DateHired = DpDateHired.SelectedDate ?? DateTime.Today;
                emp.Department = TxtDepartment.Text.Trim();
                emp.Position = TxtPosition.Text.Trim();
                emp.PayType = (PayType)(CboPayType.SelectedIndex + 1);
                emp.BasicRate = rate;
                emp.SssNumber = TxtSss.Text.Trim();
                emp.PhilHealthNumber = TxtPhilHealth.Text.Trim();
                emp.PagIbigNumber = TxtPagIbig.Text.Trim();
                emp.TinNumber = TxtTin.Text.Trim();
                emp.BankAccountNumber = TxtBankAccount.Text.Trim();
                emp.IsActive = true;

                if (_isEditMode)
                    _repo.Update(emp);
                else
                    _repo.Insert(emp);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowError($"Save failed: {ex.Message}");
            }
        }

        private void BtnToggleActive_Click(object sender, RoutedEventArgs e)
        {
            if (_existing == null) return;
            bool newStatus = !_existing.IsActive;
            string action = newStatus ? "reactivate" : "deactivate";

            var confirm = MessageBox.Show(
                $"Are you sure you want to {action} {_existing.FullName}?",
                "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                _repo.SetActive(_existing.Id, newStatus);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowError($"Operation failed: {ex.Message}");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();

        private void ShowError(string msg)
        {
            TxtError.Text = msg;
            BannerError.Visibility = Visibility.Visible;
        }

        private void TxtMiddleName_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
