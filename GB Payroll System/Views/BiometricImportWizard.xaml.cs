using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using GB_Payroll_System.Data;
using GB_Payroll_System.Models;
using GB_Payroll_System.Services;

namespace GB_Payroll_System.Views
{
    // Preview row shown in the wizard's DataGrid
    public class BiometricPreviewRow
    {
        public string BiometricId { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public TimeSpan? TimeIn { get; set; }
        public TimeSpan? TimeOut { get; set; }
        public double RegularHrs { get; set; }
        public double OtHrs { get; set; }
        public bool IsMatched { get; set; }
        public int MatchedEmployeeId { get; set; }
        public string MatchedEmployeeName { get; set; } = "— No match —";

        public string TimeInDisplay  => TimeIn.HasValue  ? DateTime.Today.Add(TimeIn.Value).ToString("hh:mm tt")  : "—";
        public string TimeOutDisplay => TimeOut.HasValue ? DateTime.Today.Add(TimeOut.Value).ToString("hh:mm tt") : "—";
    }

    public partial class BiometricImportWizard : Window
    {
        private readonly AttendanceRepository _attRepo = new();
        private readonly EmployeeRepository   _empRepo = new();

        private string? _selectedFilePath;
        private List<BiometricPreviewRow> _previewRows = [];
        private List<Employee> _employees = [];

        public BiometricImportWizard()
        {
            InitializeComponent();
            try { _employees = _empRepo.GetAll(); } catch { /* offline */ }
        }

        // ── STEP 1: Browse file ──────────────────────────────────────────────
        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title  = "Select Biometric Log File",
                Filter = "CSV / Text Files|*.csv;*.txt;*.log|All Files|*.*"
            };

            if (dialog.ShowDialog() != true) return;
            _selectedFilePath = dialog.FileName;
            TxtFilePath.Text = Path.GetFileName(_selectedFilePath);
            TxtFilePath.Foreground = new SolidColorBrush(Color.FromRgb(26, 32, 44));
            BtnPreview.IsEnabled = true;
            TxtFooterHint.Text = "Click 'Preview Records' to parse the file before importing.";
            BtnImport.IsEnabled = false;
            PreviewGrid.ItemsSource = null;
            TxtPreviewCount.Text = "";
            BannerResult.Visibility = Visibility.Collapsed;
        }

