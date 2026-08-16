using System;
using System.Windows;
using System.Windows.Controls;
using GB_Payroll_System.Data;
using GB_Payroll_System.Models;
using GB_Payroll_System.Services;

namespace GB_Payroll_System.Views
{
    public partial class PayrollPeriodDialog : Window
    {
        private readonly PayrollPeriodRepository _repo = new();

        public PayrollPeriodDialog()
        {
            InitializeComponent();
            var today = DateTime.Today;
            bool isSecondHalf = today.Day > 15;
            if (isSecondHalf) ApplySecondHalf(today);
            else ApplyFirstHalf(today);
        }

        private void BtnFirstHalf_Click(object sender, RoutedEventArgs e) => ApplyFirstHalf(DateTime.Today);
        private void BtnSecondHalf_Click(object sender, RoutedEventArgs e) => ApplySecondHalf(DateTime.Today);

        private void ApplyFirstHalf(DateTime reference)
        {
            DpStart.SelectedDate  = new DateTime(reference.Year, reference.Month, 1);
            DpEnd.SelectedDate    = new DateTime(reference.Year, reference.Month, 15);
            DpPayout.SelectedDate = new DateTime(reference.Year, reference.Month, 20);
        }

        private void ApplySecondHalf(DateTime reference)
        {
            int lastDay = DateTime.DaysInMonth(reference.Year, reference.Month);
            DpStart.SelectedDate  = new DateTime(reference.Year, reference.Month, 16);
            DpEnd.SelectedDate    = new DateTime(reference.Year, reference.Month, lastDay);
            // Payout on the 5th of next month
            var payout = new DateTime(reference.Year, reference.Month, 1).AddMonths(1).AddDays(4);
            DpPayout.SelectedDate = payout;
        }

        private void Dates_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (DpStart.SelectedDate == null || DpEnd.SelectedDate == null) return;
            var start = DpStart.SelectedDate.Value;
            // Auto-generate period code: YYYY-MM-[1 or 2]
            int half = start.Day <= 15 ? 1 : 2;
            TxtCode.Text = $"{start.Year}-{start.Month:D2}-{half}";
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            BannerError.Visibility = Visibility.Collapsed;

            if (DpStart.SelectedDate == null) { ShowError("Start Date is required."); return; }
            if (DpEnd.SelectedDate == null)   { ShowError("End Date is required."); return; }
            if (DpPayout.SelectedDate == null) { ShowError("Payout Date is required."); return; }
            if (DpEnd.SelectedDate <= DpStart.SelectedDate) { ShowError("End Date must be after Start Date."); return; }
            if (string.IsNullOrWhiteSpace(TxtCode.Text)) { ShowError("Period Code is required."); return; }

            try
            {
                var period = new PayrollPeriod
                {
                    PeriodCode          = TxtCode.Text.Trim(),
                    StartDate           = DpStart.SelectedDate!.Value,
                    EndDate             = DpEnd.SelectedDate!.Value,
                    PayoutDate          = DpPayout.SelectedDate!.Value,
                    IsClosed            = false,
                    ProcessedByUsername = AuthService.CurrentUser?.Username ?? "admin",
                    CreatedAt           = DateTime.UtcNow
                };
                _repo.Insert(period);
                DialogResult = true;
                Close();
            }
            catch (Exception ex) { ShowError($"Save failed: {ex.Message}"); }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();

        private void ShowError(string msg) { TxtError.Text = msg; BannerError.Visibility = Visibility.Visible; }
    }
}
