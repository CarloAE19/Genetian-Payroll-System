using System;
using System.Windows;
using System.Windows.Controls;
using GB_Payroll_System.Data;
using GB_Payroll_System.Models;

namespace GB_Payroll_System.Views
{
    public partial class HolidayFormDialog : Window
    {
        private readonly HolidayRepository _repo = new();
        private Holiday? _existing;

        public HolidayFormDialog(Holiday? holiday)
        {
            InitializeComponent();
            _existing = holiday;
            DpDate.SelectedDate = DateTime.Today;
            CboType.SelectedIndex = 0;

            if (holiday != null)
            {
                TxtDialogTitle.Text = "Edit Holiday";
                BtnSave.Content = "💾  Save Changes";
                TxtName.Text = holiday.Name;
                DpDate.SelectedDate = holiday.Date;
                CboType.SelectedIndex = (int)holiday.Type - 1;
                TxtDeclaredBy.Text = holiday.DeclaredBy;
                TxtWorkedMultiplier.Text = holiday.WorkedMultiplier.ToString("N2");
                TxtUnworkedMultiplier.Text = holiday.UnworkedMultiplier.ToString("N2");
            }
        }

        private void CboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CustomRatePanel == null) return;

            // Show custom rate fields only for Local Special Holiday
            bool isCustom = CboType.SelectedIndex == 3;
            CustomRatePanel.Visibility = isCustom ? Visibility.Visible : Visibility.Collapsed;

            // Auto-fill default multipliers for standard types
            if (!isCustom)
            {
                (decimal worked, decimal unworked) = CboType.SelectedIndex switch
                {
                    0 => (2.00m, 1.00m), // Regular
                    1 => (1.30m, 0.00m), // Special Non-Working
                    2 => (1.00m, 1.00m), // Special Working
                    _ => (1.30m, 0.00m)
                };
                TxtWorkedMultiplier.Text = worked.ToString("N2");
                TxtUnworkedMultiplier.Text = unworked.ToString("N2");
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            BannerError.Visibility = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(TxtName.Text))
            {
                ShowError("Holiday name is required.");
                return;
            }
            if (DpDate.SelectedDate == null)
            {
                ShowError("Please select the holiday date.");
                return;
            }
            if (!decimal.TryParse(TxtWorkedMultiplier.Text, out decimal worked) || worked < 0)
            {
                ShowError("Invalid worked multiplier.");
                return;
            }
            if (!decimal.TryParse(TxtUnworkedMultiplier.Text, out decimal unworked) || unworked < 0)
            {
                ShowError("Invalid unworked multiplier.");
                return;
            }

            try
            {
                var holiday = _existing ?? new Holiday();
                holiday.Name = TxtName.Text.Trim();
                holiday.Date = DpDate.SelectedDate!.Value;
                holiday.Type = (HolidayType)(CboType.SelectedIndex + 1);
                holiday.WorkedMultiplier = worked;
                holiday.UnworkedMultiplier = unworked;
                holiday.DeclaredBy = TxtDeclaredBy.Text.Trim();
                holiday.IsActive = true;

                if (_existing != null)
                    _repo.Update(holiday);
                else
                    _repo.Insert(holiday);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowError($"Save failed: {ex.Message}");
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
