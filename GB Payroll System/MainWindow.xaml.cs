using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Dapper;
using GB_Payroll_System.Data;
using GB_Payroll_System.Models;
using GB_Payroll_System.Services;
using GB_Payroll_System.Views;

namespace GB_Payroll_System
{
    public partial class MainWindow : Window
    {
        private EmployeeView?   _employeeView;
        private HolidayView?    _holidayView;
        private AttendanceView? _attendanceView;
        private PayrollView?            _payrollView;
        private SettingsView?           _settingsView;
        private GovernmentReportsView?  _govReportsView;
        private LeaveManagementView?    _leaveView;

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

            NavDashboard.Visibility  = Visibility.Visible;
            NavEmployees.Visibility  = (isFullAccess || role == UserRole.Accounting || role == UserRole.Management) ? Visibility.Visible : Visibility.Collapsed;
            NavPromotions.Visibility = isFullAccess ? Visibility.Visible : Visibility.Collapsed;
            NavAttendance.Visibility = (isFullAccess || role == UserRole.Accounting || role == UserRole.Management) ? Visibility.Visible : Visibility.Collapsed;
            NavLeave.Visibility      = (isFullAccess || role == UserRole.Accounting || role == UserRole.Management) ? Visibility.Visible : Visibility.Collapsed;
            NavHolidays.Visibility   = (isFullAccess || role == UserRole.Management) ? Visibility.Visible : Visibility.Collapsed;
            NavPayroll.Visibility    = (isFullAccess || role == UserRole.Accounting || role == UserRole.Management) ? Visibility.Visible : Visibility.Collapsed;
            NavGovReports.Visibility = (isFullAccess || role == UserRole.Accounting || role == UserRole.Management) ? Visibility.Visible : Visibility.Collapsed;
            NavSettings.Visibility   = isFullAccess ? Visibility.Visible : Visibility.Collapsed;
        }

        // ─── Dashboard Stats ───────────────────────────────────────────────────────

        private async void LoadDashboardStats()
        {
            // Run DB queries off the UI thread so the window doesn't freeze
            var (activeEmployees, cutoffLabel, estNetPayroll) = await Task.Run<(int, string, decimal)>(() =>
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
                    int?   pId     = null;

                    if (period != null)
                    {
                        DateTime start = Convert.ToDateTime(period.StartDate);
                        DateTime end   = Convert.ToDateTime(period.EndDate);
                        bool closed    = Convert.ToBoolean(period.IsClosed);
                        pId            = Convert.ToInt32(period.Id);

                        label = start.Month == end.Month
                            ? $"{start:MMMM} {start.Day}-{end.Day}, {end.Year}"
                            : $"{start:MMM d} – {end:MMM d, yyyy}";

                        if (closed) label += " (Closed)";
                    }

                    // 3. Estimated net payroll from PayrollRecords for the latest period
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

                    return (activeEmps, label, netPayroll);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Dashboard stats error: {ex.Message}");
                    return (0, "DB Unavailable", 0m);
                }
            });

            // Update UI on the dispatcher thread
            TxtTotalEmployees.Text = $"{activeEmployees} Active";
            TxtCurrentCutoff.Text  = cutoffLabel;
            TxtEstNetPayroll.Text  = $"₱{estNetPayroll:N2}";
        }

        // ─── Navigation ────────────────────────────────────────────────────────────

        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not RadioButton btn) return;

            string name = btn.Name;
            string title = btn.Content?.ToString()?
                .Replace("📊 ", "").Replace("👥 ", "").Replace("📈 ", "")
                .Replace("⏰ ", "").Replace("🌴 ", "")
                .Replace("🗓️ ", "").Replace("💵 ", "").Replace("🏛️ ", "")
                .Replace("⚙️ ", "") ?? "";

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

        // ─── Quick-action Button Handlers ──────────────────────────────────────────

        private void BtnQuickAddEmployee_Click(object sender, RoutedEventArgs e)
        {
            NavEmployees.IsChecked = true;
            TxtHeaderTitle.Text = "Employee 201 Files";
            _employeeView ??= new EmployeeView();
            SetContent(_employeeView);

            // Ask the employee view to open the Add dialog immediately
            _employeeView.OpenAddEmployeeDialog();
        }

        private void BtnQuickProcessPayroll_Click(object sender, RoutedEventArgs e)
        {
            NavPayroll.IsChecked = true;
            TxtHeaderTitle.Text = "Payroll & Payslips";
            _payrollView ??= new PayrollView();
            SetContent(_payrollView);
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            AuthService.Logout();
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}