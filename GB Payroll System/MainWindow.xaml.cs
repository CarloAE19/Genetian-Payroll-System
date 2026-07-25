using System.Windows;
using System.Windows.Controls;
using GB_Payroll_System.Services;
using GB_Payroll_System.Views;

namespace GB_Payroll_System
{
    public partial class MainWindow : Window
    {
        private EmployeeView? _employeeView;
        private HolidayView? _holidayView;

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
            }
        }

        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not RadioButton btn) return;

            string name = btn.Name;
            string title = btn.Content?.ToString()?
                .Replace("📊 ", "").Replace("👥 ", "").Replace("📈 ", "")
                .Replace("📦 ", "").Replace("⏰ ", "").Replace("🗓️ ", "")
                .Replace("💵 ", "").Replace("⚙️ ", "") ?? "";

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

                case "NavPromotions":
                    // Opens Employee view with promotion column highlighted
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
        }

        private void SetContent(UIElement control)
        {
            DashboardPanel.Visibility = Visibility.Collapsed;
            ContentFrame.Content = control;
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