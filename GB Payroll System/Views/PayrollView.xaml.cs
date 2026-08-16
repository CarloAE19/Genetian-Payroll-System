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
    public partial class PayrollView : UserControl
    {
        private readonly PayrollPeriodRepository _periodRepo = new();
        private readonly PayrollRecordRepository _recordRepo = new();
        private readonly EmployeeRepository      _empRepo    = new();
        private readonly AttendanceRepository    _attRepo    = new();
        private readonly PakyawEntryRepository   _pakyawRepo = new();

        private List<PayrollPeriod>  _periods = [];
        private List<PayrollRunRow>  _rows    = [];
        private PayrollPeriod?       _selected;

        public PayrollView()
        {
            InitializeComponent();
            Loaded += (_, _) => LoadPeriods();
        }

        // ── Period list ──────────────────────────────────────────────────────
        private void LoadPeriods()
        {
            try { _periods = _periodRepo.GetAll(); }
            catch
            {
                _periods =
                [
                    new PayrollPeriod { Id = 1, PeriodCode = "2026-07-2", StartDate = new DateTime(2026,7,16), EndDate = new DateTime(2026,7,31), PayoutDate = new DateTime(2026,8,5), IsClosed = false },
                    new PayrollPeriod { Id = 2, PeriodCode = "2026-07-1", StartDate = new DateTime(2026,7,1),  EndDate = new DateTime(2026,7,15), PayoutDate = new DateTime(2026,7,20), IsClosed = true  },
                ];
            }
            PeriodList.ItemsSource = _periods;
        }

        private void PeriodList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selected = PeriodList.SelectedItem as PayrollPeriod;
            if (_selected == null) return;

            bool isOpen = !_selected.IsClosed;
            TxtPeriodLabel.Text = _selected.PeriodCode;
            TxtPeriodSub.Text   = $"{_selected.StartDate:MMMM dd} – {_selected.EndDate:MMMM dd, yyyy}  |  Payout: {_selected.PayoutDate:MMMM dd, yyyy}";

            BtnRecompute.IsEnabled = isOpen;
            BtnClose.IsEnabled     = isOpen;
            BtnPrintAll.IsEnabled  = true;

            LoadRunData(_selected.Id);
        }

        private void LoadRunData(int periodId)
        {
            try { _rows = _recordRepo.GetByPeriod(periodId); }
            catch { _rows = GenerateSampleRows(); }
            PayrollGrid.ItemsSource = _rows;
            UpdateSummaryBar();
        }

        private void UpdateSummaryBar()
        {
            decimal gross      = _rows.Sum(r => r.GrossPay);
            decimal deductions = _rows.Sum(r => r.TotalDeductions);
            decimal net        = _rows.Sum(r => r.NetPay);
            TxtTotalGross.Text      = $"Total Gross: ₱{gross:N2}";
            TxtTotalDeductions.Text = $"Total Deductions: ₱{deductions:N2}";
            TxtTotalNet.Text        = $"Total Net Payroll: ₱{net:N2}";
            TxtEmployeeCount.Text   = $"{_rows.Count} employee(s)";
        }

        // ── Run payroll ──────────────────────────────────────────────────────
        private void BtnRecompute_Click(object sender, RoutedEventArgs e)
        {
            if (_selected == null) return;

            var confirm = MessageBox.Show(
                $"Run payroll computation for {_selected.PeriodCode}?\n\nThis will replace any existing computed records for this period.",
                "Confirm Payroll Run", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                var employees = _empRepo.GetAll(activeOnly: true);
                var computed  = new List<PayrollRecord>();

                foreach (var emp in employees)
                {
                    var attendances   = _attRepo.GetByDateRange(_selected.StartDate, _selected.EndDate, emp.Id)
                                                .Select(vm => new Attendance
                                                {
                                                    Id                 = vm.Id,
                                                    EmployeeId         = vm.EmployeeId,
                                                    Date               = vm.Date,
                                                    TimeIn             = vm.TimeIn,
                                                    TimeOut            = vm.TimeOut,
                                                    LateMinutes        = vm.LateMinutes,
                                                    UndertimeMinutes   = vm.UndertimeMinutes,
                                                    RegularHoursWorked = vm.RegularHoursWorked,
                                                    OvertimeHours      = vm.OvertimeHours,
                                                    NightDiffHours     = vm.NightDiffHours,
                                                    Status             = vm.Status
                                                }).ToList();

                    var pakyawEntries = _pakyawRepo.GetByDateRange(_selected.StartDate, _selected.EndDate, emp.Id)
                                                   .Select(vm => new PakyawEntry
                                                   {
                                                       EmployeeId        = vm.EmployeeId,
                                                       PakyawRateId      = vm.PakyawRateId,
                                                       WorkDate          = vm.WorkDate,
                                                       QuantityCompleted = vm.QuantityCompleted,
                                                       UnitRate          = vm.UnitRate
                                                   }).ToList();

                    var record = PayrollService.ComputePayrollRecord(emp, _selected, attendances, pakyawEntries);
                    computed.Add(record);
                }

                _recordRepo.DeleteByPeriod(_selected.Id);
                _recordRepo.BulkInsert(computed);
                LoadRunData(_selected.Id);

                MessageBox.Show($"✅ Payroll computed for {computed.Count} employee(s).",
                    "Payroll Run Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Payroll run failed:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Close period ─────────────────────────────────────────────────────
        private void BtnClosePeriod_Click(object sender, RoutedEventArgs e)
        {
            if (_selected == null || _selected.IsClosed) return;

            var confirm = MessageBox.Show(
                $"Close payroll period {_selected.PeriodCode}?\n\nOnce closed, this period cannot be re-processed.",
                "Confirm Close", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                _periodRepo.Close(_selected.Id, AuthService.CurrentUser?.Username ?? "admin");
                LoadPeriods();
                BtnRecompute.IsEnabled = false;
                BtnClose.IsEnabled     = false;
                MessageBox.Show("Period closed successfully.", "Done", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) { MessageBox.Show($"Failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        // ── New period dialog ─────────────────────────────────────────────────
        private void BtnNewPeriod_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new PayrollPeriodDialog();
            if (dialog.ShowDialog() == true) LoadPeriods();
        }

        // ── Payslip ──────────────────────────────────────────────────────────
        private void BtnPayslip_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int empId && _selected != null)
            {
                var row = _rows.FirstOrDefault(r => r.EmployeeId == empId);
                if (row != null)
                    new PayslipWindow(row, _selected).ShowDialog();
            }
        }

        private void BtnPrintAll_Click(object sender, RoutedEventArgs e)
        {
            if (_selected == null || _rows.Count == 0) return;
            foreach (var row in _rows)
                new PayslipWindow(row, _selected).ShowDialog();
        }

        // ── Sample offline data ───────────────────────────────────────────────
        private static List<PayrollRunRow> GenerateSampleRows() =>
        [
            new PayrollRunRow { EmployeeId = 1, EmployeeCode = "EMP-2026-001", FullName = "Juan Dela Cruz",   Department = "Construction", Position = "Foreman",    PayType = PayType.Daily,   BasicPay = 12200m,  PakyawPay = 1200m, OvertimePay = 500m,  SssEmployee = 675m,  PhilHealthEmployee = 325m,  PagIbigEmployee = 100m, WithholdingTax = 0m,      TardinessDeduction = 152m },
            new PayrollRunRow { EmployeeId = 2, EmployeeCode = "EMP-2026-002", FullName = "Maria Santos",     Department = "HR",           Position = "HR Officer", PayType = PayType.Monthly, BasicPay = 12500m,  PakyawPay = 0m,    OvertimePay = 0m,    SssEmployee = 787.50m, PhilHealthEmployee = 500m, PagIbigEmployee = 100m, WithholdingTax = 937.50m, TardinessDeduction = 0m   },
            new PayrollRunRow { EmployeeId = 3, EmployeeCode = "EMP-2026-003", FullName = "Pedro Reyes",      Department = "Accounting",   Position = "Accountant", PayType = PayType.Monthly, BasicPay = 14000m,  PakyawPay = 0m,    OvertimePay = 750m,  SssEmployee = 900m,    PhilHealthEmployee = 550m, PagIbigEmployee = 100m, WithholdingTax = 1250m,   TardinessDeduction = 0m   },
        ];
    }
}
