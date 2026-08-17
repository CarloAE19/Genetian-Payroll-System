using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using GB_Payroll_System.Data;
using GB_Payroll_System.Models;
using GB_Payroll_System.Services;
using Microsoft.Win32;

namespace GB_Payroll_System.Views
{
    public partial class EmployeeFormDialog : Window
    {
        private readonly EmployeeRepository _repo = new();
        private readonly DocumentRepository _docRepo = new();
        private readonly ContractRepository _contractRepo = new();

        private Employee? _existing;
        private bool _isEditMode;
        private List<EmployeeDocument> _documents = [];

        public EmployeeFormDialog(Employee? employee)
        {
            InitializeComponent();
            _existing = employee;
            _isEditMode = employee != null;

            // Attach Currency Auto-formatting (Commas + .00 on LostFocus)
            CurrencyInputHelper.Attach(TxtBasicRate);
            CurrencyInputHelper.Attach(TxtCustomSss);
            CurrencyInputHelper.Attach(TxtCustomPh);
            CurrencyInputHelper.Attach(TxtPagIbigAmount);

            if (_isEditMode)
            {
                TxtDialogTitle.Text = "Edit Employee 201 Profile & Contract";
                TxtEmployeeCodeHeader.Text = employee!.EmployeeCode;
                BtnToggleActive.Visibility = Visibility.Visible;
                BtnManageHistoryFooter.Visibility = Visibility.Visible;
                BtnToggleActive.Content = employee.IsActive ? "⛔ Deactivate Employee" : "✅ Reactivate Employee";
                BtnSave.Content = "💾  Save Changes";
                PopulateFields(employee);
                LoadDocuments(employee.Id);
            }
            else
            {
                try { TxtEmployeeCode.Text = _repo.GenerateNextEmployeeCode(); }
                catch { TxtEmployeeCode.Text = $"EMP-{DateTime.Now.Year}-001"; }

                DpDateHired.SelectedDate = DateTime.Today;
                CboPayType.SelectedIndex = 0;
            }
        }

        private void FormTab_Changed(object sender, RoutedEventArgs e)
        {
            if (PanelPersonal == null || PanelEmployment == null || PanelContributions == null || PanelVault == null) return;

            PanelPersonal.Visibility      = TabPersonal.IsChecked == true      ? Visibility.Visible : Visibility.Collapsed;
            PanelEmployment.Visibility    = TabEmployment.IsChecked == true    ? Visibility.Visible : Visibility.Collapsed;
            PanelContributions.Visibility = TabContributions.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            PanelVault.Visibility         = TabVault.IsChecked == true         ? Visibility.Visible : Visibility.Collapsed;
        }

        private void PopulateFields(Employee emp)
        {
            // Personal Details
            TxtLastName.Text = emp.LastName;
            TxtFirstName.Text = emp.FirstName;
            TxtMiddleName.Text = emp.MiddleName;
            TxtEmployeeCode.Text = emp.EmployeeCode;
            DpBirthDate.SelectedDate = emp.BirthDate;
            CboGender.SelectedIndex = emp.Gender == "Female" ? 1 : 0;
            CboCivilStatus.Text = string.IsNullOrWhiteSpace(emp.CivilStatus) ? "Single" : emp.CivilStatus;
            TxtContactNumber.Text = emp.ContactNumber;
            TxtEmailAddress.Text = emp.EmailAddress;
            TxtAddress.Text = emp.Address;
            TxtEmergencyPerson.Text = emp.EmergencyContactName;
            TxtEmergencyPhone.Text = emp.EmergencyContactPhone;

            // Employment & Contract
            TxtDepartment.Text = emp.Department;
            TxtPosition.Text = emp.Position;
            CboContractType.SelectedIndex = Math.Clamp((int)emp.ContractType - 1, 0, 4);
            CboContractStatus.SelectedIndex = Math.Clamp((int)emp.ContractStatus - 1, 0, 3);
            DpDateHired.SelectedDate = emp.DateHired;
            DpContractEnd.SelectedDate = emp.ContractEndDate;
            CboPayType.SelectedIndex = emp.PayType == PayType.Daily ? 1 : 0;
            TxtBasicRate.Text = emp.BasicRate.ToString("N2");
            TxtBiometricId.Text = emp.BiometricUserId;
            TxtBankAccount.Text = emp.BankAccountNumber;

            // Government IDs
            TxtSss.Text = emp.SssNumber;
            TxtPhilHealth.Text = emp.PhilHealthNumber;
            TxtPagIbig.Text = emp.PagIbigNumber;
            TxtTin.Text = emp.TinNumber;

            // Contribution Settings
            CboSssMode.SelectedIndex = Math.Clamp((int)emp.SssDeductionMode - 1, 0, 2);
            TxtCustomSss.Text = emp.CustomSssAmount > 0 ? emp.CustomSssAmount.ToString("N2") : "";

            CboPhMode.SelectedIndex = Math.Clamp((int)emp.PhilHealthDeductionMode - 1, 0, 2);
            TxtCustomPh.Text = emp.CustomPhilHealthAmount > 0 ? emp.CustomPhilHealthAmount.ToString("N2") : "";

            CboPagIbigMode.SelectedIndex = Math.Clamp((int)emp.PagIbigDeductionMode - 1, 0, 2);
            TxtPagIbigAmount.Text = emp.PagIbigEmployeeAmount > 0 ? emp.PagIbigEmployeeAmount.ToString("N2") : "200.00";

            CboCutoffSchedule.SelectedIndex = Math.Clamp((int)emp.ContributionSchedule - 1, 0, 2);
            ChkMinimumWage.IsChecked = emp.IsMinimumWageEarner;
            ChkTaxExempt.IsChecked = emp.IsTaxExempt;
        }

