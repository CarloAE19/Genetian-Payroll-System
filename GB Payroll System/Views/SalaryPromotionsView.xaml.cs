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
    public partial class SalaryPromotionsView : UserControl
    {
        private readonly SalaryPromotionRepository _repo = new();
        private List<SalaryPromotionHistory> _allPromotions = [];
        private bool _isLoading = false;

        public SalaryPromotionsView()
        {
            InitializeComponent();
            Loaded += async (s, e) => await LoadPromotionsAsync();
        }

        public async Task LoadPromotionsAsync()
        {
            if (_isLoading) return;
            _isLoading = true;
            PanelLoading.Visibility = Visibility.Visible;

            try
            {
                _allPromotions = await Task.Run(() => _repo.GetAllWithEmployeeDetails());
                UpdateKpiCards();
                PopulateDepartmentFilter();
                ApplyFilter();
            }
            catch (Exception)
            {
                // Offline fallback mock promotions
                _allPromotions =
                [
                    new SalaryPromotionHistory
                    {
                        Id = 1,
                        EmployeeId = 1,
                        EmployeeCode = "EMP-2026-001",
                        EmployeeFullName = "Juan Dela Cruz",
                        Department = "Construction",
                        PreviousPosition = "Lead Welder",
                        NewPosition = "Site Foreman",
                        PreviousRate = 550m,
                        NewRate = 610m,
                        EffectiveDate = new DateTime(2025, 1, 15),
                        Reason = "Annual appraisal and promotion to site supervisor role",
                        ApprovedByUsername = "admin"
                    },
                    new SalaryPromotionHistory
                    {
                        Id = 2,
                        EmployeeId = 2,
                        EmployeeCode = "EMP-2026-002",
                        EmployeeFullName = "Maria Santos",
                        Department = "HR",
                        PreviousPosition = "HR Assistant",
                        NewPosition = "HR Officer",
                        PreviousRate = 20000m,
                        NewRate = 25000m,
                        EffectiveDate = new DateTime(2024, 6, 1),
                        Reason = "Regularization and promotion to Lead HR Officer",
                        ApprovedByUsername = "admin"
                    }
                ];
                UpdateKpiCards();
                PopulateDepartmentFilter();
                ApplyFilter();
                TxtStatusBar.Text = $"Offline mode — {_allPromotions.Count} sample promotion records loaded.";
            }
            finally
            {
                _isLoading = false;
                PanelLoading.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateKpiCards()
        {
            TxtTotalPromotionsKpi.Text = _allPromotions.Count.ToString();
            
            decimal avgIncrease = _allPromotions.Count > 0 
                ? _allPromotions.Average(p => p.IncreasePercentage) 
                : 0m;
            TxtAvgIncreaseKpi.Text = avgIncrease >= 0 ? $"+{avgIncrease:F1}%" : $"{avgIncrease:F1}%";

            decimal totalIncrement = _allPromotions.Sum(p => p.RateIncreaseAmount);
            TxtTotalIncrementKpi.Text = $"₱{totalIncrement:N2}";

            var latest = _allPromotions.OrderByDescending(p => p.EffectiveDate).FirstOrDefault();
            TxtLatestDateKpi.Text = latest != null ? latest.EffectiveDate.ToString("MMM dd, yyyy") : "None";
        }

        private void PopulateDepartmentFilter()
        {
            string currentSelection = (CboDepartmentFilter.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All Departments";
            var departments = _allPromotions
                .Select(p => p.Department)
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

            var query = _allPromotions.AsEnumerable();

            if (!string.IsNullOrEmpty(selectedDept) && selectedDept != "All Departments")
            {
                query = query.Where(p => string.Equals(p.Department, selectedDept, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p =>
                    p.EmployeeFullName.ToLower().Contains(search) ||
                    p.EmployeeCode.ToLower().Contains(search) ||
                    p.Department.ToLower().Contains(search) ||
                    p.NewPosition.ToLower().Contains(search) ||
                    p.PreviousPosition.ToLower().Contains(search) ||
                    p.ApprovedByUsername.ToLower().Contains(search));
            }

            var filtered = query.ToList();
            PromotionsGrid.ItemsSource = filtered;

            PanelEmptyState.Visibility = (filtered.Count == 0 && !_isLoading) ? Visibility.Visible : Visibility.Collapsed;
            TxtTotalCount.Text = $"Total Records: {_allPromotions.Count}";
            TxtStatusBar.Text = $"{filtered.Count} salary promotion record(s) matching current criteria.";
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

        private void BtnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            TxtSearch.Clear();
            if (CboDepartmentFilter.Items.Count > 0) CboDepartmentFilter.SelectedIndex = 0;
            ApplyFilter();
        }

        private async void BtnAddPromotion_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SalaryPromotionDialog(null);
            if (dialog.ShowDialog() == true)
            {
                await LoadPromotionsAsync();
            }
        }
    }
}
