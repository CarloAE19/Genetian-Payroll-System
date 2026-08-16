using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GB_Payroll_System.Data;
using GB_Payroll_System.Models;
using GB_Payroll_System.Services;

namespace GB_Payroll_System.Views
{
    public partial class SettingsView : UserControl
    {
        private readonly UserRepository _userRepo = new();
        private List<User> _users = [];

        public SettingsView()
        {
            InitializeComponent();
            Loaded += (_, _) =>
            {
                LoadUsers();
                LoadConnectionSettings();
            };
        }

        // ── TAB SWITCHING ────────────────────────────────────────────────────
        private void Tab_Changed(object sender, RoutedEventArgs e)
        {
            if (UsersPanel == null) return;
            bool showUsers = TabUsers.IsChecked == true;
            UsersPanel.Visibility    = showUsers ? Visibility.Visible   : Visibility.Collapsed;
            DatabasePanel.Visibility = showUsers ? Visibility.Collapsed : Visibility.Visible;
        }

        // ══════════════════════════════════════════════════════════════════════
        // USER ACCOUNTS TAB
        // ══════════════════════════════════════════════════════════════════════
        private void LoadUsers()
        {
            if (UsersGrid == null) return;
            try { _users = _userRepo.GetAll(); }
            catch
            {
                // Offline sample
                _users =
                [
                    new User { Id = 1, Username = "admin",    FullName = "Genetian Administrator", Email = "admin@genetian.ph",   Role = UserRole.Admin,      IsActive = true,  CreatedAt = DateTime.Now.AddDays(-60) },
                    new User { Id = 2, Username = "hr_user",  FullName = "HR Officer",             Email = "hr@genetian.ph",      Role = UserRole.HR,         IsActive = true,  CreatedAt = DateTime.Now.AddDays(-30) },
                    new User { Id = 3, Username = "acct",     FullName = "Accountant",             Email = "acct@genetian.ph",    Role = UserRole.Accounting,  IsActive = true,  CreatedAt = DateTime.Now.AddDays(-15) },
                    new User { Id = 4, Username = "mgmt",     FullName = "Branch Manager",         Email = "mgmt@genetian.ph",    Role = UserRole.Management,  IsActive = false, CreatedAt = DateTime.Now.AddDays(-45) },
                ];
            }
            UsersGrid.ItemsSource = _users;
            TxtUserCount.Text = $"{_users.Count} user(s)";
        }

        private void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new UserFormDialog(null);
            if (dialog.ShowDialog() == true) LoadUsers();
        }

        private void BtnEditUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var user = _users.Find(u => u.Id == id);
                if (user == null) return;

                // Protect the currently logged-in admin from role changes
                if (user.Username == AuthService.CurrentUser?.Username)
                {
                    MessageBox.Show("You cannot edit your own account from here. Use 'Change Password' instead.",
                        "Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var dialog = new UserFormDialog(user);
                if (dialog.ShowDialog() == true) LoadUsers();
            }
        }

        private void BtnResetPassword_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var user = _users.Find(u => u.Id == id);
                if (user == null) return;
                var dialog = new ResetPasswordDialog(user);
                if (dialog.ShowDialog() == true)
                    MessageBox.Show("Password updated successfully.", "Done", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnToggleUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var user = _users.Find(u => u.Id == id);
                if (user == null) return;

                if (user.Username == AuthService.CurrentUser?.Username)
                {
                    MessageBox.Show("You cannot deactivate your own account.", "Not Allowed",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool newState = !user.IsActive;
                var confirm = MessageBox.Show(
                    $"{(newState ? "Reactivate" : "Deactivate")} user '{user.Username}'?",
                    "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirm != MessageBoxResult.Yes) return;

                try { _userRepo.SetActive(id, newState); LoadUsers(); }
                catch (Exception ex) { MessageBox.Show($"Failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // DATABASE CONNECTION TAB
        // ══════════════════════════════════════════════════════════════════════
        private void LoadConnectionSettings()
        {
            if (TxtHost == null) return;
            TxtHost.Text     = DbConnectionFactory.Server;
            TxtPort.Text     = DbConnectionFactory.Port;
            TxtDatabase.Text = DbConnectionFactory.Database;
            TxtDbUser.Text   = DbConnectionFactory.Username;
            TxtDbPassword.Password = DbConnectionFactory.Password;
        }

        private void BtnTestConnection_Click(object sender, RoutedEventArgs e)
        {
            ApplyConnectionFromFields();
            bool ok = DbConnectionFactory.TestConnection(out string error);
            ShowConnectionBanner(ok, ok ? "Connection successful!" : "Connection failed", ok ? "" : error);
        }

        private void BtnSaveConnection_Click(object sender, RoutedEventArgs e)
        {
            ApplyConnectionFromFields();
            bool ok = DbConnectionFactory.TestConnection(out string error);
            if (ok)
            {
                ShowConnectionBanner(true, "Settings saved and connection verified.", "");
                MessageBox.Show("Database connection settings saved successfully.", "Saved",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                ShowConnectionBanner(false, "Settings saved but connection failed.", error);
                MessageBox.Show($"Settings saved, but could not connect:\n{error}\n\nCheck your server address and credentials.",
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ApplyConnectionFromFields()
        {
            DbConnectionFactory.Server   = TxtHost.Text.Trim();
            DbConnectionFactory.Port     = TxtPort.Text.Trim();
            DbConnectionFactory.Database = TxtDatabase.Text.Trim();
            DbConnectionFactory.Username = TxtDbUser.Text.Trim();
            DbConnectionFactory.Password = TxtDbPassword.Password;
        }

        private void ShowConnectionBanner(bool success, string status, string detail)
        {
            BannerConnection.Visibility = Visibility.Visible;
            BannerConnection.Background = success
                ? new SolidColorBrush(Color.FromRgb(198, 246, 213))
                : new SolidColorBrush(Color.FromRgb(254, 215, 215));
            TxtConnectionIcon.Text   = success ? "✅" : "❌";
            TxtConnectionStatus.Text = status;
            TxtConnectionStatus.Foreground = success
                ? new SolidColorBrush(Color.FromRgb(39, 103, 73))
                : new SolidColorBrush(Color.FromRgb(155, 44, 44));
            TxtConnectionDetail.Text = detail;
            TxtConnectionDetail.Foreground = new SolidColorBrush(Color.FromRgb(113, 128, 150));
        }
    }
}
