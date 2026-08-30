using System;
using System.Windows;
using GB_Payroll_System.Data;
using GB_Payroll_System.Models;

namespace GB_Payroll_System.Views
{
    public partial class BranchFormDialog : Window
    {
        private readonly BranchRepository _repo = new();
        private readonly Branch? _existing;
        private readonly bool _isEditMode;

        public BranchFormDialog(Branch? branch = null)
        {
            InitializeComponent();
            _existing = branch;
            _isEditMode = branch != null;

            if (_isEditMode)
            {
                TxtDialogTitle.Text = $"🏢 Edit Branch — {branch!.Name}";
                TxtCode.Text = branch.Code;
                TxtName.Text = branch.Name;
                TxtLocation.Text = branch.Location;
                ChkActive.IsChecked = branch.IsActive;
                BtnSave.Content = "💾  Update Branch";
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string code = TxtCode.Text.Trim().ToUpper();
            string name = TxtName.Text.Trim();
            string location = TxtLocation.Text.Trim();
            bool isActive = ChkActive.IsChecked == true;

            if (string.IsNullOrWhiteSpace(code))
            {
                MessageBox.Show("Please enter a Branch Code (e.g. MAIN, DVO).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtCode.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Please enter a Branch Name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtName.Focus();
                return;
            }

            try
            {
                var branch = _existing ?? new Branch();
                branch.Code = code;
                branch.Name = name;
                branch.Location = location;
                branch.IsActive = isActive;

                if (_isEditMode)
                {
                    _repo.Update(branch);
                }
                else
                {
                    _repo.Insert(branch);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save branch:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}
