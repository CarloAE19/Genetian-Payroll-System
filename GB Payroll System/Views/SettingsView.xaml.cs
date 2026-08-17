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
        private readonly StatutorySettingsRepository _statRepo = new();
        private List<User> _users = [];
        private StatutorySettings _currentStatutory = new();

        public SettingsView()
        {
            InitializeComponent();
            Loaded += (_, _) =>
            {
                // Attach currency auto-formatting
                CurrencyInputHelper.Attach(TxtSssMinCredit);
                CurrencyInputHelper.Attach(TxtSssMaxCredit);
                CurrencyInputHelper.Attach(TxtPhMinCredit);
                CurrencyInputHelper.Attach(TxtPhMaxCredit);
                CurrencyInputHelper.Attach(TxtPagIbigEe);
                CurrencyInputHelper.Attach(TxtPagIbigEr);
                CurrencyInputHelper.Attach(TxtBirSemiExempt);
                CurrencyInputHelper.Attach(TxtBirBonusCap);

                LoadUsers();
                LoadStatutorySettings();
                LoadConnectionSettings();
            };
        }

        // ── TAB SWITCHING ────────────────────────────────────────────────────
        private void Tab_Changed(object sender, RoutedEventArgs e)
        {
            if (UsersPanel == null || StatutoryPanel == null || DatabasePanel == null) return;

            UsersPanel.Visibility     = TabUsers.IsChecked == true     ? Visibility.Visible : Visibility.Collapsed;
            StatutoryPanel.Visibility = TabStatutory.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            DatabasePanel.Visibility  = TabDatabase.IsChecked == true  ? Visibility.Visible : Visibility.Collapsed;
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
        // STATUTORY & CONTRIBUTION RATES TAB
        // ══════════════════════════════════════════════════════════════════════
        private void LoadStatutorySettings()
        {
            if (TxtSssEeRate == null) return;
            try
            {
                _currentStatutory = _statRepo.GetSettings();
                PopulateStatutoryFields(_currentStatutory);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load statutory settings: {ex.Message}");
            }
        }

        private void PopulateStatutoryFields(StatutorySettings s)
        {
            // SSS
            TxtSssEeRate.Text    = s.SssEmployeeSharePercent.ToString("F2");
            TxtSssErRate.Text    = s.SssEmployerSharePercent.ToString("F2");
            TxtSssTotalRate.Text = s.SssTotalRatePercent.ToString("F2");
            TxtSssMinCredit.Text = s.SssMinSalaryCredit.ToString("N2");
            TxtSssMaxCredit.Text = s.SssMaxSalaryCredit.ToString("N2");

            // PhilHealth
            TxtPhTotalRate.Text  = s.PhilHealthTotalRatePercent.ToString("F2");
            TxtPhEeRate.Text     = s.PhilHealthEmployeeSharePercent.ToString("F2");
            TxtPhErRate.Text     = s.PhilHealthEmployerSharePercent.ToString("F2");
            TxtPhMinCredit.Text  = s.PhilHealthMinSalaryCredit.ToString("N2");
            TxtPhMaxCredit.Text  = s.PhilHealthMaxSalaryCredit.ToString("N2");

            // Pag-IBIG
            TxtPagIbigEe.Text    = s.PagIbigEmployeeStandardMonthly.ToString("N2");
            TxtPagIbigEr.Text    = s.PagIbigEmployerStandardMonthly.ToString("N2");

            // BIR Tax
            TxtBirSemiExempt.Text = s.BirSemiMonthlyExemptCeiling.ToString("N2");
            TxtBirBonusCap.Text   = s.BirBonusExemptCap.ToString("N2");
        }

        private void BtnSaveStatutory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var s = new StatutorySettings();

                // Parse SSS
                if (decimal.TryParse(TxtSssEeRate.Text.Replace(",", ""), out decimal sssEe)) s.SssEmployeeSharePercent = sssEe;
                if (decimal.TryParse(TxtSssErRate.Text.Replace(",", ""), out decimal sssEr)) s.SssEmployerSharePercent = sssEr;
                if (decimal.TryParse(TxtSssTotalRate.Text.Replace(",", ""), out decimal sssTot)) s.SssTotalRatePercent = sssTot;
                if (decimal.TryParse(TxtSssMinCredit.Text.Replace(",", ""), out decimal sssMin)) s.SssMinSalaryCredit = sssMin;
                if (decimal.TryParse(TxtSssMaxCredit.Text.Replace(",", ""), out decimal sssMax)) s.SssMaxSalaryCredit = sssMax;

                // Parse PhilHealth
                if (decimal.TryParse(TxtPhTotalRate.Text.Replace(",", ""), out decimal phTot)) s.PhilHealthTotalRatePercent = phTot;
                if (decimal.TryParse(TxtPhEeRate.Text.Replace(",", ""), out decimal phEe)) s.PhilHealthEmployeeSharePercent = phEe;
                if (decimal.TryParse(TxtPhErRate.Text.Replace(",", ""), out decimal phEr)) s.PhilHealthEmployerSharePercent = phEr;
                if (decimal.TryParse(TxtPhMinCredit.Text.Replace(",", ""), out decimal phMin)) s.PhilHealthMinSalaryCredit = phMin;
                if (decimal.TryParse(TxtPhMaxCredit.Text.Replace(",", ""), out decimal phMax)) s.PhilHealthMaxSalaryCredit = phMax;

                // Parse Pag-IBIG
                if (decimal.TryParse(TxtPagIbigEe.Text.Replace(",", ""), out decimal piEe)) s.PagIbigEmployeeStandardMonthly = piEe;
                if (decimal.TryParse(TxtPagIbigEr.Text.Replace(",", ""), out decimal piEr)) s.PagIbigEmployerStandardMonthly = piEr;

                // Parse BIR
                if (decimal.TryParse(TxtBirSemiExempt.Text.Replace(",", ""), out decimal birSemi)) s.BirSemiMonthlyExemptCeiling = birSemi;
                if (decimal.TryParse(TxtBirBonusCap.Text.Replace(",", ""), out decimal birBonus)) s.BirBonusExemptCap = birBonus;

                s.UpdatedByUsername = AuthService.CurrentUser?.Username ?? "admin";

                _statRepo.SaveSettings(s);
                _currentStatutory = s;

                MessageBox.Show("✅ National statutory contribution rates and caps saved and applied successfully!",
                    "Statutory Rates Updated", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save statutory settings:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnResetStatutory_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show(
                "Reset all statutory contribution rates to standard 2025/2026 DOLE schedules (SSS 14%, PhilHealth 5%, Pag-IBIG ₱200, BIR ₱10,417)?",
                "Confirm Reset", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                _currentStatutory = _statRepo.ResetToDefaults(AuthService.CurrentUser?.Username ?? "admin");
                PopulateStatutoryFields(_currentStatutory);
                MessageBox.Show("Statutory rates have been reset to national standard schedules.", "Reset Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to reset statutory settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
