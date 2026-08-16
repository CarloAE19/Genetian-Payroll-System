using System;
using System.Windows;
using System.Windows.Controls;
using GB_Payroll_System.Data;
using GB_Payroll_System.Models;

namespace GB_Payroll_System.Views
{
    public partial class PakyawTaskFormDialog : Window
    {
        private readonly PakyawRateRepository _repo = new();
        private PakyawRate? _existing;

        public PakyawTaskFormDialog(PakyawRate? rate)
        {
            InitializeComponent();
            _existing = rate;

            if (rate != null)
            {
                TxtDialogTitle.Text = "Edit Pakyaw Task";
                BtnSave.Content     = "💾  Save Changes";
                BtnToggleActive.Visibility = Visibility.Visible;
                BtnToggleActive.Content = rate.IsActive ? "⛔ Deactivate Task" : "✅ Reactivate Task";

                TxtTaskCode.Text = rate.TaskCode;
                TxtTaskName.Text = rate.TaskName;
                TxtRate.Text     = rate.RatePerUnit.ToString("N2");
                CboUnit.Text     = rate.UnitOfMeasure;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            BannerError.Visibility = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(TxtTaskCode.Text)) { ShowError("Task Code is required."); return; }
            if (string.IsNullOrWhiteSpace(TxtTaskName.Text)) { ShowError("Task Description is required."); return; }
            if (!decimal.TryParse(TxtRate.Text.Replace(",", ""), out decimal rate) || rate <= 0)
            { ShowError("Please enter a valid Rate Per Unit (must be > 0)."); return; }

            string unit = CboUnit.Text.Trim();
            if (string.IsNullOrWhiteSpace(unit)) { ShowError("Unit of Measure is required."); return; }

            try
            {
                var task = _existing ?? new PakyawRate();
                task.TaskCode      = TxtTaskCode.Text.Trim().ToUpper();
                task.TaskName      = TxtTaskName.Text.Trim();
                task.RatePerUnit   = rate;
                task.UnitOfMeasure = unit;
                task.IsActive      = true;

                if (_existing != null) _repo.Update(task);
                else                  _repo.Insert(task);

                DialogResult = true;
                Close();
            }
            catch (Exception ex) { ShowError($"Save failed: {ex.Message}"); }
        }

        private void BtnToggleActive_Click(object sender, RoutedEventArgs e)
        {
            if (_existing == null) return;
            bool newStatus = !_existing.IsActive;
            var confirm = MessageBox.Show(
                $"{(newStatus ? "Reactivate" : "Deactivate")} task '{_existing.TaskName}'?",
                "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;
            try { _repo.SetActive(_existing.Id, newStatus); DialogResult = true; Close(); }
            catch (Exception ex) { ShowError($"Failed: {ex.Message}"); }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();

        private void ShowError(string msg) { TxtError.Text = msg; BannerError.Visibility = Visibility.Visible; }
    }
}
