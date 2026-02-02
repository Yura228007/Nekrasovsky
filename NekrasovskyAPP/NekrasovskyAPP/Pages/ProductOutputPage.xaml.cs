using NekrasovskyAPP.Models;
using NekrasovskyAPP.Services;

namespace NekrasovskyAPP.Pages
{
    public partial class ProductOutputPage : ContentPage
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;
        private List<ProductOutput> _outputs = new();
        private ProductOutputOptionsResponse? _outputOptions;
        private List<Product> _products = new();
        private List<Warehouse> _warehouses = new();
        private List<Machine> _machines = new();
        /// <summary>Макс. количество для пары (productId, warehouseId). Ключ: warehouseId ?? -1 для "без склада".</summary>
        private Dictionary<(int productId, int warehouseKey), int?> _maxQuantityByProductWarehouse = new();

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

                _outputOptions = await _apiService.GetProductOutputOptionsAsync();
                if (_outputOptions == null)
                {
                    await DisplayAlert("Ошибка", "Не удалось загрузить варианты выпуска", "OK");
                    return;
                }

                var options = _outputOptions.Options;
                _products = options
                    .GroupBy(o => o.ProductId)
                    .Select(g => new Product
                    {
                        Id = g.Key,
                        Name = g.First().ProductName,
                        MeasuringUnit = g.First().MeasuringUnit ?? "шт",
                        IsActive = true
                    })
                    .ToList();

                _maxQuantityByProductWarehouse.Clear();
                foreach (var o in options)
                {
                    // При выпуске из партии: ключ (productId, -productBatchId); иначе (productId, warehouseId ?? -1)
                    var secondKey = o.ProductBatchId.HasValue ? -o.ProductBatchId.Value : (o.WarehouseId ?? -1);
                    _maxQuantityByProductWarehouse[(o.ProductId, secondKey)] = o.MaxQuantity;
                }

                if (_outputOptions.HasSendToSale)
                    _warehouses = await _apiService.GetAllWarehousesAsync();
                else
                {
                    var warehouseIds = options.Where(o => o.WarehouseId.HasValue).Select(o => o.WarehouseId!.Value).Distinct().ToList();
                    var allWarehouses = await _apiService.GetAllWarehousesAsync();
                    _warehouses = allWarehouses.Where(w => warehouseIds.Contains(w.Id)).ToList();
                }

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
            if (_outputOptions == null || !_products.Any())
            {
                await DisplayAlert("Ошибка", "Нет доступных продуктов под вашей ответственностью", "OK");
                return;
            }

            var title = existingOutput == null ? "Новый выпуск" : "Редактировать выпуск";
            var hasSendToSale = _outputOptions.HasSendToSale;

            ProductOutputOption? selectedOption = null;
            int? selectedWarehouseId = null;
            int warehouseKey = -1;
            string? selectedProductName = null;
            Product? selectedProduct = null;

            if (hasSendToSale)
            {
                // Выбор продукта
                var productOptions = _products.Select(p => p.Name).ToArray();
                selectedProductName = await DisplayActionSheet($"{title} - Выберите продукт:", "Отмена", null, productOptions);
                if (selectedProductName == "Отмена" || string.IsNullOrEmpty(selectedProductName))
                    return;

                selectedProduct = _products.FirstOrDefault(p => p.Name == selectedProductName);
                if (selectedProduct == null)
                    return;

                var warehousesForProduct = _outputOptions.Options
                    .Where(o => o.ProductId == selectedProduct.Id)
                    .ToList();
                var warehouseChoices = warehousesForProduct
                    .Select(o => o.WarehouseName ?? "Без склада")
                    .Distinct()
                    .ToList();
                if (!warehouseChoices.Contains("Без склада"))
                    warehouseChoices.Insert(0, "Без склада");

                var warehouseOptions = warehouseChoices.ToArray();
                if (warehouseOptions.Length == 0)
                {
                    await DisplayAlert("Ошибка", "Нет складов для этого продукта", "OK");
                    return;
                }

                var selectedWarehouseName = await DisplayActionSheet("Выберите склад:", "Отмена", null, warehouseOptions);
                if (selectedWarehouseName == "Отмена")
                    return;

                if (selectedWarehouseName != "Без склада")
                {
                    var opt = warehousesForProduct.FirstOrDefault(o => o.WarehouseName == selectedWarehouseName);
                    selectedWarehouseId = opt?.WarehouseId;
                }
                warehouseKey = selectedWarehouseId ?? -1;
                selectedOption = warehousesForProduct.FirstOrDefault(o =>
                    (o.WarehouseName ?? "Без склада") == selectedWarehouseName);
            }
            else
            {
                // Выпуск только из партий: выбор партии (продукт + партия + макс. количество)
                var batchChoices = _outputOptions.Options
                    .Where(o => o.ProductBatchId.HasValue && o.MaxQuantity.HasValue)
                    .Select(o => $"{o.ProductName} — партия {o.BatchNumber ?? o.ProductBatchId.ToString()} (макс. {o.MaxQuantity})")
                    .ToArray();
                if (batchChoices.Length == 0)
                {
                    await DisplayAlert("Ошибка", "Нет доступных партий под вашей ответственностью для выпуска", "OK");
                    return;
                }

                var selectedChoice = await DisplayActionSheet($"{title} - Выберите партию для выпуска:", "Отмена", null, batchChoices);
                if (selectedChoice == "Отмена" || string.IsNullOrEmpty(selectedChoice))
                    return;

                var idx = Array.IndexOf(batchChoices, selectedChoice);
                selectedOption = _outputOptions.Options
                    .Where(o => o.ProductBatchId.HasValue && o.MaxQuantity.HasValue)
                    .ElementAtOrDefault(idx);
                if (selectedOption == null)
                    return;
                warehouseKey = selectedOption.ProductBatchId.HasValue ? -selectedOption.ProductBatchId.Value : -1;
            }

