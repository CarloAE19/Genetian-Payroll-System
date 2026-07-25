using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GB_Payroll_System.Data;
using GB_Payroll_System.Models;

namespace GB_Payroll_System.Views
{
    public partial class EmployeeView : UserControl
    {
        private readonly EmployeeRepository _repo = new();
        private List<Employee> _allEmployees = [];
        private bool _showInactive = false;

        public EmployeeView()
        {
            InitializeComponent();
            LoadEmployees();
        }

        private void LoadEmployees()
        {
            try
            {
                _allEmployees = _repo.GetAll(activeOnly: !_showInactive);
                ApplyFilter();
            }
            catch (Exception)
            {
                // Show sample data offline when database not available
                _allEmployees =
                [
                    new Employee { Id = 1, EmployeeCode = "EMP-2026-001", FirstName = "Juan", LastName = "Dela Cruz", Department = "Construction", Position = "Foreman", PayType = PayType.Daily, BasicRate = 610m, DateHired = new DateTime(2024,1,15), IsActive = true },
                    new Employee { Id = 2, EmployeeCode = "EMP-2026-002", FirstName = "Maria", LastName = "Santos",  Department = "HR",           Position = "HR Officer", PayType = PayType.Monthly, BasicRate = 25000m, DateHired = new DateTime(2023,6,1), IsActive = true },
                    new Employee { Id = 3, EmployeeCode = "EMP-2026-003", FirstName = "Pedro", LastName = "Reyes",   Department = "Accounting",    Position = "Accountant", PayType = PayType.Monthly, BasicRate = 28000m, DateHired = new DateTime(2022,3,10), IsActive = true },
                ];
                ApplyFilter();
                TxtStatusBar.Text = $"Offline mode - {_allEmployees.Count} sample records shown.";
            }
        }

        private void ApplyFilter()
        {
            string search = TxtSearch.Text.Trim().ToLower();
            var filtered = string.IsNullOrEmpty(search)
                ? _allEmployees
                : _allEmployees.Where(e =>
                    e.FullName.ToLower().Contains(search) ||
                    e.EmployeeCode.ToLower().Contains(search) ||
                    e.Department.ToLower().Contains(search) ||
                    e.Position.ToLower().Contains(search)).ToList();

            EmployeeGrid.ItemsSource = filtered;
            TxtStatusBar.Text = $"{filtered.Count} employee(s) loaded.";
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            PlaceholderSearch.Visibility = string.IsNullOrWhiteSpace(TxtSearch.Text)
                ? Visibility.Visible : Visibility.Collapsed;
            ApplyFilter();
        }

        private void BtnShowInactive_Click(object sender, RoutedEventArgs e)
        {
            _showInactive = !_showInactive;
            BtnShowInactive.Content = _showInactive ? "Hide Inactive" : "Show Inactive";
            LoadEmployees();
        }

        private void BtnAddEmployee_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new EmployeeFormDialog(null);
            if (dialog.ShowDialog() == true)
                LoadEmployees();
        }

        private void EmployeeGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void EmployeeGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (EmployeeGrid.SelectedItem is Employee emp)
            {
                var dialog = new EmployeeFormDialog(emp);
                if (dialog.ShowDialog() == true)
                    LoadEmployees();
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var emp = _allEmployees.FirstOrDefault(x => x.Id == id);
                if (emp != null)
                {
                    var dialog = new EmployeeFormDialog(emp);
                    if (dialog.ShowDialog() == true)
                        LoadEmployees();
                }
            }
        }

        private void BtnPromote_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var emp = _allEmployees.FirstOrDefault(x => x.Id == id);
                if (emp != null)
                {
                    var dialog = new SalaryPromotionDialog(emp);
                    if (dialog.ShowDialog() == true)
                        LoadEmployees();
                }
            }
        }
    }
}
