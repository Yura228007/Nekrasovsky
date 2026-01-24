using NekrasovskyAPP.ViewModels;
using NekrasovskyAPP.Services;
using NekrasovskyAPP.Models;

namespace NekrasovskyAPP.Pages
{
    public partial class LogsPage : ContentPage
    {
        private readonly MainViewModel _viewModel;
        private readonly IApiService _apiService;

        public LogsPage(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _apiService = viewModel.GetType().GetField("_apiService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(viewModel) as IApiService;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadLogsAsync();
        }

        private async Task LoadLogsAsync()
        {
            await _viewModel.LoadLogsAsync(_viewModel.CurrentPage);
            UpdatePaginationButtons();
        }

        private void UpdatePaginationButtons()
        {
            PreviousButton.IsEnabled = _viewModel.CurrentPage > 1;
            NextButton.IsEnabled = _viewModel.CurrentPage < _viewModel.TotalPages;
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            var searchText = SearchEntry.Text?.Trim();
            if (string.IsNullOrEmpty(searchText))
            {
                await LoadLogsAsync();
                return;
            }

            // Поиск по URL
            try
            {
                _viewModel.IsLoading = true;
                _viewModel.ErrorMessage = string.Empty;

                var result = await _apiService.AdvancedSearchLogsAsync(
                    url: searchText,
                    pageNumber: 1,
                    pageSize: 50);

                if (result != null)
                {
                    _viewModel.Logs.Clear();
                    foreach (var log in result.Logs)
                    {
                        _viewModel.Logs.Add(log);
                    }

                    _viewModel.CurrentPage = result.PageNumber;
                    _viewModel.TotalPages = result.TotalPages;
                    _viewModel.TotalLogsCount = result.TotalCount;
                }

                UpdatePaginationButtons();
            }
            catch (Exception ex)
            {
                _viewModel.ErrorMessage = $"Ошибка поиска: {ex.Message}";
                await DisplayAlert("Ошибка", _viewModel.ErrorMessage, "OK");
            }
            finally
            {
                _viewModel.IsLoading = false;
            }
        }

        private async void OnApplyFiltersClicked(object sender, EventArgs e)
        {
            try
            {
                // Parse filter values
                int? userId = null;
                if (!string.IsNullOrWhiteSpace(UserIdEntry.Text) && int.TryParse(UserIdEntry.Text, out int userIdVal))
                {
                    userId = userIdVal;
                }

                int? warehouseId = null;
                if (!string.IsNullOrWhiteSpace(WarehouseIdEntry.Text) && int.TryParse(WarehouseIdEntry.Text, out int warehouseIdVal))
                {
                    warehouseId = warehouseIdVal;
                }

                int? materialId = null;
                if (!string.IsNullOrWhiteSpace(MaterialIdEntry.Text) && int.TryParse(MaterialIdEntry.Text, out int materialIdVal))
                {
                    materialId = materialIdVal;
                }

                int? productId = null;
                if (!string.IsNullOrWhiteSpace(ProductIdEntry.Text) && int.TryParse(ProductIdEntry.Text, out int productIdVal))
                {
                    productId = productIdVal;
                }

                DateTime? startDate = StartDatePicker.Date != DateTime.Today ? StartDatePicker.Date : null;
                DateTime? endDate = EndDatePicker.Date != DateTime.Today ? EndDatePicker.Date : null;

                // Set filters in ViewModel
                _viewModel.SelectedUserIdFilter = userId;
                _viewModel.SelectedWarehouseIdFilter = warehouseId;
                _viewModel.SelectedMaterialIdFilter = materialId;
                _viewModel.SelectedProductIdFilter = productId;
                _viewModel.StartDateFilter = startDate;
                _viewModel.EndDateFilter = endDate;

                // Load logs with filters
                await _viewModel.SearchLogsAsync();
                UpdatePaginationButtons();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка применения фильтров: {ex.Message}", "OK");
            }
        }

        private async void OnResetFiltersClicked(object sender, EventArgs e)
        {
            // Reset UI
            UserIdEntry.Text = string.Empty;
            WarehouseIdEntry.Text = string.Empty;
            MaterialIdEntry.Text = string.Empty;
            ProductIdEntry.Text = string.Empty;
            StartDatePicker.Date = DateTime.Today;
            EndDatePicker.Date = DateTime.Today;
            SearchEntry.Text = string.Empty;

            // Reset ViewModel filters
            _viewModel.SelectedUserIdFilter = null;
            _viewModel.SelectedWarehouseIdFilter = null;
            _viewModel.SelectedMaterialIdFilter = null;
            _viewModel.SelectedProductIdFilter = null;
            _viewModel.StartDateFilter = null;
            _viewModel.EndDateFilter = null;

            // Reload logs
            await LoadLogsAsync();
        }

        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            await LoadLogsAsync();
        }

