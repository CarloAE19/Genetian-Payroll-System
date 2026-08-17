using System;
using System.Windows;
using System.Windows.Controls;
using GB_Payroll_System.Data;
using GB_Payroll_System.Models;
using GB_Payroll_System.Services;

namespace GB_Payroll_System.Views
{
    public partial class AttendanceCorrectionDialog : Window
    {
        private readonly AttendanceRepository _repo = new();
        private AttendanceViewModel? _existing;
        private bool _isManualEntry;

        public AttendanceCorrectionDialog(AttendanceViewModel? record)
        {
            InitializeComponent();
            _existing = record;
            _isManualEntry = record == null;

            if (_isManualEntry)
            {
                TxtDialogTitle.Text = "Manual Attendance Entry";
                TxtSubtitle.Text = "Add a new attendance record manually";
                ManualEntryPanel.Visibility = Visibility.Visible;
                RecordInfoCard.Visibility = Visibility.Collapsed;
                DpManualDate.SelectedDate = DateTime.Today;
                CboStatus.SelectedIndex = 0;
                TxtTimeIn.Text = "08:00";
                TxtTimeOut.Text = "17:00";
            }
            else
            {
                TxtDialogTitle.Text = "Correct Attendance Record";
                TxtSubtitle.Text = $"{record!.EmployeeCode} — {record.Date:MMMM dd, yyyy (dddd)}";
                TxtInfoEmployee.Text = record.FullName;
                TxtInfoDate.Text = record.Date.ToString("MMM dd, yyyy");
                TxtInfoOriginal.Text = $"{record.TimeInDisplay} → {record.TimeOutDisplay}";

                TxtTimeIn.Text = record.TimeIn.HasValue
                    ? DateTime.Today.Add(record.TimeIn.Value).ToString("HH:mm") : "";
                TxtTimeOut.Text = record.TimeOut.HasValue
                    ? DateTime.Today.Add(record.TimeOut.Value).ToString("HH:mm") : "";

                CboStatus.SelectedIndex = record.Status switch
                {
                    AttendanceStatus.Present  => 0,
                    AttendanceStatus.Absent   => 1,
                    AttendanceStatus.OnLeave  => 2,
                    AttendanceStatus.HalfDay  => 3,
                    AttendanceStatus.Holiday  => 4,
                    AttendanceStatus.AWOL     => 5,
                    _ => 0
                };
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            BannerError.Visibility = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(TxtReason.Text.Trim()))
            {
                ShowError("A reason for correction is required for audit trail.");
                return;
            }

            TimeSpan? timeIn = null, timeOut = null;

            if (!string.IsNullOrWhiteSpace(TxtTimeIn.Text))
            {
                if (!TimeSpan.TryParseExact(TxtTimeIn.Text.Trim(), @"hh\:mm", null, out var ti))
                {
                    ShowError("Invalid Time In format. Use HH:mm (e.g. 08:05).");
                    return;
                }
                timeIn = ti;
            }

            if (!string.IsNullOrWhiteSpace(TxtTimeOut.Text))
            {
                if (!TimeSpan.TryParseExact(TxtTimeOut.Text.Trim(), @"hh\:mm", null, out var to))
                {
                    ShowError("Invalid Time Out format. Use HH:mm (e.g. 17:30).");
                    return;
                }
                timeOut = to;
            }

            if (timeIn.HasValue && timeOut.HasValue && timeOut <= timeIn)
            {
                ShowError("Time Out must be after Time In.");
                return;
            }

            // Parse selected shift
            var shiftTag = (CboShift.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "08:00-17:00";
            var parts = shiftTag.Split('-');
            var shiftStart = TimeSpan.ParseExact(parts[0], @"hh\:mm", null);
            var shiftEnd   = TimeSpan.ParseExact(parts[1], @"hh\:mm", null);

            // Compute hours
            double late = 0, undertime = 0, reg = 0, ot = 0, nightDiff = 0;
            if (timeIn.HasValue && timeOut.HasValue)
            {
                (late, undertime, reg, ot, nightDiff) =
                    AttendanceService.CalculateShiftHours(timeIn.Value, timeOut.Value, shiftStart, shiftEnd);
            }

            AttendanceStatus status = CboStatus.SelectedIndex switch
            {
                1 => AttendanceStatus.Absent,
                2 => AttendanceStatus.OnLeave,
                3 => AttendanceStatus.HalfDay,
                4 => AttendanceStatus.Holiday,
                5 => AttendanceStatus.AWOL,
                _ => AttendanceStatus.Present
            };

            try
            {
                if (_isManualEntry)
                {
                    // Lookup employee by code
                    var empRepo = new EmployeeRepository();
                    // For now build record with manual date
                    var newRec = new Attendance
                    {
                        Date                 = DpManualDate.SelectedDate ?? DateTime.Today,
                        TimeIn               = timeIn,
                        TimeOut              = timeOut,
                        LateMinutes          = late,
                        UndertimeMinutes     = undertime,
                        RegularHoursWorked   = reg,
                        OvertimeHours        = ot,
                        NightDiffHours       = nightDiff,
                        Status               = status,
                        IsManuallyAdjusted   = true,
                        AdjustmentReason     = TxtReason.Text.Trim(),
                        AdjustedByUsername   = AuthService.CurrentUser?.Username ?? "admin"
                    };
                    _repo.Insert(newRec);
                }
                else
                {
                    var rec = _repo.GetById(_existing!.Id);
                    if (rec != null)
                    {
                        rec.TimeIn               = timeIn;
                        rec.TimeOut              = timeOut;
                        rec.LateMinutes          = late;
                        rec.UndertimeMinutes     = undertime;
                        rec.RegularHoursWorked   = reg;
                        rec.OvertimeHours        = ot;
                        rec.NightDiffHours       = nightDiff;
                        rec.Status               = status;
                        rec.IsManuallyAdjusted   = true;
                        rec.AdjustmentReason     = TxtReason.Text.Trim();
                        rec.AdjustedByUsername   = AuthService.CurrentUser?.Username ?? "admin";
                        _repo.Update(rec);
                    }
                }

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
