using System;
using System.Windows;
using System.Windows.Controls;
using GB_Payroll_System.Data;
using GB_Payroll_System.Models;

namespace GB_Payroll_System.Views
{
    public partial class UserFormDialog : Window
    {
        private readonly UserRepository _repo = new();
        private User? _existing;

        public UserFormDialog(User? user)
        {
            InitializeComponent();
            _existing = user;

            if (user != null)
            {
                TxtDialogTitle.Text   = "Edit User Account";
                TxtSubtitle.Text      = $"Editing: {user.Username}";
                BtnSave.Content       = "💾  Save Changes";
                PasswordPanel.Visibility = Visibility.Collapsed; // Password changed via separate dialog

                TxtFullName.Text  = user.FullName;
                TxtEmail.Text     = user.Email;
                TxtUsername.Text  = user.Username;
                TxtUsername.IsReadOnly = true; // Username is immutable after creation

                CboRole.SelectedIndex = user.Role switch
                {
                    UserRole.Admin      => 0,
                    UserRole.HR         => 1,
                    UserRole.Accounting => 2,
                    UserRole.Management => 3,
                    _ => 1
                };
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            BannerError.Visibility = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(TxtFullName.Text)) { ShowError("Full Name is required."); return; }
            if (string.IsNullOrWhiteSpace(TxtUsername.Text)) { ShowError("Username is required."); return; }

            UserRole role = (CboRole.SelectedItem as ComboBoxItem)?.Content.ToString() switch
            {
                "Admin"      => UserRole.Admin,
                "HR"         => UserRole.HR,
                "Accounting" => UserRole.Accounting,
                "Management" => UserRole.Management,
                _ => UserRole.HR
            };

            if (_existing == null) // ── ADD mode
            {
                if (string.IsNullOrWhiteSpace(TxtPassword.Password)) { ShowError("Password is required."); return; }
                if (TxtPassword.Password != TxtConfirmPassword.Password) { ShowError("Passwords do not match."); return; }
                if (TxtPassword.Password.Length < 6) { ShowError("Password must be at least 6 characters."); return; }

                try
                {
                    if (_repo.UsernameExists(TxtUsername.Text.Trim()))
                    { ShowError($"Username '{TxtUsername.Text.Trim()}' is already taken."); return; }

                    var newUser = new User
                    {
                        Username     = TxtUsername.Text.Trim().ToLower(),
                        FullName     = TxtFullName.Text.Trim(),
                        Email        = TxtEmail.Text.Trim(),
                        PasswordHash = UserRepository.HashPassword(TxtPassword.Password),
                        Role         = role,
                        IsActive     = true,
                        CreatedAt    = DateTime.UtcNow
                    };
                    _repo.Insert(newUser);
                    DialogResult = true;
                    Close();
                }
                catch (Exception ex) { ShowError($"Save failed: {ex.Message}"); }
            }
            else // ── EDIT mode
            {
                try
                {
                    _existing.FullName = TxtFullName.Text.Trim();
                    _existing.Email    = TxtEmail.Text.Trim();
                    _existing.Role     = role;
                    _repo.Update(_existing);
                    DialogResult = true;
                    Close();
                }
                catch (Exception ex) { ShowError($"Save failed: {ex.Message}"); }
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();

        private void ShowError(string msg) { TxtError.Text = msg; BannerError.Visibility = Visibility.Visible; }
    }
}