        // ── STEP 2: Parse & preview ──────────────────────────────────────────
        private void BtnPreview_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFilePath == null) return;
            BannerResult.Visibility = Visibility.Collapsed;

            var shiftStart = TimeSpan.ParseExact(
                (CboShiftStart.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "08:00", @"hh\:mm", null);
            var shiftEnd = TimeSpan.ParseExact(
                (CboShiftEnd.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "17:00", @"hh\:mm", null);

            try
            {
                var rawPunches = ParseFile(_selectedFilePath);

                // Group by (EmployeeID, Date) — first punch = IN, last punch = OUT
                _previewRows = rawPunches
                    .GroupBy(p => (p.BiometricId, p.Date.Date))
                    .Select(g =>
                    {
                        var ordered   = g.OrderBy(p => p.Time).ToList();
                        var timeIn    = ordered.FirstOrDefault(p => p.PunchType == 1)?.Time
                                     ?? ordered.First().Time;
                        var timeOut   = ordered.LastOrDefault(p => p.PunchType == 0)?.Time
                                     ?? (ordered.Count > 1 ? ordered.Last().Time : (TimeSpan?)null);

                        double reg = 0, ot = 0;
                        if (timeIn.HasValue && timeOut.HasValue)
                            (_, _, reg, ot, _) = AttendanceService.CalculateShiftHours(
                                timeIn.Value, timeOut.Value, shiftStart, shiftEnd);

                        // Match to employee
                        var emp = _employees.FirstOrDefault(e2 => e2.BiometricUserId == g.Key.BiometricId);
                        return new BiometricPreviewRow
                        {
                            BiometricId         = g.Key.BiometricId,
                            Date                = g.Key.Date,
                            TimeIn              = timeIn,
                            TimeOut             = timeOut,
                            RegularHrs          = reg,
                            OtHrs               = ot,
                            IsMatched           = emp != null,
                            MatchedEmployeeId   = emp?.Id ?? 0,
                            MatchedEmployeeName = emp != null ? $"{emp.FullName} ({emp.EmployeeCode})" : "— No match —"
                        };
                    })
                    .OrderBy(r => r.Date).ThenBy(r => r.BiometricId)
                    .ToList();

                PreviewGrid.ItemsSource = _previewRows;
                int matched   = _previewRows.Count(r => r.IsMatched);
                int unmatched = _previewRows.Count - matched;
                TxtPreviewCount.Text = $"{_previewRows.Count} records  |  ✅ {matched} matched  |  ❌ {unmatched} unmatched";
                BtnImport.IsEnabled  = matched > 0;
                TxtFooterHint.Text   = matched > 0
                    ? $"Ready to import {matched} matched record(s). Unmatched rows will be skipped."
                    : "No records matched any employee. Check BiometricUserId mappings in Employee 201 Files.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to parse file:\n{ex.Message}", "Parse Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── STEP 3: Import to database ───────────────────────────────────────
        private void BtnImportConfirm_Click(object sender, RoutedEventArgs e)
        {
            var matched = _previewRows.Where(r => r.IsMatched).ToList();
            if (matched.Count == 0) return;

            var confirm = MessageBox.Show(
                $"Import {matched.Count} attendance record(s) into the database?\n\nDuplicate entries (same employee + date) will be skipped automatically.",
                "Confirm Import", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            var shiftStart = TimeSpan.ParseExact(
                (CboShiftStart.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "08:00", @"hh\:mm", null);
            var shiftEnd = TimeSpan.ParseExact(
                (CboShiftEnd.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "17:00", @"hh\:mm", null);

            try
            {
                var records = matched.Select(r =>
                {
                    double late = 0, undertime = 0, reg = 0, ot = 0, nightDiff = 0;
                    if (r.TimeIn.HasValue && r.TimeOut.HasValue)
                        (late, undertime, reg, ot, nightDiff) = AttendanceService.CalculateShiftHours(
                            r.TimeIn.Value, r.TimeOut.Value, shiftStart, shiftEnd);

                    return new Attendance
                    {
                        EmployeeId           = r.MatchedEmployeeId,
                        Date                 = r.Date,
                        TimeIn               = r.TimeIn,
                        TimeOut              = r.TimeOut,
                        LateMinutes          = late,
                        UndertimeMinutes     = undertime,
                        RegularHoursWorked   = reg,
                        OvertimeHours        = ot,
                        NightDiffHours       = nightDiff,
                        Status               = AttendanceStatus.Present,
                        IsManuallyAdjusted   = false,
                        AdjustedByUsername   = AuthService.CurrentUser?.Username ?? "system"
                    };
                }).ToList();

                int inserted = _attRepo.BulkInsert(records);
                int skipped  = matched.Count - inserted;

                ShowResult(
                    $"✅  Import complete!  {inserted} record(s) inserted.  {skipped} duplicate(s) skipped.",
                    success: true);

                BtnImport.IsEnabled = false;
                DialogResult = inserted > 0;
            }
            catch (Exception ex)
            {
                ShowResult($"❌  Import failed: {ex.Message}", success: false);
            }
        }

        // ── File parser ──────────────────────────────────────────────────────
        private record RawPunch(string BiometricId, DateTime Date, TimeSpan? Time, int PunchType);

        private static List<RawPunch> ParseFile(string path)
        {
            var results = new List<RawPunch>();
            var lines   = File.ReadAllLines(path);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#')) continue;

                var cols = line.Split([',', '\t', ';'], StringSplitOptions.TrimEntries);
                if (cols.Length < 3) continue;

                string bioId = cols[0];
                if (!DateTime.TryParse(cols[1], out DateTime date)) continue;
                if (!TimeSpan.TryParse(cols[2], out TimeSpan time)) continue;

                int punchType = 1; // default = IN
                if (cols.Length >= 4 && int.TryParse(cols[3], out int pt))
                    punchType = pt;

                results.Add(new RawPunch(bioId, date, time, punchType));
            }
            return results;
        }

        private void ShowResult(string msg, bool success)
        {
            TxtResult.Text = msg;
            BannerResult.Background = success
                ? new SolidColorBrush(Color.FromRgb(198, 246, 213))
                : new SolidColorBrush(Color.FromRgb(254, 215, 215));
            TxtResult.Foreground = success
                ? new SolidColorBrush(Color.FromRgb(39, 103, 73))
                : new SolidColorBrush(Color.FromRgb(155, 44, 44));
            BannerResult.Visibility = Visibility.Visible;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}
