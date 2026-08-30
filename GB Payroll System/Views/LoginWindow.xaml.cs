using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using GB_Payroll_System.Data;
using GB_Payroll_System.Services;

namespace GB_Payroll_System.Views
{
    public partial class LoginWindow : Window
    {
        private bool _isPasswordShown = false;

        // ─── DWM Title Bar Coloring (Windows 11 / Windows 10 fallback) ─────────────

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20; // Win 10 dark caption fallback
        private const int DWMWA_CAPTION_COLOR           = 35; // Win 11: title bar background
        private const int DWMWA_TEXT_COLOR               = 36; // Win 11: title bar text

        /// <summary>Converts "#RRGGBB" to Win32 COLORREF (0x00BBGGRR).</summary>
        private static int ToColorRef(byte r, byte g, byte b) => r | (g << 8) | (b << 16);

        private void ApplyTitleBarColor()
        {
            try
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                if (hwnd == IntPtr.Zero) return;

                // Windows 11 (Build 22000+): exact caption color + text color
                if (Environment.OSVersion.Version.Build >= 22000)
                {
                    int captionColor = ToColorRef(0x0A, 0x4D, 0x9C); // Genetian Blue #0A4D9C
                    DwmSetWindowAttribute(hwnd, DWMWA_CAPTION_COLOR, ref captionColor, sizeof(int));

                    int textColor = ToColorRef(0xFF, 0xFF, 0xFF); // White text
                    DwmSetWindowAttribute(hwnd, DWMWA_TEXT_COLOR, ref textColor, sizeof(int));
                }
                else
                {
                    // Windows 10 fallback: enable dark-mode caption (dark grey is the closest available)
                    int useDark = 1;
                    DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDark, sizeof(int));
                }
            }
            catch
            {
                // Non-critical — silently ignore if DWM is unavailable
            }
        }

        public LoginWindow()
        {
            InitializeComponent();
            // SourceInitialized fires once the native HWND exists — earliest safe point for DWM calls
            SourceInitialized += (_, _) => ApplyTitleBarColor();
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
                "Please contact your IT / HR Administrator for credentials and password resets.",
                "Genetian Payroll Help",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
