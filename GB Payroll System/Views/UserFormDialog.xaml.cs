using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GB_Payroll_System.Data;
using GB_Payroll_System.Models;
using GB_Payroll_System.Services;

namespace GB_Payroll_System.Views
{
    public partial class UserFormDialog : Window
    {
        private readonly UserRepository _repo = new();
        private User? _existing;
        private bool _isEditMode;

        public UserFormDialog(User? user)
        {
            InitializeComponent();
            _existing = user;
            _isEditMode = user != null;

            if (_isEditMode)
            {
                TxtDialogTitle.Text = "👤 Edit User Account";
                TxtSubtitle.Text = $"Configuring login and role permissions for @{user!.Username}";
                BtnSave.Content = "💾  Save Changes";
                PasswordPanel.Visibility = Visibility.Collapsed;
                EditActionsPanel.Visibility = Visibility.Visible;
                PillStatus.Visibility = Visibility.Visible;

                TxtFullName.Text = user.FullName;
                TxtEmail.Text = user.Email;
                TxtUsername.Text = user.Username;
                TxtUsername.IsReadOnly = true; // Username is immutable
                TxtUsername.Background = new SolidColorBrush(Color.FromRgb(241, 245, 249));

                CboRole.SelectedIndex = user.Role switch
                {
                    UserRole.Admin => 0,
                    UserRole.HR => 1,
                    UserRole.Accounting => 2,
                    UserRole.Management => 3,
                    _ => 1
                };

                UpdateStatusUI(user.IsActive);
                TxtAccountMeta.Text = $"Created: {user.CreatedAt:MMM dd, yyyy} | Last Login: {(user.LastLoginAt.HasValue ? user.LastLoginAt.Value.ToString("MMM dd, yyyy hh:mm tt") : "Never")}";
            }
            else
            {
                UpdateRoleSummary("HR");
            }
        }

        private void UpdateStatusUI(bool isActive)
        {
            if (isActive)
            {
                PillStatus.Background = new SolidColorBrush(Color.FromRgb(220, 252, 231));
                PillStatus.BorderBrush = new SolidColorBrush(Color.FromRgb(187, 247, 208));
                TxtStatusBadge.Text = "Active Account";
                TxtStatusBadge.Foreground = new SolidColorBrush(Color.FromRgb(21, 128, 61));

                BtnToggleStatus.Content = "⛔ Deactivate";
                BtnToggleStatus.Background = new SolidColorBrush(Color.FromRgb(254, 226, 226));
                BtnToggleStatus.Foreground = new SolidColorBrush(Color.FromRgb(185, 28, 28));
            }
            else
            {
                PillStatus.Background = new SolidColorBrush(Color.FromRgb(254, 226, 226));
                PillStatus.BorderBrush = new SolidColorBrush(Color.FromRgb(254, 202, 202));
                TxtStatusBadge.Text = "Inactive Account";
                TxtStatusBadge.Foreground = new SolidColorBrush(Color.FromRgb(185, 28, 28));

                BtnToggleStatus.Content = "✅ Reactivate";
                BtnToggleStatus.Background = new SolidColorBrush(Color.FromRgb(220, 252, 231));
                BtnToggleStatus.Foreground = new SolidColorBrush(Color.FromRgb(21, 128, 61));
            }
        }

        private void CboRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TxtRoleSummary == null) return;
            string roleName = (CboRole.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "HR";
            UpdateRoleSummary(roleName);
        }

        private void UpdateRoleSummary(string roleName)
        {
            TxtRoleSummary.Text = roleName switch
            {
                "Admin" => "• Full Access: Complete authority across HRIS 201 files, attendance, payroll runs, government remittances, user management, and system database settings.",
                "HR" => "• Full HR Access: Manage employees, 201 contracts, biometric timekeeping, leaves, salary promotions, and view payroll registers.",
                "Accounting" => "• Accounting Access: Full control over payroll calculations, payslip generation, withholding taxes, statutory remittances, and financial adjustments.",
                "Management" => "• Executive View: Read-only dashboards, employee summaries, attendance registers, and compliance reports.",
                _ => "• Standard access to assigned modules."
            };
        }

        private void BtnQuickResetPassword_Click(object sender, RoutedEventArgs e)
        {
            if (_existing == null) return;
            var dialog = new ResetPasswordDialog(_existing) { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                MessageBox.Show($"Password for '{_existing.Username}' was reset successfully.", "Password Updated", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnToggleStatus_Click(object sender, RoutedEventArgs e)
        {
            if (_existing == null) return;

            if (_existing.Username == AuthService.CurrentUser?.Username)
            {
                MessageBox.Show("You cannot deactivate your own account.", "Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool newState = !_existing.IsActive;
            var confirm = MessageBox.Show(
                $"{(newState ? "Reactivate" : "Deactivate")} account '{_existing.Username}'?",
                "Confirm Account Status", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                _repo.SetActive(_existing.Id, newState);
                _existing.IsActive = newState;
                UpdateStatusUI(newState);
            }
            catch (Exception ex)
            {
                ShowError($"Failed to update status: {ex.Message}");
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            BannerError.Visibility = Visibility.Collapsed;

            string fullName = TxtFullName.Text.Trim();
            string email = TxtEmail.Text.Trim();
            string username = TxtUsername.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(fullName))
            {
                ShowError("Full Name is required.");
                TxtFullName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                ShowError("Username is required.");
                TxtUsername.Focus();
                return;
            }

            UserRole role = (CboRole.SelectedItem as ComboBoxItem)?.Content.ToString() switch
            {
                "Admin" => UserRole.Admin,
                "HR" => UserRole.HR,
                "Accounting" => UserRole.Accounting,
                "Management" => UserRole.Management,
                _ => UserRole.HR
            };

            if (!_isEditMode) // ── ADD mode
            {
                string password = TxtPassword.Password;
                string confirm = TxtConfirmPassword.Password;

                if (string.IsNullOrWhiteSpace(password))
                {
                    ShowError("Password is required for new accounts.");
                    TxtPassword.Focus();
                    return;
                }

                if (password.Length < 6)
                {
                    ShowError("Password must be at least 6 characters long.");
                    TxtPassword.Focus();
                    return;
                }

                if (password != confirm)
                {
                    ShowError("Passwords do not match. Please re-enter.");
                    TxtConfirmPassword.Focus();
                    return;
                }

                try
                {
                    if (_repo.UsernameExists(username))
                    {
                        ShowError($"Username '{username}' is already taken. Please choose another.");
                        TxtUsername.Focus();
                        return;
                    }

                    var newUser = new User
                    {
                        Username = username,
                        FullName = fullName,
                        Email = email,
                        PasswordHash = UserRepository.HashPassword(password),
                        Role = role,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    _repo.Insert(newUser);
                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    ShowError($"Failed to create user account:\n{ex.Message}");
                }
            }
            else // ── EDIT mode
            {
                try
                {
                    _existing!.FullName = fullName;
                    _existing.Email = email;
                    _existing.Role = role;

                    _repo.Update(_existing);
                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    ShowError($"Failed to update user account:\n{ex.Message}");
                }
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
