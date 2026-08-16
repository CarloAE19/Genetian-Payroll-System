using System;
using System.Windows;
using GB_Payroll_System.Data;
using GB_Payroll_System.Models;

namespace GB_Payroll_System.Views
{
    public partial class ResetPasswordDialog : Window
    {
        private readonly UserRepository _repo = new();
        private readonly User _user;

        public ResetPasswordDialog(User user)
        {
            InitializeComponent();
            _user = user;
            TxtSubtitle.Text = $"Changing password for: {user.FullName} ({user.Username})";
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            BannerError.Visibility = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(TxtNewPassword.Password))
            { ShowError("New password cannot be empty."); return; }

            if (TxtNewPassword.Password.Length < 6)
            { ShowError("Password must be at least 6 characters long."); return; }

            if (TxtNewPassword.Password != TxtConfirm.Password)
            { ShowError("Passwords do not match."); return; }

            try
            {
                string hash = UserRepository.HashPassword(TxtNewPassword.Password);
                _repo.ChangePassword(_user.Id, hash);
                DialogResult = true;
                Close();
            }
            catch (Exception ex) { ShowError($"Failed: {ex.Message}"); }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();

        private void ShowError(string msg) { TxtError.Text = msg; BannerError.Visibility = Visibility.Visible; }
    }
}
