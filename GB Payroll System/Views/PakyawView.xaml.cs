using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GB_Payroll_System.Data;
using GB_Payroll_System.Models;

namespace GB_Payroll_System.Views
{
    public partial class PakyawView : UserControl
    {
        private readonly PakyawRateRepository  _rateRepo  = new();
        private readonly PakyawEntryRepository _entryRepo = new();

        private List<PakyawRate>           _catalog = [];
        private List<PakyawEntryViewModel> _entries = [];
        private bool _showingCatalog = true;

        public PakyawView()
        {
            InitializeComponent();
            Loaded += (_, _) => InitDefaults();
        }

        private void InitDefaults()
        {
            var today = DateTime.Today;
            bool isSecondHalf = today.Day > 15;
            DpFrom.SelectedDate = isSecondHalf
                ? new DateTime(today.Year, today.Month, 16)
                : new DateTime(today.Year, today.Month, 1);
            DpTo.SelectedDate = isSecondHalf
                ? new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month))
                : new DateTime(today.Year, today.Month, 15);

            LoadCatalog();
        }

        // ── TAB SWITCHING ────────────────────────────────────────────────────
        private void Tab_Changed(object sender, RoutedEventArgs e)
        {
            if (CatalogGrid == null) return;
            _showingCatalog = TabCatalog.IsChecked == true;

            CatalogGrid.Visibility   = _showingCatalog ? Visibility.Visible   : Visibility.Collapsed;
            OutputGrid.Visibility    = _showingCatalog ? Visibility.Collapsed  : Visibility.Visible;
            DateRangePanel.Visibility = _showingCatalog ? Visibility.Collapsed : Visibility.Visible;

            BtnAdd.Content = _showingCatalog ? "➕  Add Task" : "➕  Log Output";

            if (_showingCatalog) LoadCatalog();
            else LoadEntries();
        }

        // ── CATALOG ──────────────────────────────────────────────────────────
        private void LoadCatalog()
        {
            if (CatalogGrid == null) return;
            try { _catalog = _rateRepo.GetAll(activeOnly: false); }
            catch
            {
                _catalog =
                [
                    new PakyawRate { Id = 1, TaskCode = "TASK-PACK-01", TaskName = "Box Packing",         UnitOfMeasure = "Box",   RatePerUnit = 25.00m, IsActive = true },
                    new PakyawRate { Id = 2, TaskCode = "TASK-SORT-01", TaskName = "Sorting (per batch)",  UnitOfMeasure = "Batch", RatePerUnit = 50.00m, IsActive = true },
                    new PakyawRate { Id = 3, TaskCode = "TASK-WELD-01", TaskName = "Welding (per meter)",  UnitOfMeasure = "Meter", RatePerUnit = 80.00m, IsActive = true },
                    new PakyawRate { Id = 4, TaskCode = "TASK-TILE-01", TaskName = "Tile Setting (sq.m.)", UnitOfMeasure = "SqM",   RatePerUnit = 120.00m, IsActive = false },
                ];
            }
            CatalogGrid.ItemsSource = _catalog;
            TxtStatusBar.Text = $"{_catalog.Count(r => r.IsActive)} active task(s)  |  {_catalog.Count(r => !r.IsActive)} inactive";
            TxtTotalEarnings.Text = "";
        }

        private void CatalogGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (CatalogGrid.SelectedItem is PakyawRate rate)
                OpenTaskForm(rate);
        }

        private void BtnEditTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var rate = _catalog.FirstOrDefault(r => r.Id == id);
                if (rate != null) OpenTaskForm(rate);
            }
        }

        private void OpenTaskForm(PakyawRate? rate)
        {
            var dialog = new PakyawTaskFormDialog(rate);
            if (dialog.ShowDialog() == true) LoadCatalog();
        }

        // ── OUTPUT ENTRIES ───────────────────────────────────────────────────
        private void LoadEntries()
        {
            if (OutputGrid == null || DpFrom.SelectedDate == null || DpTo.SelectedDate == null) return;
            try { _entries = _entryRepo.GetByDateRange(DpFrom.SelectedDate.Value, DpTo.SelectedDate.Value); }
            catch
            {
                _entries =
                [
                    new PakyawEntryViewModel { Id = 1, EmployeeCode = "EMP-2026-001", FullName = "Juan Dela Cruz", Department = "Construction", TaskCode = "TASK-WELD-01", TaskName = "Welding (per meter)", UnitOfMeasure = "Meter", WorkDate = DateTime.Today.AddDays(-1), QuantityCompleted = 12, UnitRate = 80m, RecordedByUsername = "supervisor" },
                    new PakyawEntryViewModel { Id = 2, EmployeeCode = "EMP-2026-001", FullName = "Juan Dela Cruz", Department = "Construction", TaskCode = "TASK-WELD-01", TaskName = "Welding (per meter)", UnitOfMeasure = "Meter", WorkDate = DateTime.Today, QuantityCompleted = 15, UnitRate = 80m, RecordedByUsername = "supervisor" },
                    new PakyawEntryViewModel { Id = 3, EmployeeCode = "EMP-2026-002", FullName = "Maria Santos",   Department = "Packaging",     TaskCode = "TASK-PACK-01", TaskName = "Box Packing", UnitOfMeasure = "Box",  WorkDate = DateTime.Today, QuantityCompleted = 48, UnitRate = 25m, RecordedByUsername = "admin" },
                ];
            }
            OutputGrid.ItemsSource = _entries;
            decimal total = _entries.Sum(e => e.TotalEarnings);
            TxtStatusBar.Text = $"{_entries.Count} output record(s)";
            TxtTotalEarnings.Text = $"Total Pakyaw Earnings: ₱{total:N2}";
        }

        private void OutputFilter_Changed(object sender, SelectionChangedEventArgs e) => LoadEntries();

        private void BtnDeleteEntry_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var entry = _entries.FirstOrDefault(x => x.Id == id);
                if (entry == null) return;
                var confirm = MessageBox.Show(
                    $"Delete output entry for {entry.FullName} — {entry.WorkDate:MMM dd}?",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirm != MessageBoxResult.Yes) return;
                try { _entryRepo.Delete(id); LoadEntries(); }
                catch (Exception ex) { MessageBox.Show($"Delete failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        // ── ADD BUTTON (context-aware) ───────────────────────────────────────
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (_showingCatalog)
            {
                OpenTaskForm(null);
            }
            else
            {
                var dialog = new PakyawOutputEntryDialog(_catalog.Where(r => r.IsActive).ToList());
                if (dialog.ShowDialog() == true) LoadEntries();
            }
        }

        // Also called from the inline "📝" button on the catalog grid
        private void BtnLogOutput_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int rateId)
            {
                var rate = _catalog.FirstOrDefault(r => r.Id == rateId);
                var dialog = new PakyawOutputEntryDialog(_catalog.Where(r => r.IsActive).ToList(), preselectedRateId: rateId);
                if (dialog.ShowDialog() == true)
                {
                    TabOutput.IsChecked = true; // switch to output tab
                    LoadEntries();
                }
            }
        }
    }
}
