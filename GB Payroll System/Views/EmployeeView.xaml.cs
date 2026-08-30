using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private bool _isLoading = false;

        public EmployeeView()
        {
            InitializeComponent();
            Loaded += async (s, e) => await LoadEmployeesAsync();
        }

        public async Task LoadEmployeesAsync()
        {
            if (_isLoading) return;
            _isLoading = true;
            PanelLoading.Visibility = Visibility.Visible;

            try
            {
                // Fetch all employees from database asynchronously
                _allEmployees = await Task.Run(() => _repo.GetAll(activeOnly: false));
                PopulateDepartmentFilter();
                ApplyFilter();
            }
            catch (Exception)
            {
                // Offline fallback data
                _allEmployees =
                [
                    new Employee { Id = 1, EmployeeCode = "EMP-2026-001", FirstName = "Juan", LastName = "Dela Cruz", Department = "Construction", Position = "Foreman", ContractType = ContractType.Regular, PayType = PayType.Daily, BasicRate = 610m, DateHired = new DateTime(2024, 1, 15), IsActive = true },
                    new Employee { Id = 2, EmployeeCode = "EMP-2026-002", FirstName = "Maria", LastName = "Santos", Department = "HR", Position = "HR Officer", ContractType = ContractType.Regular, PayType = PayType.Monthly, BasicRate = 25000m, DateHired = new DateTime(2023, 6, 1), IsActive = true },
                    new Employee { Id = 3, EmployeeCode = "EMP-2026-003", FirstName = "Pedro", LastName = "Reyes", Department = "Accounting", Position = "Accountant", ContractType = ContractType.Probationary, PayType = PayType.Monthly, BasicRate = 28000m, DateHired = new DateTime(2025, 3, 10), IsActive = true },
                ];
                PopulateDepartmentFilter();
                ApplyFilter();
                TxtStatusBar.Text = $"Offline mode — {_allEmployees.Count} sample records loaded.";
            }
            finally
            {
                _isLoading = false;
                PanelLoading.Visibility = Visibility.Collapsed;
            }
        }

        private void PopulateDepartmentFilter()
        {
            string currentSelection = (CboDepartmentFilter.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All Departments";
            var departments = _allEmployees
                .Select(e => e.Department)
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            CboDepartmentFilter.Items.Clear();
            var allItem = new ComboBoxItem { Content = "All Departments", IsSelected = true };
            CboDepartmentFilter.Items.Add(allItem);

            foreach (var dept in departments)
            {
                var item = new ComboBoxItem { Content = dept };
                if (dept == currentSelection) item.IsSelected = true;
                CboDepartmentFilter.Items.Add(item);
            }
        }

        private void ApplyFilter()
        {
            string search = TxtSearch.Text.Trim().ToLower();
            string selectedDept = (CboDepartmentFilter?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All Departments";
            int statusIndex = CboStatusFilter?.SelectedIndex ?? 0; // 0: Active Only, 1: All, 2: Inactive Only

            var query = _allEmployees.AsEnumerable();

            // Status filter
            if (statusIndex == 0) query = query.Where(e => e.IsActive);
            else if (statusIndex == 2) query = query.Where(e => !e.IsActive);

            // Department filter
            if (!string.IsNullOrEmpty(selectedDept) && selectedDept != "All Departments")
            {
                query = query.Where(e => string.Equals(e.Department, selectedDept, StringComparison.OrdinalIgnoreCase));
            }

            // Search query
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(e =>
                    e.FullName.ToLower().Contains(search) ||
                    e.EmployeeCode.ToLower().Contains(search) ||
                    e.Department.ToLower().Contains(search) ||
                    e.Position.ToLower().Contains(search));
            }

            var filtered = query.ToList();
            EmployeeGrid.ItemsSource = filtered;

            // Empty state handling
            PanelEmptyState.Visibility = (filtered.Count == 0 && !_isLoading) ? Visibility.Visible : Visibility.Collapsed;

            // Update status bar
            int activeCount = _allEmployees.Count(e => e.IsActive);
            TxtActiveCount.Text = $"Active: {activeCount}";
            TxtTotalCount.Text = $"Total: {_allEmployees.Count}";
            TxtStatusBar.Text = $"{filtered.Count} employee record(s) matching current criteria.";
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            PlaceholderSearch.Visibility = string.IsNullOrWhiteSpace(TxtSearch.Text)
                ? Visibility.Visible : Visibility.Collapsed;
            ApplyFilter();
        }

        private void CboDepartmentFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded) ApplyFilter();
        }

        private void CboStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded) ApplyFilter();
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadEmployeesAsync();
        }

        private void BtnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            TxtSearch.Clear();
            if (CboDepartmentFilter.Items.Count > 0) CboDepartmentFilter.SelectedIndex = 0;
            if (CboStatusFilter.Items.Count > 0) CboStatusFilter.SelectedIndex = 0;
            ApplyFilter();
        }

        private async void BtnAddEmployee_Click(object sender, RoutedEventArgs e)
        {
            await OpenAddEmployeeDialogAsync();
        }

        /// <summary>Called by MainWindow's dashboard quick-action button or local Add button.</summary>
        public async Task OpenAddEmployeeDialogAsync()
        {
            var dialog = new EmployeeFormDialog(null);
            if (dialog.ShowDialog() == true)
            {
                await LoadEmployeesAsync();
            }
        }

        public void OpenAddEmployeeDialog()
        {
            _ = OpenAddEmployeeDialogAsync();
        }

        private void EmployeeGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private async void EmployeeGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (EmployeeGrid.SelectedItem is Employee emp)
            {
                var dialog = new EmployeeFormDialog(emp);
                if (dialog.ShowDialog() == true)
                {
                    await LoadEmployeesAsync();
                }
            }
        }

        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var emp = _allEmployees.FirstOrDefault(x => x.Id == id);
                if (emp != null)
                {
                    var dialog = new EmployeeFormDialog(emp);
                    if (dialog.ShowDialog() == true)
                    {
                        await LoadEmployeesAsync();
                    }
                }
            }
        }

        private async void BtnPromote_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var emp = _allEmployees.FirstOrDefault(x => x.Id == id);
                if (emp != null)
                {
                    var dialog = new SalaryPromotionDialog(emp);
                    if (dialog.ShowDialog() == true)
                    {
                        await LoadEmployeesAsync();
                    }
                }
            }
        }

        private async void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var emp = _allEmployees.FirstOrDefault(x => x.Id == id);
                if (emp != null)
                {
                    var dialog = new EmploymentHistoryDialog(emp);
                    if (dialog.ShowDialog() == true)
                    {
                        await LoadEmployeesAsync();
                    }
                }
            }
        }
    }
}
