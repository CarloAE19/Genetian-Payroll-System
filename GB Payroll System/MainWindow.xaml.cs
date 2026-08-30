using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Dapper;
using GB_Payroll_System.Data;
using GB_Payroll_System.Models;
using GB_Payroll_System.Services;
using GB_Payroll_System.Views;

namespace GB_Payroll_System
{
    public partial class MainWindow : Window
    {
        private EmployeeView?          _employeeView;
        private HolidayView?           _holidayView;
        private AttendanceView?        _attendanceView;
        private PayrollView?           _payrollView;
        private SettingsView?          _settingsView;
        private GovernmentReportsView? _govReportsView;
        private LeaveManagementView?   _leaveView;

        public MainWindow()
        {
            InitializeComponent();
            LoadUserData();
            ShowDashboard();
        }

        private void LoadUserData()
        {
            var user = AuthService.CurrentUser;
            if (user != null)
            {
                TxtUserFullName.Text = user.FullName;
                TxtUserRole.Text = user.Role.ToString().ToUpper();
                ApplyRolePermissions(user.Role);
            }
        }

        private void ApplyRolePermissions(UserRole role)
        {
            // Admin and HR have FULL ACCESS to all modules & data
            bool isFullAccess = (role == UserRole.Admin || role == UserRole.HR);

            SecMain.Visibility       = Visibility.Visible;
            NavDashboard.Visibility  = Visibility.Visible;

            bool canSeeHr = isFullAccess || role == UserRole.Accounting || role == UserRole.Management;
            SecHr.Visibility         = canSeeHr ? Visibility.Visible : Visibility.Collapsed;
            NavEmployees.Visibility  = canSeeHr ? Visibility.Visible : Visibility.Collapsed;
            NavPromotions.Visibility = isFullAccess ? Visibility.Visible : Visibility.Collapsed;

            bool canSeeTime = isFullAccess || role == UserRole.Accounting || role == UserRole.Management;
            SecTime.Visibility       = canSeeTime ? Visibility.Visible : Visibility.Collapsed;
            NavAttendance.Visibility = canSeeTime ? Visibility.Visible : Visibility.Collapsed;
            NavLeave.Visibility      = canSeeTime ? Visibility.Visible : Visibility.Collapsed;
            NavHolidays.Visibility   = (isFullAccess || role == UserRole.Management) ? Visibility.Visible : Visibility.Collapsed;

            bool canSeePayroll = isFullAccess || role == UserRole.Accounting || role == UserRole.Management;
            SecPayroll.Visibility    = canSeePayroll ? Visibility.Visible : Visibility.Collapsed;
            NavPayroll.Visibility    = canSeePayroll ? Visibility.Visible : Visibility.Collapsed;
            NavGovReports.Visibility = canSeePayroll ? Visibility.Visible : Visibility.Collapsed;

            SecAdmin.Visibility      = isFullAccess ? Visibility.Visible : Visibility.Collapsed;
            NavSettings.Visibility   = isFullAccess ? Visibility.Visible : Visibility.Collapsed;
        }

        // ─── Dashboard Stats ───────────────────────────────────────────────────────

        private async void LoadDashboardStats()
        {
            // Run DB queries off the UI thread so the window doesn't freeze
            var (activeEmployees, cutoffLabel, isClosed, pendingLeaves, estNetPayroll) = await Task.Run<(int, string, bool, int, decimal)>(() =>
            {
                try
                {
                    using var conn = DbConnectionFactory.CreateConnection();
                    conn.Open();

                    // 1. Active employee count
                    int activeEmps = conn.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM Employees WHERE IsActive = TRUE;");

                    // 2. Latest payroll period (most recent by StartDate)
                    var period = conn.QueryFirstOrDefault<dynamic>(@"
                        SELECT Id, PeriodCode, StartDate, EndDate, IsClosed
                        FROM PayrollPeriods
                        ORDER BY StartDate DESC
                        LIMIT 1;");

                    string label   = "No Cutoff Found";
                    bool closed    = false;
                    int? pId       = null;

                    if (period != null)
                    {
                        DateTime start = Convert.ToDateTime(period.StartDate);
                        DateTime end   = Convert.ToDateTime(period.EndDate);
                        closed         = Convert.ToBoolean(period.IsClosed);
                        pId            = Convert.ToInt32(period.Id);

                        label = start.Month == end.Month
                            ? $"{start:MMMM} {start.Day}-{end.Day}, {end.Year}"
                            : $"{start:MMM d} – {end:MMM d, yyyy}";
                    }

                    // 3. Pending leave requests count
                    int pendingCount = 0;
                    try
                    {
                        pendingCount = conn.ExecuteScalar<int>(
                            "SELECT COUNT(*) FROM LeaveApplications WHERE Status = 1;");
                    }
                    catch { /* Fallback if table is not seeded */ }

                    // 4. Estimated net payroll from PayrollRecords for the latest period
                    decimal netPayroll = 0m;
                    if (pId.HasValue)
                    {
                        netPayroll = conn.ExecuteScalar<decimal>(@"
                            SELECT COALESCE(SUM(
                                (BasicPay + OvertimePay + NightDiffPay + HolidayPay + Allowances)
                                - (TardinessDeduction + UndertimeDeduction + AbsenceDeduction
                                   + SssEmployee + PhilHealthEmployee + PagIbigEmployee
                                   + WithholdingTax + OtherDeductions)
                            ), 0)
                            FROM PayrollRecords
                            WHERE PayrollPeriodId = @PeriodId;",
                            new { PeriodId = pId.Value });
                    }

                    return (activeEmps, label, closed, pendingCount, netPayroll);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Dashboard stats error: {ex.Message}");
                    return (0, "DB Unavailable", false, 0, 0m);
                }
            });

            // Update UI on the dispatcher thread
            TxtTotalEmployees.Text = $"{activeEmployees} Active";
            TxtCurrentCutoff.Text  = cutoffLabel;

            if (BadgeCutoffStatus != null && BorderCutoffBadge != null)
            {
                BadgeCutoffStatus.Text = isClosed ? "CLOSED" : "OPEN";
                BorderCutoffBadge.Background = isClosed
                    ? new SolidColorBrush(Color.FromRgb(254, 226, 226))
                    : new SolidColorBrush(Color.FromRgb(220, 252, 231));
                BadgeCutoffStatus.Foreground = isClosed
                    ? new SolidColorBrush(Color.FromRgb(185, 28, 28))
                    : new SolidColorBrush(Color.FromRgb(21, 128, 61));
            }

            TxtPendingLeaves.Text = $"{pendingLeaves} Pending";
            TxtEstNetPayroll.Text = $"₱{estNetPayroll:N2}";
        }

        // ─── Navigation ────────────────────────────────────────────────────────────

        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not RadioButton btn) return;

