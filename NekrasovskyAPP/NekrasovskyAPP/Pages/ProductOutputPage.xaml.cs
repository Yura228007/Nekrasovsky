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
        private List<Warehouse> _finishedGoodsWarehouses = new();
        private List<Warehouse> _disposalWarehouses = new();
        private List<User> _allUsers = new();
        /// <summary>Макс. количество для пары (productId, warehouseId). Ключ: warehouseId ?? -1 для "без склада".</summary>
        private Dictionary<(int productId, int warehouseKey), double?> _maxQuantityByProductWarehouse = new();

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
                    await DisplayAlert("Ошибка", "Не удалось загрузить варианты упаковки", "OK");
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
                    // При упаковке из партии: ключ (productId, -productBatchId); иначе (productId, warehouseId ?? -1)
                    var secondKey = o.ProductBatchId.HasValue ? -o.ProductBatchId.Value : (o.WarehouseId ?? -1);
                    _maxQuantityByProductWarehouse[(o.ProductId, secondKey)] = o.MaxQuantity;
                }

                if (_outputOptions.HasSendToSale)
                    _warehouses = await _apiService.GetAllWarehousesAsync();
                else
                {
                    var warehouseIds = options.Where(o => o.WarehouseId.HasValue).Select(o => o.WarehouseId!.Value).Distinct().ToList();
                    var allWarehousesTemp = await _apiService.GetAllWarehousesAsync();
                    _warehouses = allWarehousesTemp.Where(w => warehouseIds.Contains(w.Id)).ToList();
                }

                _outputs = await _apiService.GetProductOutputsByUserAsync(currentUser.Id);

                // Загружаем склады готовой продукции и утиля
                var allWarehousesForDisposal = await _apiService.GetAllWarehousesAsync();
                _finishedGoodsWarehouses = allWarehousesForDisposal
                    .Where(w => w.IsActive && w.Type?.Contains("Готовая продукция", StringComparison.OrdinalIgnoreCase) == true)
                    .ToList();
                _disposalWarehouses = allWarehousesForDisposal
                    .Where(w => w.IsActive && w.Type?.Contains("Утиль", StringComparison.OrdinalIgnoreCase) == true)
                    .ToList();

                // Загружаем пользователей для перемотки
                _allUsers = await _apiService.GetAllUsersAsync();

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
            var totalRewind = _outputs.Sum(o => o.RewindQuantity);

            TotalProducedLabel.Text = totalProduced.ToString();
            TotalDefectLabel.Text = totalDefect.ToString();
            TotalEcoLabel.Text = totalEco.ToString();
            TotalRewindLabel.Text = totalRewind.ToString();
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
                    $"Упаковка: {selectedOutput.ProductName}",
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
            var currentUser = _authService.CurrentUser;
            if (currentUser == null)
            {
                await DisplayAlert("Ошибка", "Пользователь не авторизован", "OK");
                return;
            }

            if (_outputOptions == null || !_products.Any())
            {
                await DisplayAlert("Ошибка", "Нет доступных продуктов под вашей ответственностью", "OK");
                return;
            }

            var title = existingOutput == null ? "Новая упаковка" : "Редактировать упаковку";
            var hasSendToSale = _outputOptions.HasSendToSale;

            ProductOutputOption? selectedOption = null;
            int? selectedWarehouseId = null;
            int warehouseKey = -1;
            Product? selectedProduct = null;

            // Выбор партии напрямую (всегда из партий под ответственностью)
            var batchOptions = _outputOptions.Options
                .Where(o => o.ProductBatchId.HasValue && o.MaxQuantity.HasValue && o.MaxQuantity.Value > 0)
                .Select(o => new
                {
                    Option = o,
                    DisplayName = $"{o.ProductName} - {o.BatchNumber ?? $"Партия #{o.ProductBatchId}"} ({o.WarehouseName ?? "Без склада"}: {o.MaxQuantity} {o.MeasuringUnit ?? "шт"})"
                })
                .ToList();

            if (batchOptions.Count == 0)
            {
                await DisplayAlert("Ошибка", "Нет доступных партий под вашей ответственностью", "OK");
                return;
            }

            var batchChoices = batchOptions.Select(b => b.DisplayName).ToArray();
            var selectedBatchChoice = await DisplayActionSheet($"{title} - Выберите партию:", "Отмена", null, batchChoices);
            if (selectedBatchChoice == "Отмена" || string.IsNullOrEmpty(selectedBatchChoice))
                return;

            var selectedBatchOption = batchOptions.FirstOrDefault(b => b.DisplayName == selectedBatchChoice);
            if (selectedBatchOption == null)
                return;

            selectedOption = selectedBatchOption.Option;
            if (selectedOption == null || !selectedOption.ProductBatchId.HasValue)
            {
                await DisplayAlert("Ошибка", "Не удалось выбрать партию", "OK");
                return;
            }

            selectedWarehouseId = selectedOption.WarehouseId;
            warehouseKey = selectedOption.ProductBatchId.HasValue ? -selectedOption.ProductBatchId.Value : (selectedWarehouseId ?? -1);
            
            // Получаем продукт для дальнейшей работы (для совместимости, но используем selectedOption)
            selectedProduct = _products.FirstOrDefault(p => p.Id == selectedOption.ProductId);

            // Доступное количество для выбранной партии
            var baseAvailableQuantity = selectedOption.MaxQuantity;
            double? availableQuantity = baseAvailableQuantity;
            if (existingOutput != null && existingOutput.ProductBatchId == selectedOption.ProductBatchId && baseAvailableQuantity.HasValue)
            {
                if (existingOutput.WarehouseId == selectedWarehouseId)
                {
                    var oldTotalUsed = existingOutput.ProducedQuantity + existingOutput.DefectQuantity + existingOutput.EcoQuantity + existingOutput.RewindQuantity;
                    availableQuantity = baseAvailableQuantity.Value + oldTotalUsed;
                }
            }

            var quantityLimitText = availableQuantity.HasValue
                ? $"\n(Можно выпустить: {availableQuantity.Value} {selectedOption.MeasuringUnit ?? "шт"})"
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
                    $"Количество произведенной продукции ({produced} {selectedOption.MeasuringUnit ?? "шт"}) превышает доступное количество ({availableQuantity.Value} {selectedOption.MeasuringUnit ?? "шт"})",
                    "OK");
                return;
            }

            // Enter eco quantity
            var ecoStr = await DisplayPromptAsync(
                title,
                $"Количество эко-продукции:{quantityLimitText}",
                "Далее",
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

            // Проверяем доступное количество для эко
            if (availableQuantity.HasValue)
            {
                var totalUsed = produced + eco;
                if (totalUsed > availableQuantity.Value)
                {
                    await DisplayAlert(
                        "Ошибка",
                        $"Сумма (произведено + эко = {totalUsed} {selectedOption.MeasuringUnit ?? "шт"}) превышает доступное количество ({availableQuantity.Value} {selectedOption.MeasuringUnit ?? "шт"})",
                        "OK");
                    return;
                }
            }

            // Выбор склада готовой продукции для нормальной и ЭКО продукции (один склад для обоих)
            int? normalWarehouseId = null;
            if (produced > 0 || eco > 0)
            {
                if (_finishedGoodsWarehouses.Count == 0)
                {
                    await DisplayAlert("Ошибка", "Нет доступных складов готовой продукции", "OK");
                    return;
                }

                var finishedGoodsOptions = _finishedGoodsWarehouses.Select(w => w.Name).ToArray();
                var selectedFinishedGoodsName = await DisplayActionSheet(
                    "Выберите склад готовой продукции для нормальной и ЭКО продукции:",
                    "Отмена", null, finishedGoodsOptions);
                if (selectedFinishedGoodsName == "Отмена")
                    return;

                normalWarehouseId = _finishedGoodsWarehouses
                    .FirstOrDefault(w => w.Name == selectedFinishedGoodsName)?.Id;
                if (normalWarehouseId == null)
                    return;
            }
            // ЭКО использует тот же склад что и нормальная
            int? ecoWarehouseId = normalWarehouseId;

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
                var totalUsed = produced + eco + defect;
                if (totalUsed > availableQuantity.Value)
                {
                    await DisplayAlert(
                        "Ошибка", 
                        $"Сумма (произведено + эко + брак = {totalUsed} {selectedOption.MeasuringUnit ?? "шт"}) превышает доступное количество ({availableQuantity.Value} {selectedOption.MeasuringUnit ?? "шт"})",
                        "OK");
                    return;
                }
            }

            // Выбор склада утиля для брака
            int? defectWarehouseId = null;
            if (defect > 0)
            {
                if (_disposalWarehouses.Count == 0)
                {
                    await DisplayAlert("Ошибка", "Нет доступных складов утиля", "OK");
                    return;
                }

                var disposalOptions = _disposalWarehouses.Select(w => w.Name).ToArray();
                var selectedDisposalName = await DisplayActionSheet(
                    "Выберите склад утиля для брака:",
                    "Отмена", null, disposalOptions);
                if (selectedDisposalName == "Отмена")
                    return;

                defectWarehouseId = _disposalWarehouses
                    .FirstOrDefault(w => w.Name == selectedDisposalName)?.Id;
                if (defectWarehouseId == null)
                    return;
            }

            // Enter rewind quantity
            var rewindStr = await DisplayPromptAsync(
                title,
                $"Количество на перемотку:{quantityLimitText}",
                "Сохранить",
                "Отмена",
                "0",
                -1,
                Keyboard.Numeric,
                existingOutput?.RewindQuantity.ToString() ?? "0");
            if (rewindStr == null)
                return;
            if (!int.TryParse(rewindStr, out var rewind) || rewind < 0)
            {
                await DisplayAlert("Ошибка", "Введите корректное число", "OK");
                return;
            }

            // Проверяем общее доступное количество (произведено + брак + эко + перемотка)
            if (availableQuantity.HasValue)
            {
                var totalUsed = produced + defect + eco + rewind;
                if (totalUsed > availableQuantity.Value)
                {
                    await DisplayAlert(
                        "Ошибка",
                        $"Общее количество (произведено + брак + эко + перемотка = {totalUsed} {selectedOption.MeasuringUnit ?? "шт"}) превышает доступное количество ({availableQuantity.Value} {selectedOption.MeasuringUnit ?? "шт"})",
                        "OK");
                    return;
                }
            }

            // Выбор склада и пользователя для перемотки
            int? rewindWarehouseId = null;
            int? rewindToUserId = null;
            if (rewind > 0)
            {
                // Выбор склада для перемотки
                var allWarehouses = await _apiService.GetAllWarehousesAsync();
                var rewindWarehouseOptions = allWarehouses
                    .Where(w => w.IsActive)
                    .Select(w => w.Name)
                    .ToArray();
                
                if (rewindWarehouseOptions.Length == 0)
                {
                    await DisplayAlert("Ошибка", "Нет доступных складов", "OK");
                    return;
                }

                var selectedRewindWarehouseName = await DisplayActionSheet(
                    "Выберите склад для перемотки:",
                    "Отмена", null, rewindWarehouseOptions);
                if (selectedRewindWarehouseName == "Отмена")
                    return;

                rewindWarehouseId = allWarehouses
                    .FirstOrDefault(w => w.Name == selectedRewindWarehouseName && w.IsActive)?.Id;
                if (rewindWarehouseId == null)
                    return;

                // Выбор пользователя для перемотки
                var userOptions = _allUsers
                    .Where(u => u.Id != currentUser.Id)
                    .Select(u => $"{u.Name} {u.Surname}")
                    .ToArray();

                if (userOptions.Length == 0)
                {
                    await DisplayAlert("Ошибка", "Нет доступных пользователей", "OK");
                    return;
                }

                var selectedUserName = await DisplayActionSheet(
                    "Выберите пользователя для перемотки:",
                    "Отмена", null, userOptions);
                if (selectedUserName == "Отмена")
                    return;

                var selectedUser = _allUsers
                    .FirstOrDefault(u => $"{u.Name} {u.Surname}" == selectedUserName);
                if (selectedUser == null)
                    return;

                rewindToUserId = selectedUser.Id;
            }

            // currentUser уже объявлен в начале метода

            var output = new ProductOutput
            {
                Id = existingOutput?.Id ?? 0,
                UserId = currentUser.Id,
                ProductId = selectedOption.ProductId,
                WarehouseId = selectedWarehouseId,
                ProductBatchId = selectedOption.ProductBatchId,
                ProducedQuantity = produced,
                DefectQuantity = defect,
                EcoQuantity = eco,
                RewindQuantity = rewind,
                NormalWarehouseId = normalWarehouseId,
                EcoWarehouseId = ecoWarehouseId,
                DefectWarehouseId = defectWarehouseId,
                RewindWarehouseId = rewindWarehouseId,
                RewindToUserId = rewindToUserId,
                MeasuringUnit = selectedOption.MeasuringUnit ?? "шт",
                CreatedAt = existingOutput?.CreatedAt ?? DateTime.UtcNow
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
                $"Удалить запись об упаковке продукта \"{output.ProductName}\"?",
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
