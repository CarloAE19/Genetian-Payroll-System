using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GB_Payroll_System.Data;
using GB_Payroll_System.Models;

namespace GB_Payroll_System.Views
{
    public partial class AttendanceView : UserControl
    {
        private readonly AttendanceRepository _repo = new();
        private List<AttendanceViewModel> _allRecords = [];

        public AttendanceView()
        {
            InitializeComponent();
            Loaded += (_, _) => InitDefaults();
        }

        private void InitDefaults()
        {
            // Default: current month's first half or second half based on today
            DateTime today = DateTime.Today;
            bool isSecondHalf = today.Day > 15;
            DpFrom.SelectedDate = isSecondHalf ? new DateTime(today.Year, today.Month, 16) : new DateTime(today.Year, today.Month, 1);
            DpTo.SelectedDate = isSecondHalf
                ? new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month))
                : new DateTime(today.Year, today.Month, 15);
            CboStatus.SelectedIndex = 0;
            LoadRecords();
        }

        private void LoadRecords()
        {
            if (DpFrom.SelectedDate == null || DpTo.SelectedDate == null) return;
            try
            {
                _allRecords = _repo.GetByDateRange(DpFrom.SelectedDate.Value, DpTo.SelectedDate.Value);
            }
            catch
            {
                // Offline demo data
                _allRecords = GenerateSampleData();
            }
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (AttendanceGrid == null) return;

            var filtered = _allRecords.AsEnumerable();

            // Status filter
            if (CboStatus?.SelectedIndex > 0)
            {
                AttendanceStatus status = CboStatus.SelectedIndex switch
                {
                    1 => AttendanceStatus.Present,
                    2 => AttendanceStatus.Absent,
                    3 => AttendanceStatus.OnLeave,
                    4 => AttendanceStatus.HalfDay,
                    5 => AttendanceStatus.Holiday,
                    _ => AttendanceStatus.Present
                };
                filtered = filtered.Where(r => r.Status == status);
            }

            var list = filtered.ToList();
            AttendanceGrid.ItemsSource = list;
            TxtRecordCount.Text = $"{list.Count} records";

            // Summary bar
            TxtPresentCount.Text  = $"Present: {list.Count(r => r.Status == AttendanceStatus.Present)}";
            TxtAbsentCount.Text   = $"Absent: {list.Count(r => r.Status == AttendanceStatus.Absent)}";
            TxtLeaveCount.Text    = $"On Leave: {list.Count(r => r.Status == AttendanceStatus.OnLeave)}";
            TxtLateCount.Text     = $"Late: {list.Count(r => r.LateMinutes > 0)}";
            TxtTotalOT.Text       = $"Total OT: {list.Sum(r => r.OvertimeHours):F2} hrs";
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e) => LoadRecords();

        private void AttendanceGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (AttendanceGrid.SelectedItem is AttendanceViewModel vm)
                OpenCorrection(vm.Id);
        }

        private void BtnCorrect_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
                OpenCorrection(id);
        }

        private void OpenCorrection(int attendanceId)
        {
            var record = _allRecords.FirstOrDefault(r => r.Id == attendanceId);
            if (record == null) return;
            var dialog = new AttendanceCorrectionDialog(record);
            if (dialog.ShowDialog() == true) LoadRecords();
        }

        private void BtnAddManual_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AttendanceCorrectionDialog(null);
            if (dialog.ShowDialog() == true) LoadRecords();
        }

        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            var wizard = new BiometricImportWizard();
            if (wizard.ShowDialog() == true) LoadRecords();
        }

        // ── Sample offline data ──────────────────────────────────────────────
        private static List<AttendanceViewModel> GenerateSampleData()
        {
            var today = DateTime.Today;
            var shift = (Start: new TimeSpan(8, 0, 0), End: new TimeSpan(17, 0, 0));
            var employees = new[]
            {
                (Id: 1, Code: "EMP-2026-001", Name: "Juan Dela Cruz",   Dept: "Construction"),
                (Id: 2, Code: "EMP-2026-002", Name: "Maria Santos",     Dept: "HR"),
                (Id: 3, Code: "EMP-2026-003", Name: "Pedro Reyes",      Dept: "Accounting"),
            };

            var list = new List<AttendanceViewModel>();
            var rng = new Random(42);
            for (int d = 0; d < 10; d++)
            {
                var date = today.AddDays(-d);
                if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) continue;
                foreach (var emp in employees)
                {
                    int roll = rng.Next(100);
                    AttendanceStatus status = roll < 80 ? AttendanceStatus.Present
                                           : roll < 90 ? AttendanceStatus.Absent
                                           : AttendanceStatus.OnLeave;

                    TimeSpan? timeIn = null, timeOut = null;
                    double late = 0, ot = 0, reg = 0;

                    if (status == AttendanceStatus.Present)
                    {
                        int lateMins = rng.Next(0, 30);
                        timeIn  = shift.Start.Add(TimeSpan.FromMinutes(lateMins));
                        timeOut = shift.End.Add(TimeSpan.FromMinutes(rng.Next(0, 120)));
                        late    = lateMins;
                        reg     = 8 - (lateMins / 60.0);
                        ot      = Math.Max(0, (timeOut.Value - shift.End).TotalHours);
                    }

                    list.Add(new AttendanceViewModel
                    {
                        Id = list.Count + 1, EmployeeId = emp.Id, EmployeeCode = emp.Code,
                        FullName = emp.Name, Department = emp.Dept, Date = date,
                        TimeIn = timeIn, TimeOut = timeOut, LateMinutes = late,
                        RegularHoursWorked = reg, OvertimeHours = ot, Status = status,
                        IsManuallyAdjusted = false
                    });
                }
            }
            return list;
        }
    }
}