            string name = btn.Name;
            string title = btn.Content?.ToString()?
                .Replace("📊", "").Replace("👥", "").Replace("📈", "")
                .Replace("⏰", "").Replace("🌴", "")
                .Replace("🗓️", "").Replace("💵", "").Replace("🏛️", "")
                .Replace("⚙️", "").Trim() ?? "";

            TxtHeaderTitle.Text = title;

            switch (name)
            {
                case "NavEmployees":
                    _employeeView ??= new EmployeeView();
                    SetContent(_employeeView);
                    break;

                case "NavHolidays":
                    _holidayView ??= new HolidayView();
                    SetContent(_holidayView);
                    break;

                case "NavAttendance":
                    _attendanceView ??= new AttendanceView();
                    SetContent(_attendanceView);
                    break;

                case "NavLeave":
                    _leaveView ??= new LeaveManagementView();
                    SetContent(_leaveView);
                    break;

                case "NavPayroll":
                    _payrollView ??= new PayrollView();
                    SetContent(_payrollView);
                    break;

                case "NavGovReports":
                    _govReportsView ??= new GovernmentReportsView();
                    SetContent(_govReportsView);
                    break;

                case "NavSettings":
                    _settingsView ??= new SettingsView();
                    SetContent(_settingsView);
                    break;

                case "NavPromotions":
                    _employeeView ??= new EmployeeView();
                    SetContent(_employeeView);
                    break;

                default:
                    ShowDashboard();
                    break;
            }
        }

        private void ShowDashboard()
        {
            TxtHeaderTitle.Text = "Management Dashboard";
            ContentFrame.Content = null;
            DashboardPanel.Visibility = Visibility.Visible;
            LoadDashboardStats();
        }

        private void SetContent(UIElement control)
        {
            DashboardPanel.Visibility = Visibility.Collapsed;
            ContentFrame.Content = control;
        }

        // ─── Header & Quick-action Button Handlers ─────────────────────────────────

        private void BtnRefreshDashboard_Click(object sender, RoutedEventArgs e)
        {
            LoadDashboardStats();
            if (ContentFrame.Content is EmployeeView empView)
            {
                _ = empView.LoadEmployeesAsync();
            }
        }

        private void BtnQuickAddEmployee_Click(object sender, RoutedEventArgs e)
        {
            NavEmployees.IsChecked = true;
            TxtHeaderTitle.Text = "Employee 201 Files";
            _employeeView ??= new EmployeeView();
            SetContent(_employeeView);

            // Ask the employee view to open the Add dialog immediately
            _employeeView.OpenAddEmployeeDialog();
        }

        private void BtnQuickAttendance_Click(object sender, RoutedEventArgs e)
        {
            NavAttendance.IsChecked = true;
            TxtHeaderTitle.Text = "Timekeeping & Bio";
            _attendanceView ??= new AttendanceView();
            SetContent(_attendanceView);
        }

        private void BtnQuickLeave_Click(object sender, RoutedEventArgs e)
        {
            NavLeave.IsChecked = true;
            TxtHeaderTitle.Text = "Leave Management";
            _leaveView ??= new LeaveManagementView();
            SetContent(_leaveView);
        }

        private void BtnQuickProcessPayroll_Click(object sender, RoutedEventArgs e)
        {
            NavPayroll.IsChecked = true;
            TxtHeaderTitle.Text = "Payroll & Payslips";
            _payrollView ??= new PayrollView();
            SetContent(_payrollView);
        }

        private void BtnQuickGovReports_Click(object sender, RoutedEventArgs e)
        {
            NavGovReports.IsChecked = true;
            TxtHeaderTitle.Text = "Statutory Reports";
            _govReportsView ??= new GovernmentReportsView();
            SetContent(_govReportsView);
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to sign out of the Genetian Payroll System?",
                "Confirm Sign Out",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                AuthService.Logout();
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }
    }
}