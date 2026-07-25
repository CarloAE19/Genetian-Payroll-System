using System;
using System.Windows;
using System.Windows.Media;
using GB_Payroll_System.Data;
using GB_Payroll_System.Services;

namespace GB_Payroll_System.Views
{
    public partial class LoginWindow : Window
    {
        private bool _isPasswordShown = false;

        public LoginWindow()
        {
            InitializeComponent();
            CheckServerConnection();
        }

        private void CheckServerConnection()
        {
            TxtEnvironmentInfo.Text = $"ENVIRONMENT: LOCAL DATABASE ({DbConnectionFactory.Server.ToUpper()})";

            bool isConnected = DbConnectionFactory.TestConnection(out string errorMessage);
            if (isConnected)
            {
                StatusDot.Fill = (Brush)Application.Current.Resources["ConnectedGreenBrush"];
                TxtStatusLabel.Text = "CONNECTED";
                TxtStatusLabel.Foreground = (Brush)Application.Current.Resources["ConnectedGreenBrush"];
            }
            else
            {
                StatusDot.Fill = new SolidColorBrush(Color.FromRgb(229, 62, 62)); // Red
                TxtStatusLabel.Text = "DISCONNECTED";
                TxtStatusLabel.Foreground = new SolidColorBrush(Color.FromRgb(229, 62, 62));
            }
        }

        private void BtnSignIn_Click(object sender, RoutedEventArgs e)
        {
            BannerError.Visibility = Visibility.Collapsed;

            string username = TxtUsername.Text.Trim();
            string password = _isPasswordShown ? TxtPasswordUnmasked.Text : TxtPassword.Password;

            var (success, message, user) = AuthService.Authenticate(username, password);

            if (!success || user == null)
            {
                TxtErrorMessage.Text = message;
                BannerError.Visibility = Visibility.Visible;
                return;
            }

            // Open MainWindow
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void BtnTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordShown = !_isPasswordShown;
            if (_isPasswordShown)
            {
                TxtPasswordUnmasked.Text = TxtPassword.Password;
                TxtPassword.Visibility = Visibility.Collapsed;
                TxtPasswordUnmasked.Visibility = Visibility.Visible;
                BtnTogglePassword.Content = "🙈";
            }
            else
            {
                TxtPassword.Password = TxtPasswordUnmasked.Text;
                TxtPasswordUnmasked.Visibility = Visibility.Collapsed;
                TxtPassword.Visibility = Visibility.Visible;
                BtnTogglePassword.Content = "👁️";
            }
        }

        private void TxtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Guard: TxtPasswordUnmasked may still be null during InitializeComponent()
            // when XAML sets the default Password property and fires this event early.
            if (!_isPasswordShown && TxtPasswordUnmasked != null)
            {
                TxtPasswordUnmasked.Text = TxtPassword.Password;
            }
        }

        private void TxtPasswordUnmasked_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_isPasswordShown)
            {
                TxtPassword.Password = TxtPasswordUnmasked.Text;
            }
        }

        private void TxtUsername_GotFocus(object sender, RoutedEventArgs e)
        {
            TxtUsernamePlaceholder.Visibility = Visibility.Collapsed;
        }

        private void TxtUsername_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtUsername.Text))
            {
                TxtUsernamePlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void HyperlinkAdmin_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Please contact your IT / HR Administrator or email admin@genetian.ph for credentials and password resets.",
                "Genetian Payroll Help",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