        private void CboPayType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TxtBasicRateLabel == null) return;
            TxtBasicRateLabel.Text = CboPayType.SelectedIndex == 1
                ? "DAILY RATE (₱)"
                : "MONTHLY BASIC SALARY (₱)";
        }

        private void LoadDocuments(int employeeId)
        {
            try
            {
                _documents = _docRepo.GetByEmployeeId(employeeId);
                DocumentsGrid.ItemsSource = _documents;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load documents: {ex.Message}");
            }
        }

        private void BtnUploadDocument_Click(object sender, RoutedEventArgs e)
        {
            if (_existing == null || _existing.Id <= 0)
            {
                MessageBox.Show("Please save the employee personal record first before uploading 201 documents.",
                    "Save Employee First", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var openDialog = new OpenFileDialog
            {
                Title = "Select 201 Document to Attach",
                Filter = "All Documents (*.pdf;*.jpg;*.jpeg;*.png;*.docx)|*.pdf;*.jpg;*.jpeg;*.png;*.docx|PDF Documents (*.pdf)|*.pdf|Images (*.jpg;*.png)|*.jpg;*.png|All Files (*.*)|*.*"
            };

            if (openDialog.ShowDialog() == true)
            {
                try
                {
                    var cat = (DocumentCategory)(CboDocCategory.SelectedIndex + 1);
                    string title = TxtDocTitle.Text.Trim();
                    _docRepo.SaveDocument(_existing.Id, cat, title, openDialog.FileName);
                    TxtDocTitle.Clear();
                    LoadDocuments(_existing.Id);
                    MessageBox.Show("Document attached successfully to 201 file vault.", "Uploaded", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to upload document: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnOpenDoc_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int docId)
            {
                var doc = _documents.Find(d => d.Id == docId);
                if (doc != null && File.Exists(doc.FilePath))
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(doc.FilePath) { UseShellExecute = true });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Could not open document: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Document file not found on disk.", "File Missing", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void BtnDeleteDoc_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int docId)
            {
                var confirm = MessageBox.Show("Delete this document from the employee's 201 vault?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirm == MessageBoxResult.Yes)
                {
                    _docRepo.DeleteDocument(docId);
                    if (_existing != null) LoadDocuments(_existing.Id);
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtLastName.Text) || string.IsNullOrWhiteSpace(TxtFirstName.Text))
            {
                MessageBox.Show("Last Name and First Name are required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                TabPersonal.IsChecked = true;
                return;
            }
            if (string.IsNullOrWhiteSpace(TxtEmployeeCode.Text))
            {
                MessageBox.Show("Employee Code is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                TabPersonal.IsChecked = true;
                return;
            }
            if (string.IsNullOrWhiteSpace(TxtPosition.Text))
            {
                MessageBox.Show("Position / Job Title is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                TabEmployment.IsChecked = true;
                return;
            }

            decimal.TryParse(TxtBasicRate.Text.Replace(",", ""), out decimal rate);
            decimal.TryParse(TxtCustomSss.Text.Replace(",", ""), out decimal customSss);
            decimal.TryParse(TxtCustomPh.Text.Replace(",", ""), out decimal customPh);
            decimal.TryParse(TxtPagIbigAmount.Text.Replace(",", ""), out decimal pagIbigAmount);
            if (pagIbigAmount <= 0) pagIbigAmount = 200m;

            try
            {
                var emp = _existing ?? new Employee();
                emp.LastName = TxtLastName.Text.Trim();
                emp.FirstName = TxtFirstName.Text.Trim();
                emp.MiddleName = TxtMiddleName.Text.Trim();
                emp.EmployeeCode = TxtEmployeeCode.Text.Trim();
                emp.BirthDate = DpBirthDate.SelectedDate;
                emp.Gender = (CboGender.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Male";
                emp.CivilStatus = (CboCivilStatus.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Single";
                emp.ContactNumber = TxtContactNumber.Text.Trim();
                emp.EmailAddress = TxtEmailAddress.Text.Trim();
                emp.Address = TxtAddress.Text.Trim();
                emp.EmergencyContactName = TxtEmergencyPerson.Text.Trim();
                emp.EmergencyContactPhone = TxtEmergencyPhone.Text.Trim();

                emp.Department = TxtDepartment.Text.Trim();
                emp.Position = TxtPosition.Text.Trim();
                emp.ContractType = (ContractType)(CboContractType.SelectedIndex + 1);
                emp.ContractStatus = (ContractStatus)(CboContractStatus.SelectedIndex + 1);
                emp.DateHired = DpDateHired.SelectedDate ?? DateTime.Today;
                emp.ContractEndDate = DpContractEnd.SelectedDate;
                emp.PayType = CboPayType.SelectedIndex == 1 ? PayType.Daily : PayType.Monthly;
                emp.BasicRate = rate;
                emp.BiometricUserId = TxtBiometricId.Text.Trim();
                emp.BankAccountNumber = TxtBankAccount.Text.Trim();

                emp.SssNumber = TxtSss.Text.Trim();
                emp.PhilHealthNumber = TxtPhilHealth.Text.Trim();
                emp.PagIbigNumber = TxtPagIbig.Text.Trim();
                emp.TinNumber = TxtTin.Text.Trim();

                emp.SssDeductionMode = (DeductionMode)(CboSssMode.SelectedIndex + 1);
                emp.CustomSssAmount = customSss;

                emp.PhilHealthDeductionMode = (DeductionMode)(CboPhMode.SelectedIndex + 1);
                emp.CustomPhilHealthAmount = customPh;

                emp.PagIbigDeductionMode = (DeductionMode)(CboPagIbigMode.SelectedIndex + 1);
                emp.PagIbigEmployeeAmount = pagIbigAmount;

                emp.ContributionSchedule = (ContributionSchedule)(CboCutoffSchedule.SelectedIndex + 1);
                emp.IsMinimumWageEarner = ChkMinimumWage.IsChecked == true;
                emp.IsTaxExempt = ChkTaxExempt.IsChecked == true;
                emp.IsActive = true;

                if (_isEditMode)
                {
                    _repo.Update(emp);
                }
                else
                {
                    int newId = _repo.Insert(emp);
                    emp.Id = newId;

                    // Automatically record initial contract
                    var contract = new EmployeeContract
                    {
                        EmployeeId = newId,
                        ContractType = emp.ContractType,
                        PositionTitle = emp.Position,
                        BasicRate = emp.BasicRate,
                        PayType = emp.PayType,
                        StartDate = emp.DateHired,
                        EndDate = emp.ContractEndDate,
                        Status = ContractStatus.Active,
                        Remarks = "Initial Employment Contract"
                    };
                    _contractRepo.Insert(contract);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Save failed:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show($"Operation failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnManageHistory_Click(object sender, RoutedEventArgs e)
        {
            if (_existing == null)
            {
                MessageBox.Show("Please save the new employee record first before adding employment history.", 
                    "Notice", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new EmploymentHistoryDialog(_existing);
            if (dialog.ShowDialog() == true)
            {
                // Reload updated employee details if changed (e.g. reactivated or position updated)
                var updated = _repo.GetById(_existing.Id);
                if (updated != null)
                {
                    _existing = updated;
                    PopulateFields(updated);
                    BtnToggleActive.Content = updated.IsActive ? "⛔ Deactivate Employee" : "✅ Reactivate Employee";
                }
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}
