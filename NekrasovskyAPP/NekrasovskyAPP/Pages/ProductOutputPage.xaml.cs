using NekrasovskyAPP.Models;
using NekrasovskyAPP.Services;

namespace NekrasovskyAPP.Pages
{
    public partial class ProductOutputPage : ContentPage
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;
        private List<ProductOutput> _outputs = new();
        private List<Product> _products = new();
        private List<Warehouse> _warehouses = new();
        private List<Machine> _machines = new();

        public ProductOutputPage(IApiService apiService, IAuthService authService)
        {
            InitializeComponent();
            _apiService = apiService;
            _authService = authService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                LoadingIndicator.IsRunning = true;
                LoadingIndicator.IsVisible = true;

                var currentUser = _authService.CurrentUser;
                if (currentUser == null)
                {
                    await DisplayAlert("Ошибка", "Пользователь не авторизован", "OK");
                    return;
                }

                _products = await _apiService.GetAllProductsAsync();
                _warehouses = await _apiService.GetAllWarehousesAsync();
                _machines = await _apiService.GetActiveMachinesAsync();
                _outputs = await _apiService.GetProductOutputsByUserAsync(currentUser.Id);

                OutputsCollectionView.ItemsSource = _outputs;
                UpdateSummary();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить данные: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
                RefreshViewControl.IsRefreshing = false;
            }
        }

        private void UpdateSummary()
        {
            var totalProduced = _outputs.Sum(o => o.ProducedQuantity);
            var totalDefect = _outputs.Sum(o => o.DefectQuantity);
            var totalEco = _outputs.Sum(o => o.EcoQuantity);

            TotalProducedLabel.Text = totalProduced.ToString();
            TotalDefectLabel.Text = totalDefect.ToString();
            TotalEcoLabel.Text = totalEco.ToString();
        }

        private async void OnRefreshClicked(object? sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        private async void OnRefreshing(object? sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        private async void OnAddClicked(object? sender, EventArgs e)
        {
            await ShowAddEditDialogAsync(null);
        }

        private async void OnOutputSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is ProductOutput selectedOutput)
            {
                OutputsCollectionView.SelectedItem = null;

                var action = await DisplayActionSheet(
                    $"Выпуск: {selectedOutput.ProductName}",
                    "Отмена",
                    null,
                    "Редактировать",
                    "Удалить");

                switch (action)
                {
                    case "Редактировать":
                        await ShowAddEditDialogAsync(selectedOutput);
                        break;
                    case "Удалить":
                        await DeleteOutputAsync(selectedOutput);
                        break;
                }
            }
        }

        private async Task ShowAddEditDialogAsync(ProductOutput? existingOutput)
        {
            if (!_products.Any())
            {
                await DisplayAlert("Ошибка", "Нет доступных продуктов", "OK");
                return;
            }

            var title = existingOutput == null ? "Новый выпуск" : "Редактировать выпуск";

            // Select product
            var productOptions = _products.Select(p => p.Name).ToArray();
            var selectedProductName = await DisplayActionSheet($"{title} - Выберите продукт:", "Отмена", null, productOptions);
            if (selectedProductName == "Отмена" || string.IsNullOrEmpty(selectedProductName))
                return;

            var selectedProduct = _products.FirstOrDefault(p => p.Name == selectedProductName);
            if (selectedProduct == null)
                return;

            // Select warehouse (optional)
            var warehouseOptions = new[] { "Без склада" }.Concat(_warehouses.Where(w => w.IsActive).Select(w => w.Name)).ToArray();
            var selectedWarehouseName = await DisplayActionSheet("Выберите склад (опционально):", "Отмена", null, warehouseOptions);
            if (selectedWarehouseName == "Отмена")
                return;

            var selectedWarehouse = selectedWarehouseName == "Без склада" ? null : _warehouses.FirstOrDefault(w => w.Name == selectedWarehouseName);

            // Select machine (optional)
            Machine? selectedMachine = null;
            if (_machines.Any())
            {
                var machineOptions = new[] { "Без станка" }.Concat(_machines.Select(m => m.DisplayName)).ToArray();
                var selectedMachineName = await DisplayActionSheet("Выберите станок/линию (опционально):", "Отмена", null, machineOptions);
                if (selectedMachineName == "Отмена")
                    return;

                selectedMachine = selectedMachineName == "Без станка" ? null : _machines.FirstOrDefault(m => m.DisplayName == selectedMachineName);
            }

            // Enter produced quantity
            var producedStr = await DisplayPromptAsync(
                title,
                "Количество произведенной продукции:",
                "Далее",
                "Отмена",
                "0",
                -1,
                Keyboard.Numeric,
                existingOutput?.ProducedQuantity.ToString() ?? "0");
            if (producedStr == null)
                return;
            if (!int.TryParse(producedStr, out var produced) || produced < 0)
            {
                await DisplayAlert("Ошибка", "Введите корректное число", "OK");
                return;
            }

            // Enter defect quantity
            var defectStr = await DisplayPromptAsync(
                title,
                "Количество брака:",
                "Далее",
                "Отмена",
                "0",
                -1,
                Keyboard.Numeric,
                existingOutput?.DefectQuantity.ToString() ?? "0");
            if (defectStr == null)
                return;
            if (!int.TryParse(defectStr, out var defect) || defect < 0)
            {
                await DisplayAlert("Ошибка", "Введите корректное число", "OK");
                return;
            }

            // Enter eco quantity
            var ecoStr = await DisplayPromptAsync(
                title,
                "Количество эко-продукции:",
                "Сохранить",
                "Отмена",
                "0",
                -1,
                Keyboard.Numeric,
                existingOutput?.EcoQuantity.ToString() ?? "0");
            if (ecoStr == null)
                return;
            if (!int.TryParse(ecoStr, out var eco) || eco < 0)
            {
                await DisplayAlert("Ошибка", "Введите корректное число", "OK");
                return;
            }

            var currentUser = _authService.CurrentUser;
            if (currentUser == null)
            {
                await DisplayAlert("Ошибка", "Пользователь не авторизован", "OK");
                return;
            }

            var output = new ProductOutput
            {
                Id = existingOutput?.Id ?? 0,
                UserId = currentUser.Id,
                ProductId = selectedProduct.Id,
                WarehouseId = selectedWarehouse?.Id,
                MachineId = selectedMachine?.Id,
                ProducedQuantity = produced,
                DefectQuantity = defect,
                EcoQuantity = eco,
                MeasuringUnit = selectedProduct.MeasuringUnit
            };

            try
            {
                LoadingIndicator.IsRunning = true;
                LoadingIndicator.IsVisible = true;

                ApiResponse<ProductOutput> response;
                if (existingOutput == null)
                {
                    response = await _apiService.AddProductOutputAsync(output);
                }
                else
                {
                    response = await _apiService.EditProductOutputAsync(existingOutput.Id, output);
                }

                if (response.Output != null)
                {
                    await DisplayAlert("Успех", response.Message, "OK");
                    await LoadDataAsync();
                }
                else
                {
                    await DisplayAlert("Ошибка", response.Message ?? "Не удалось сохранить", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось сохранить: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            }
        }

        private async Task DeleteOutputAsync(ProductOutput output)
        {
            var confirm = await DisplayAlert(
                "Удаление",
                $"Удалить запись о выпуске продукта \"{output.ProductName}\"?",
                "Удалить",
                "Отмена");

            if (!confirm)
                return;

            try
            {
                LoadingIndicator.IsRunning = true;
                LoadingIndicator.IsVisible = true;

                var response = await _apiService.DeleteProductOutputAsync(output.Id);
                await DisplayAlert("Успех", response.Message ?? "Запись удалена", "OK");
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось удалить: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            }
        }
    }
}
