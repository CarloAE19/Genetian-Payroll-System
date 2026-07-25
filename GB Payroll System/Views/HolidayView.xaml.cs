using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GB_Payroll_System.Data;
using GB_Payroll_System.Models;

namespace GB_Payroll_System.Views
{
    public partial class HolidayView : UserControl
    {
        private readonly HolidayRepository _repo = new();
        private List<Holiday> _allHolidays = [];

        public HolidayView()
        {
            InitializeComponent();
            PopulateYearFilter();
            // Load after Loaded event so all controls are fully initialized first
            Loaded += (_, _) => LoadHolidays();
        }

        private void PopulateYearFilter()
        {
            int currentYear = DateTime.Now.Year;
            for (int y = currentYear + 1; y >= currentYear - 2; y--)
                CboYear.Items.Add(new ComboBoxItem { Content = y.ToString(), Tag = y });

            // Suppress the SelectionChanged event during init by temporarily detaching it
            CboYear.SelectionChanged -= CboYear_SelectionChanged;
            CboYear.SelectedIndex = 1; // default = current year
            CboYear.SelectionChanged += CboYear_SelectionChanged;
        }

        private void LoadHolidays()
        {
            // Guard: ensure controls are ready
            if (HolidayGrid == null) return;

            try
            {
                int year = CboYear.SelectedItem is ComboBoxItem item && item.Tag is int y ? y : DateTime.Now.Year;
                _allHolidays = _repo.GetByYear(year);
            }
            catch
            {
                // Sample data for offline/demo mode
                _allHolidays =
                [
                    new Holiday { Id = 1, Name = "New Year's Day", Date = new DateTime(2026, 1, 1), Type = HolidayType.RegularHoliday, WorkedMultiplier = 2.00m, UnworkedMultiplier = 1.00m, DeclaredBy = "Proclamation No. 1" },
                    new Holiday { Id = 2, Name = "Independence Day", Date = new DateTime(2026, 6, 12), Type = HolidayType.RegularHoliday, WorkedMultiplier = 2.00m, UnworkedMultiplier = 1.00m, DeclaredBy = "Proclamation No. 368" },
                    new Holiday { Id = 3, Name = "Manila Charter Day", Date = new DateTime(2026, 6, 24), Type = HolidayType.LocalSpecialHoliday, WorkedMultiplier = 1.30m, UnworkedMultiplier = 0.00m, DeclaredBy = "Manila City Ordinance" },
                    new Holiday { Id = 4, Name = "National Heroes Day", Date = new DateTime(2026, 8, 31), Type = HolidayType.RegularHoliday, WorkedMultiplier = 2.00m, UnworkedMultiplier = 1.00m, DeclaredBy = "R.A. 9492" },
                    new Holiday { Id = 5, Name = "All Saints Day", Date = new DateTime(2026, 11, 1), Type = HolidayType.SpecialNonWorking, WorkedMultiplier = 1.30m, UnworkedMultiplier = 0.00m, DeclaredBy = "Proclamation No. 368" },
                    new Holiday { Id = 6, Name = "Christmas Day", Date = new DateTime(2026, 12, 25), Type = HolidayType.RegularHoliday, WorkedMultiplier = 2.00m, UnworkedMultiplier = 1.00m, DeclaredBy = "R.A. 9492" },
                ];
            }
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            // Guard: ensure controls are fully initialized before accessing them
            if (HolidayGrid == null || CboTypeFilter == null) return;

            var filtered = _allHolidays.AsEnumerable();
            if (CboTypeFilter.SelectedIndex > 0)
            {
                var type = (HolidayType)CboTypeFilter.SelectedIndex;
                filtered = filtered.Where(h => h.Type == type);
            }
            HolidayGrid.ItemsSource = filtered.OrderBy(h => h.Date).ToList();
        }

        private void CboYear_SelectionChanged(object sender, SelectionChangedEventArgs e) => LoadHolidays();
        private void CboTypeFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilter();

        private void BtnAddHoliday_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new HolidayFormDialog(null);
            if (dialog.ShowDialog() == true) LoadHolidays();
        }

        private void BtnEditHoliday_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var holiday = _allHolidays.FirstOrDefault(h => h.Id == id);
                if (holiday != null)
                {
                    var dialog = new HolidayFormDialog(holiday);
                    if (dialog.ShowDialog() == true) LoadHolidays();
                }
            }
        }

        private void BtnDeleteHoliday_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var holiday = _allHolidays.FirstOrDefault(h => h.Id == id);
                if (holiday == null) return;

                var confirm = MessageBox.Show(
                    $"Remove \"{holiday.Name}\" ({holiday.Date:MMMM dd, yyyy}) from the holiday list?",
                    "Confirm Removal", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (confirm != MessageBoxResult.Yes) return;

                try
                {
                    _repo.Delete(id);
                    LoadHolidays();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to remove holiday: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