            if (hasSendToSale)
                selectedProduct = _products.FirstOrDefault(p => p.Name == selectedProductName!);
            else if (selectedOption != null)
                selectedProduct = new Product
                {
                    Id = selectedOption.ProductId,
                    Name = selectedOption.ProductName,
                    MeasuringUnit = selectedOption.MeasuringUnit ?? "шт",
                    IsActive = true
                };
            if (selectedProduct == null)
                return;

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

            // Доступное количество для выбранной пары (продукт + склад) или партии
            var baseAvailableQuantity = _maxQuantityByProductWarehouse.TryGetValue((selectedProduct.Id, warehouseKey), out var maxQty) ? maxQty : null;
            int? availableQuantity = baseAvailableQuantity;
            if (existingOutput != null && existingOutput.ProductId == selectedProduct.Id && baseAvailableQuantity.HasValue)
            {
                var sameSource = hasSendToSale
                    ? existingOutput.WarehouseId == selectedWarehouseId
                    : existingOutput.ProductBatchId == selectedOption?.ProductBatchId;
                if (sameSource)
                {
                    var oldTotalUsed = existingOutput.ProducedQuantity + existingOutput.DefectQuantity + existingOutput.EcoQuantity;
                    availableQuantity = baseAvailableQuantity.Value + oldTotalUsed;
                }
            }

            var quantityLimitText = availableQuantity.HasValue
                ? $"\n(Можно выпустить: {availableQuantity.Value} {selectedProduct.MeasuringUnit})"
                : "";

            // Enter produced quantity
            var producedStr = await DisplayPromptAsync(
                title,
                $"Количество произведенной продукции:{quantityLimitText}",
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
            
            // Проверяем доступное количество
            if (availableQuantity.HasValue && produced > availableQuantity.Value)
            {
                await DisplayAlert(
                    "Ошибка", 
                    $"Количество произведенной продукции ({produced} {selectedProduct.MeasuringUnit}) превышает доступное количество ({availableQuantity.Value} {selectedProduct.MeasuringUnit})",
                    "OK");
                return;
            }

            // Enter defect quantity
            var defectStr = await DisplayPromptAsync(
                title,
                $"Количество брака:{quantityLimitText}",
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
            
            // Проверяем доступное количество для брака
            if (availableQuantity.HasValue)
            {
                var totalUsed = produced + defect;
                if (totalUsed > availableQuantity.Value)
                {
                    await DisplayAlert(
                        "Ошибка", 
                        $"Сумма произведенной продукции и брака ({totalUsed} {selectedProduct.MeasuringUnit}) превышает доступное количество ({availableQuantity.Value} {selectedProduct.MeasuringUnit})",
                        "OK");
                    return;
                }
            }

            // Enter eco quantity
            var ecoStr = await DisplayPromptAsync(
                title,
                $"Количество эко-продукции:{quantityLimitText}",
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
            
            // Проверяем общее доступное количество (произведено + брак + эко)
            if (availableQuantity.HasValue)
            {
                var totalUsed = produced + defect + eco;
                if (totalUsed > availableQuantity.Value)
                {
                    await DisplayAlert(
                        "Ошибка", 
                        $"Общее количество (произведено + брак + эко = {totalUsed} {selectedProduct.MeasuringUnit}) превышает доступное количество ({availableQuantity.Value} {selectedProduct.MeasuringUnit})",
                        "OK");
                    return;
                }
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
                WarehouseId = hasSendToSale ? selectedWarehouseId : selectedOption?.TargetWarehouseId,
                ProductBatchId = selectedOption?.ProductBatchId,
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