        private async void OnPreviousPageClicked(object sender, EventArgs e)
        {
            if (_viewModel.CurrentPage > 1)
            {
                await _viewModel.LoadLogsAsync(_viewModel.CurrentPage - 1);
                UpdatePaginationButtons();
            }
        }

        private async void OnNextPageClicked(object sender, EventArgs e)
        {
            if (_viewModel.CurrentPage < _viewModel.TotalPages)
            {
                await _viewModel.LoadLogsAsync(_viewModel.CurrentPage + 1);
                UpdatePaginationButtons();
            }
        }

        private async void OnLogSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is RequestLog selectedLog)
            {
                var details = $"ID: {selectedLog.Id}\n" +
                              $"URL: {selectedLog.Url}\n" +
                              $"Метод: {selectedLog.HttpMethod}\n" +
                              $"Статус: {selectedLog.StatusCode}\n" +
                              $"Длительность: {selectedLog.DurationMs} мс\n" +
                              $"Контроллер: {selectedLog.Controller}\n" +
                              $"Действие: {selectedLog.Action}\n" +
                              $"Время: {selectedLog.Timestamp:dd.MM.yyyy HH:mm:ss}\n" +
                              $"Пользователь ID: {selectedLog.UserId}\n" +
                              $"IP адрес: {selectedLog.IpAddress}\n" +
                              $"User Agent: {selectedLog.UserAgent}";

                if (selectedLog.WarehouseId.HasValue)
                    details += $"\nСклад ID: {selectedLog.WarehouseId}";
                if (selectedLog.MaterialId.HasValue)
                    details += $"\nМатериал ID: {selectedLog.MaterialId}";
                if (selectedLog.ProductId.HasValue)
                    details += $"\nПродукт ID: {selectedLog.ProductId}";
                if (!string.IsNullOrEmpty(selectedLog.ErrorMessage))
                    details += $"\n\nОшибка: {selectedLog.ErrorMessage}";

                await DisplayAlert("Детали лога", details, "OK");

                // Deselect
                LogsCollectionView.SelectedItem = null;
            }
        }

        private async void OnExportCsvClicked(object sender, EventArgs e)
        {
            await ExportLogsAsync("csv");
        }

        private async void OnExportExcelClicked(object sender, EventArgs e)
        {
            await ExportLogsAsync("excel");
        }

        private async Task ExportLogsAsync(string format)
        {
            try
            {
                _viewModel.IsLoading = true;
                _viewModel.ErrorMessage = string.Empty;

                var fileData = await _apiService.ExportLogsAsync(
                    format: format,
                    userId: _viewModel.SelectedUserIdFilter,
                    warehouseId: _viewModel.SelectedWarehouseIdFilter,
                    materialId: _viewModel.SelectedMaterialIdFilter,
                    productId: _viewModel.SelectedProductIdFilter,
                    startDate: _viewModel.StartDateFilter,
                    endDate: _viewModel.EndDateFilter);

                if (fileData != null && fileData.Length > 0)
                {
                    var fileName = $"logs_{DateTime.Now:yyyyMMdd_HHmmss}.{(format == "csv" ? "csv" : "xlsx")}";
                    var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

                    await File.WriteAllBytesAsync(filePath, fileData);

                    await DisplayAlert("Успех", $"Файл сохранен: {filePath}", "OK");
                }
                else
                {
                    await DisplayAlert("Ошибка", "Не удалось экспортировать логи", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка экспорта: {ex.Message}", "OK");
            }
            finally
            {
                _viewModel.IsLoading = false;
            }
        }
    }
}
