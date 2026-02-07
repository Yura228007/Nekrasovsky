using NekrasovskyAPP.ViewModels;
using NekrasovskyAPP.Models;
using System.Linq;
using System.Collections.Generic;

namespace NekrasovskyAPP.Pages
{
    public partial class MaterialsPage : ContentPage
    {
        private readonly MainViewModel _viewModel;
        private bool _permissionsChecked;

        public MaterialsPage(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Проверяем права один раз при открытии страницы
            if (!_permissionsChecked)
            {
                await UpdateToolbarPermissionsAsync();
                _permissionsChecked = true;
            }

            // Всегда обновляем данные при заходе на страницу
            await ApplyFiltersAsync();
        }

        private async Task UpdateToolbarPermissionsAsync()
        {
            var canAdd = await _viewModel.CanAddOrEditItemsAsync();
            var canScan = await _viewModel.HasAssignBarcodePermissionAsync();

            // ToolbarItem не поддерживает IsVisible, поэтому удаляем элементы
            if (!canScan && ToolbarItems.Contains(ScanToolbarItem))
            {
                ToolbarItems.Remove(ScanToolbarItem);
            }

            if (!canAdd && ToolbarItems.Contains(AddToolbarItem))
            {
                ToolbarItems.Remove(AddToolbarItem);
            }
        }

        private async void OnRefreshing(object? sender, EventArgs e)
        {
            try
            {
                // Refresh should respect current search/filters.
                await ApplyFiltersAsync();
            }
            finally
            {
                // Stop the pull-to-refresh spinner.
                if (sender is RefreshView refreshView)
                {
                    refreshView.IsRefreshing = false;
                }
            }
        }

        private async void OnRefreshClicked(object? sender, EventArgs e)
        {
            await ApplyFiltersAsync();
        }

        private string _lastSearchText = string.Empty;

        private async void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
        {
            var searchText = e.NewTextValue ?? string.Empty;

            if (searchText == _lastSearchText)
                return;

            _lastSearchText = searchText;

            await ApplyFiltersAsync();
        }

        private async void OnFilterChanged(object? sender, EventArgs e)
        {
            await ApplyFiltersAsync();
        }

        private async void OnScanBarcodeClicked(object? sender, EventArgs e)
        {
#if ANDROID || IOS
            try
            {
                var scannerPage = new BarcodeScannerPage();
                scannerPage.BarcodeScanned += OnBarcodeScanned;
                await Navigation.PushModalAsync(scannerPage);
            }
            catch
            {
                await DisplayAlert("Ошибка", "Не удалось открыть сканер. Убедитесь, что приложение имеет разрешение на использование камеры.", "OK");
            }
#else
            await DisplayAlert("Недоступно", "Сканирование штрих-кодов доступно только на Android и iOS устройствах.", "OK");
#endif
        }

        private async void OnBarcodeScanned(object? sender, string barcodeValue)
        {
#if ANDROID || IOS
            if (sender is BarcodeScannerPage scannerPage)
            {
                scannerPage.BarcodeScanned -= OnBarcodeScanned;
            }
#endif

            if (!string.IsNullOrWhiteSpace(barcodeValue))
            {
                SearchEntry.Text = barcodeValue;
                await _viewModel.SearchMaterialsAsync(null, barcodeValue);

                await DisplayAlert(
                    "Штрих-код найден",
                    $"Найден штрих-код: {barcodeValue}\nВыполняется поиск...",
                    "OK");
            }
        }

        private async Task ApplyFiltersAsync()
        {
            var searchText = _lastSearchText ?? string.Empty;

            // Если нет поиска и все фильтры не выбраны, загружаем все материалы
            if (string.IsNullOrWhiteSpace(searchText) &&
                (StatusPicker.SelectedIndex <= 0) &&
                (SortPicker.SelectedIndex < 0))
            {
                await _viewModel.LoadMaterialsAsync();
                return;
            }

            // Поиск по имени и коду одновременно (сервер использует OR логику)
            string? name = null;
            string? code = null;
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var searchTerm = searchText.Trim();
                // Передаем поисковый запрос в оба параметра - сервер найдет по имени ИЛИ по коду
                name = searchTerm;
                code = searchTerm;
            }

            // Фильтр по статусу
            bool? isActive = null;
            if (StatusPicker.SelectedIndex == 1)
                isActive = true;
            else if (StatusPicker.SelectedIndex == 2)
                isActive = false;

            // Сортировка
            string? sortBy = null;
            if (SortPicker.SelectedIndex >= 0)
            {
                sortBy = SortPicker.SelectedIndex switch
                {
                    0 => "name",
                    1 => "name_desc",
                    2 => "code",
                    3 => "code_desc",
                    _ => null
                };
            }

            await _viewModel.SearchMaterialsAsync(name, code, isActive, sortBy);
        }

        private async void OnAddClicked(object? sender, EventArgs e)
        {
            await ShowMaterialDialogAsync(null);
        }

        private async void OnMaterialSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is MaterialDisplayItem displayItem)
            {
                var selectedMaterial = displayItem.Material;
                {
                    var isPrivileged = await _viewModel.IsPrivilegedUserAsync();
                    var canDelete = await _viewModel.CanDeleteItemsAsync();
                    var canEdit = await _viewModel.CanAddOrEditItemsAsync();
                    var canTransfer = await _viewModel.HasTransferPermissionAsync();
                    var canManageResponsibility = await _viewModel.HasManageResponsibilityAsync();
                    var currentUser = _viewModel.AuthService?.CurrentUser;
                    var canSendToSDH = currentUser != null && await _viewModel.ApiService.CheckPermissionAsync(currentUser.Id, "SendToSDH");

                    var actions = new List<string> { "Просмотр" };

                    if (canEdit)
                    {
                        actions.Add("Редактировать");
                    }
                    if (canTransfer)
                    {
                        actions.Add("Изменить количество");
                    }
                    if (canDelete)
                    {
                        actions.Add("Удалить");
                    }
                    if (canManageResponsibility)
                    {
                        actions.Add("Изменить ответственное лицо");
                        actions.Add("Снять ответственность");
                    }
                    if (canSendToSDH && displayItem.Responsibility != null && displayItem.Responsibility.WarehouseId.HasValue)
                    {
                        actions.Add("Отправить на СДХ");
                    }

                    var action = await DisplayActionSheet(
                        $"Материал: {selectedMaterial.Name}",
                        "Отмена",
                        null,
                        actions.ToArray());

                    switch (action)
                    {
                        case "Просмотр":
                            await DisplayAlert("Информация о материале",
                                $"Название: {selectedMaterial.Name}\n" +
                                $"Код: {selectedMaterial.Code ?? "Не указан"}\n" +
                                $"Описание: {selectedMaterial.Description ?? "Не указано"}\n" +
                                $"Единица измерения: {selectedMaterial.MeasuringUnit}",
                                "OK");
                            break;

                        case "Редактировать":
                            await ShowMaterialDialogAsync(selectedMaterial);
                            break;

                        case "Изменить количество":
                            await ShowMaterialQuantityDialogAsync(displayItem);
                            break;

                        case "Удалить":
                            // Удаляем только конкретную карточку (ResponsibilityFilling), а не весь материал
                            if (displayItem.Responsibility == null || !displayItem.Responsibility.ResponsibilityFillingId.HasValue)
                            {
                                await DisplayAlert("Ошибка", "Эта операция доступна только для карточек с ответственным лицом.", "OK");
                                break;
                            }

                            var confirm = await DisplayAlert(
                                "Подтверждение удаления",
                                $"Вы уверены, что хотите удалить эту карточку?\n\nМатериал: {selectedMaterial.Name}\nОтветственный: {displayItem.Responsibility.UserName}\nКоличество: {displayItem.Responsibility.Quantity} {displayItem.Responsibility.MeasuringUnit ?? selectedMaterial.MeasuringUnit}",
                                "Удалить",
                                "Отмена");

                            if (confirm)
                            {
                                var response = await _viewModel.ApiService.DeleteResponsibilityFillingAsync(displayItem.Responsibility.ResponsibilityFillingId.Value);
                                if (!response.IsSuccess)
                                {
                                    await DisplayAlert("Ошибка", response.Message ?? "Произошла ошибка", "OK");
                                }
                                else
                                {
                                    await DisplayAlert("Успех", "Карточка успешно удалена", "OK");
                                    await _viewModel.LoadMaterialsAsync();
                                }
                            }
                            break;
                        case "Изменить ответственное лицо":
                            await ChangeMaterialResponsibilityAsync(displayItem);
                            break;
                        case "Снять ответственность":
                            await ReleaseMaterialResponsibilityAsync(displayItem);
                            break;
                        case "Отправить на СДХ":
                            await SendMaterialToSDHAsync(displayItem);
                            break;
                    }

                    MaterialsCollectionView.SelectedItem = null;
                }
            }
        }

        private async Task ShowMaterialQuantityDialogAsync(MaterialDisplayItem displayItem)
        {
            var material = displayItem.Material;
            
            // Работаем только с карточками, где есть ответственное лицо
            if (displayItem.Responsibility == null || !displayItem.Responsibility.WarehouseId.HasValue)
            {
                await DisplayAlert("Ошибка", "Эта операция доступна только для карточек с ответственным лицом и складом.", "OK");
                return;
            }

            var warehouseId = displayItem.Responsibility.WarehouseId.Value;
            var userId = displayItem.Responsibility.UserId;
            var currentQuantity = displayItem.Responsibility.Quantity ?? 0;
            var unit = displayItem.Responsibility.MeasuringUnit ?? material.MeasuringUnit ?? "ед.";

            var quantityText = await DisplayPromptAsync(
                "Изменить количество",
                $"Текущее количество: {currentQuantity} {unit}\nВведите новое количество:",
                "Сохранить",
                "Отмена",
                currentQuantity.ToString(),
                -1,
                Keyboard.Numeric);

            if (quantityText == null)
            {
                return;
            }

            if (!double.TryParse(quantityText, out var newQuantity))
            {
                await DisplayAlert("Ошибка", "Количество должно быть числом.", "OK");
                return;
            }

            if (newQuantity < 0)
            {
                await DisplayAlert("Ошибка", "Количество не может быть отрицательным.", "OK");
                return;
            }

            if (newQuantity == 0)
            {
                var confirmZero = await DisplayAlert(
                    "Подтверждение",
                    "Количество = 0. Оставить так?",
                    "Да",
                    "Изменить");
                if (!confirmZero)
                {
                    return;
                }
            }

            var response = await _viewModel.ApiService.UpdateMaterialResponsibilityFillingAsync(
                warehouseId, material.Id, userId, newQuantity, unit);
            
            if (!response.IsSuccess)
            {
                await DisplayAlert("Ошибка", response.Message ?? "Произошла ошибка", "OK");
                return;
            }

            await _viewModel.LoadMaterialsAsync();
            await DisplayAlert("Успех", "Количество обновлено", "OK");
        }

        private async Task ChangeMaterialResponsibilityAsync(MaterialDisplayItem displayItem)
        {
            var material = displayItem.Material;

            if (_viewModel.Users.Count == 0)
            {
                await _viewModel.LoadUsersAsync();
            }

            if (_viewModel.Users.Count == 0)
            {
                await DisplayAlert("Ошибка", "Нет доступных пользователей для назначения ответственности.", "OK");
                return;
            }

            var options = _viewModel.Users
                .Select(u => $"{u.Surname} {u.Name}")
                .ToArray();

            var choice = await DisplayActionSheet("Выберите ответственное лицо:", "Отмена", null, options);
            if (string.IsNullOrWhiteSpace(choice) || choice == "Отмена")
                return;

            var index = Array.IndexOf(options, choice);
            if (index < 0 || index >= _viewModel.Users.Count)
                return;
            var selectedUser = _viewModel.Users[index];

            // Передача ответственности по складу (ResponsibilityFilling): забираем часть или всё у текущего, передаём новому
            if (displayItem.Responsibility != null && displayItem.Responsibility.WarehouseId.HasValue)
            {
                var fromUserId = displayItem.Responsibility.UserId;
                if (fromUserId == selectedUser.Id)
                {
                    await DisplayAlert("Ошибка", "Выберите другого пользователя (не текущего ответственного).", "OK");
                    return;
                }

                var warehouseId = displayItem.Responsibility.WarehouseId.Value;
                var currentQty = displayItem.Responsibility.Quantity ?? 0;
                var unit = displayItem.Responsibility.MeasuringUnit ?? material.MeasuringUnit ?? "ед.";

                int? quantityToTransfer = null;
                if (currentQty > 0)
                {
                    var transferChoice = await DisplayActionSheet(
                        "Сколько передать новому ответственному?",
                        "Отмена",
                        null,
                        "Всё количество",
                        "Часть (указать)");
                    if (string.IsNullOrWhiteSpace(transferChoice) || transferChoice == "Отмена")
                        return;
                    if (transferChoice == "Часть (указать)")
                    {
                        var qtyText = await DisplayPromptAsync(
                            "Количество",
                            $"Укажите, сколько передать (макс. {currentQty} {unit}). У текущего ответственного останется остаток.",
                            "Передать",
                            "Отмена",
                            currentQty.ToString(),
                            -1,
                            Keyboard.Numeric);
                        if (qtyText == null)
                            return;
                        if (!int.TryParse(qtyText, out var qty) || qty <= 0 || qty > currentQty)
                        {
                            await DisplayAlert("Ошибка", $"Введите число от 1 до {currentQty}.", "OK");
                            return;
                        }
                        quantityToTransfer = qty;
                    }
                }

                var response = await _viewModel.ApiService.TransferMaterialResponsibilityFillingAsync(
                    warehouseId, material.Id, fromUserId, selectedUser.Id, quantityToTransfer);
                if (!response.IsSuccess)
                {
                    await DisplayAlert("Ошибка", response.Message ?? "Произошла ошибка", "OK");
                    return;
                }
                var msg = quantityToTransfer.HasValue
                    ? $"Передано {quantityToTransfer} {unit}. У предыдущего ответственного осталось {currentQty - quantityToTransfer.Value} {unit}."
                    : "Вся ответственность передана новому лицу.";
                await DisplayAlert("Успех", msg, "OK");
                await _viewModel.LoadMaterialsAsync();
                return;
            }

            // Назначение без склада (неответственная часть) - нужно выбрать склад
            if (displayItem.Responsibility == null)
            {
                // Получаем склады, где есть неответственное количество
                var fillings = await _viewModel.ApiService.GetFillingsByMaterialAsync(material.Id);
                if (fillings.Count == 0)
                {
                    await DisplayAlert("Ошибка", "Для этого материала нет записей по складам.", "OK");
                    return;
                }

                var activeWarehouses = await GetActiveWarehousesAsync();
                var warehouseMap = activeWarehouses.ToDictionary(
                    w => w.Id,
                    w => $"{w.Name} ({w.Type})");

                // Получаем ответственность по складам для этого материала
                var responsibilityFillings = await _viewModel.ApiService.GetResponsibilityFillingsByUserAsync(selectedUser.Id);
                var responsibleByWarehouse = responsibilityFillings
                    .Where(rf => rf.MaterialId == material.Id && rf.IsActive)
                    .GroupBy(rf => rf.WarehouseId)
                    .ToDictionary(g => g.Key, g => g.Sum(rf => rf.Quantity));

                // Вычисляем неответственное количество по каждому складу
                var availableWarehouses = fillings
                    .Where(f => warehouseMap.ContainsKey(f.WarehouseId))
                    .Select(f => new
                    {
                        Filling = f,
                        WarehouseId = f.WarehouseId,
                        WarehouseName = warehouseMap[f.WarehouseId],
                        AvailableQty = f.Quantity - responsibleByWarehouse.GetValueOrDefault(f.WarehouseId, 0)
                    })
                    .Where(x => x.AvailableQty > 0)
                    .ToList();

                if (availableWarehouses.Count == 0)
                {
                    await DisplayAlert("Ошибка", "Нет доступного количества для назначения ответственности.", "OK");
                    return;
                }

                // Если один склад, используем его, иначе запрашиваем выбор
                int selectedWarehouseId;
                if (availableWarehouses.Count == 1)
                {
                    selectedWarehouseId = availableWarehouses[0].WarehouseId;
                }
                else
                {
                    var warehouseOptions = availableWarehouses
                        .Select(w => $"{w.WarehouseName}: {w.AvailableQty} {w.Filling.MeasuringType ?? material.MeasuringUnit ?? "ед."}")
                        .ToArray();
                    var warehouseChoice = await DisplayActionSheet("Выберите склад:", "Отмена", null, warehouseOptions);
                    if (string.IsNullOrWhiteSpace(warehouseChoice) || warehouseChoice == "Отмена")
                        return;
                    var warehouseIndex = Array.IndexOf(warehouseOptions, warehouseChoice);
                    if (warehouseIndex < 0 || warehouseIndex >= availableWarehouses.Count)
                        return;
                    selectedWarehouseId = availableWarehouses[warehouseIndex].WarehouseId;
                }

                var selectedWarehouse = availableWarehouses.First(w => w.WarehouseId == selectedWarehouseId);
                var maxQuantity = selectedWarehouse.AvailableQty;
                var unit = selectedWarehouse.Filling.MeasuringType ?? material.MeasuringUnit ?? "ед.";

                var quantityText = await DisplayPromptAsync(
                    "Количество",
                    $"Укажите количество для назначения ответственности (макс. {maxQuantity} {unit}):",
                    "Назначить",
                    "Отмена",
                    maxQuantity.ToString(),
                    -1,
                    Keyboard.Numeric);
                
                if (quantityText == null)
                    return;

                if (!double.TryParse(quantityText, out var qty) || qty <= 0 || qty > maxQuantity)
                {
                    await DisplayAlert("Ошибка", $"Введите число от 1 до {maxQuantity}.", "OK");
                    return;
                }

                // Используем endpoint для назначения через ResponsibilityFilling с указанием склада
                var assignResponse = await _viewModel.ApiService.AssignMaterialResponsibilityFillingAsync(
                    selectedWarehouseId, material.Id, selectedUser.Id, qty, unit);
                
                if (!assignResponse.IsSuccess)
                {
                    await DisplayAlert("Ошибка", assignResponse.Message ?? "Произошла ошибка", "OK");
                    return;
                }
                
                await DisplayAlert("Успех", $"Ответственность назначена. Количество: {qty} {unit}", "OK");
                await _viewModel.LoadMaterialsAsync();
                return;
            }

            // Старая логика для случаев с ответственным лицом (не должна использоваться, но оставляем для совместимости)
            await DisplayAlert("Ошибка", "Эта операция не поддерживается для карточек с ответственным лицом. Используйте 'Изменить ответственное лицо'.", "OK");
        }

        private async Task ReleaseMaterialResponsibilityAsync(MaterialDisplayItem displayItem)
        {
            var material = displayItem.Material;
            
            // Работаем только с карточками, где есть ответственное лицо
            if (displayItem.Responsibility == null || !displayItem.Responsibility.WarehouseId.HasValue)
            {
                await DisplayAlert("Ошибка", "Эта операция доступна только для карточек с ответственным лицом и складом.", "OK");
                return;
            }

            var warehouseId = displayItem.Responsibility.WarehouseId.Value;
            var userId = displayItem.Responsibility.UserId;
            var availableQuantity = displayItem.Responsibility.Quantity ?? 0;
            var unit = displayItem.Responsibility.MeasuringUnit ?? material.MeasuringUnit ?? "ед.";

            if (availableQuantity <= 0)
            {
                await DisplayAlert("Ошибка", "Нет ответственности для снятия.", "OK");
                return;
            }

            var quantityText = await DisplayPromptAsync(
                "Снять ответственность",
                $"Доступно для снятия: {availableQuantity} {unit}\nВведите количество для снятия (оставьте пустым для снятия всей ответственности):",
                "Снять",
                "Отмена",
                availableQuantity.ToString(),
                -1,
                Keyboard.Numeric);

            if (quantityText == null)
            {
                return;
            }

            double? quantityToRelease = null;
            if (!string.IsNullOrWhiteSpace(quantityText))
            {
                if (!double.TryParse(quantityText, out var qty) || qty <= 0 || qty > availableQuantity)
                {
                    await DisplayAlert("Ошибка", $"Введите число от 1 до {availableQuantity}.", "OK");
                    return;
                }
                quantityToRelease = qty;
            }

            var confirm = await DisplayAlert(
                "Подтверждение",
                quantityToRelease.HasValue
                    ? $"Снять ответственность в количестве {quantityToRelease} {unit}?"
                    : $"Снять всю ответственность ({availableQuantity} {unit})?",
                "Снять",
                "Отмена");
            
            if (!confirm)
            {
                return;
            }

            var response = await _viewModel.ApiService.ReleaseMaterialResponsibilityFillingAsync(
                warehouseId, material.Id, userId, quantityToRelease);
            
            if (!response.IsSuccess)
            {
                await DisplayAlert("Ошибка", response.Message ?? "Произошла ошибка", "OK");
                return;
            }

            await DisplayAlert("Успех", "Ответственность снята", "OK");
            await _viewModel.LoadMaterialsAsync();
        }

        private async Task<(string? code, bool cancelled)> GetCodeAsync(string title, string? existingCode)
        {
#if ANDROID || IOS
            var action = await DisplayActionSheet(
                "Ввод артикула (кода)",
                "Отмена",
                null,
                "Ввести вручную",
                "Сканировать QR-код");

            if (action == "Отмена" || action == null)
                return (existingCode, true);

            if (action == "Сканировать QR-код")
            {
                var tcs = new TaskCompletionSource<string?>();

                var scannerPage = new BarcodeScannerPage();

                void OnBarcodeScanned(object? sender, string barcodeValue)
                {
                    scannerPage.BarcodeScanned -= OnBarcodeScanned;
                    tcs.TrySetResult(barcodeValue);
                }

                scannerPage.BarcodeScanned += OnBarcodeScanned;
                scannerPage.Disappearing += (s, e) =>
                {
                    tcs.TrySetResult(null);
                };

                await Navigation.PushModalAsync(scannerPage);

                var scannedCode = await tcs.Task;

                if (!string.IsNullOrWhiteSpace(scannedCode))
                {
                    return (scannedCode, false);
                }

                // Если сканирование не удалось, предложить ввести вручную
                var manualCode = await DisplayPromptAsync(title, "Код (обязательно):", "Далее", "Отмена", "Код", -1, Keyboard.Default, existingCode ?? "");
                if (manualCode == null)
                    return (existingCode, true);
                return (manualCode, false);
            }
#endif
            // Ручной ввод
            var code = await DisplayPromptAsync(title, "Код (обязательно):", "Далее", "Отмена", "Код", -1, Keyboard.Default, existingCode ?? "");
            if (code == null)
                return (existingCode, true);
            return (code, false);
        }

        private async Task ShowMaterialDialogAsync(Material? existingMaterial)
        {
            bool isEdit = existingMaterial != null;
            string title = isEdit ? "Редактирование материала" : "Создание материала";

            var name = await DisplayPromptAsync(title, "Название:", "Далее", "Отмена", "Название", -1, Keyboard.Default, existingMaterial?.Name ?? "");
            if (string.IsNullOrWhiteSpace(name))
                return;

            var (code, cancelled) = await GetCodeAsync(title, existingMaterial?.Code);
            if (cancelled)
                return;
            if (string.IsNullOrWhiteSpace(code))
            {
                await DisplayAlert("Ошибка", "Код обязателен для заполнения.", "OK");
                return;
            }
            
            var description = await DisplayPromptAsync(title, "Описание (необязательно):", "Далее", "Отмена", "Описание", -1, Keyboard.Default, existingMaterial?.Description ?? "");
            if (description == null)
                return;
            
            var unitOptions = new[] { "шт", "кг", "г", "л", "мл", "м", "см" };
            var currentUnit = existingMaterial?.MeasuringUnit ?? "шт";
            var measuringUnit = await DisplayActionSheet($"Единица измерения (текущая: {currentUnit}):", "Отмена", null, unitOptions);
            if (measuringUnit == "Отмена" || string.IsNullOrEmpty(measuringUnit))
                return;
            if (string.IsNullOrWhiteSpace(measuringUnit))
                measuringUnit = "шт";

            var material = existingMaterial ?? new Material();
            material.Name = name;
            material.Code = code;
            material.Description = description;
            material.MeasuringUnit = measuringUnit;

            bool success;
            if (isEdit)
            {
                success = await _viewModel.UpdateMaterialAsync(existingMaterial!.Id, material);
            }
            else
            {
                var selectedWarehouse = await SelectWarehouseAsync();
                if (selectedWarehouse == null)
                {
                    return;
                }

                var quantityText = await DisplayPromptAsync(
                    title,
                    "Количество (по умолчанию 0):",
                    "Далее",
                    "Отмена",
                    "0",
                    -1,
                    Keyboard.Numeric);

                if (quantityText == null)
                {
                    return;
                }

                var quantity = 0;
                if (!string.IsNullOrWhiteSpace(quantityText) && !int.TryParse(quantityText, out quantity))
                {
                    await DisplayAlert("Ошибка", "Количество должно быть числом.", "OK");
                    return;
                }

                if (quantity == 0)
                {
                    var confirmZero = await DisplayAlert(
                        "Подтверждение",
                        "Количество = 0. Оставить так?",
                        "Да",
                        "Изменить");
                    if (!confirmZero)
                    {
                        return;
                    }
                }

                // Передаем quantity, measuringUnit и warehouseId: на сервере создаются материал, Responsibility, FillingWarehouse и ResponsibilityFilling
                var createResponse = await _viewModel.ApiService.AddMaterialAsync(
                    material,
                    quantity > 0 ? quantity : null,
                    material.MeasuringUnit,
                    quantity > 0 ? selectedWarehouse.Id : null);
                var createdMaterial = createResponse.Material ?? createResponse.GetData();
                if (createdMaterial == null)
                {
                    await DisplayAlert("Ошибка", createResponse.Message ?? "Ошибка создания материала", "OK");
                    return;
                }

                if (quantity <= 0)
                {
                    var filling = new FillingWarehouse
                    {
                        WarehouseId = selectedWarehouse.Id,
                        MaterialId = createdMaterial.Id,
                        Quantity = 0,
                        MeasuringType = createdMaterial.MeasuringUnit
                    };
                    await _viewModel.ApiService.AddFillingWarehouseAsync(filling);
                }

                await _viewModel.LoadMaterialsAsync();
                success = true;
            }

            if (success)
            {
                await DisplayAlert("Успех", isEdit ? "Материал успешно обновлен" : "Материал успешно создан", "OK");
            }
            else
            {
                await DisplayAlert("Ошибка", _viewModel.ErrorMessage, "OK");
            }
        }

        private async Task<Warehouse?> SelectWarehouseAsync()
        {
            var activeWarehouses = await GetActiveWarehousesAsync();
            if (activeWarehouses.Count == 0)
            {
                await DisplayAlert("Ошибка", "Нет активных складов для привязки материала.", "OK");
                return null;
            }

            var options = activeWarehouses
                .Select(w => $"{w.Name} ({w.Type})")
                .ToArray();

            var choice = await DisplayActionSheet("Выберите склад", "Отмена", null, options);
            if (string.IsNullOrWhiteSpace(choice) || choice == "Отмена")
            {
                return null;
            }

            var index = Array.IndexOf(options, choice);
            if (index < 0 || index >= activeWarehouses.Count)
            {
                return null;
            }

            return activeWarehouses[index];
        }

        private async Task<List<Warehouse>> GetActiveWarehousesAsync()
        {
            var warehouses = await _viewModel.ApiService.GetAllWarehousesAsync();
            return warehouses.Where(w => w.IsActive).ToList();
        }

        private async Task SendMaterialToSDHAsync(MaterialDisplayItem displayItem)
        {
            var material = displayItem.Material;

            // Работаем только с карточками, где есть ответственное лицо и склад
            if (displayItem.Responsibility == null || !displayItem.Responsibility.WarehouseId.HasValue)
            {
                await DisplayAlert("Ошибка", "Эта операция доступна только для карточек с ответственным лицом и складом.", "OK");
                return;
            }

            var warehouseId = displayItem.Responsibility.WarehouseId.Value;
            var currentQuantity = displayItem.Responsibility.Quantity ?? 0;
            var unit = displayItem.Responsibility.MeasuringUnit ?? material.MeasuringUnit ?? "ед.";

            if (currentQuantity <= 0)
            {
                await DisplayAlert("Ошибка", "Нет доступного количества для отправки на СДХ.", "OK");
                return;
            }

            // Запрашиваем количество
            var quantityText = await DisplayPromptAsync(
                "Отправить на СДХ",
                $"Материал: {material.Name}\nТекущее количество: {currentQuantity} {unit}\nВведите количество для отправки:",
                "Отправить",
                "Отмена",
                currentQuantity.ToString(),
                -1,
                Keyboard.Numeric);

            if (quantityText == null)
            {
                return;
            }

            if (!double.TryParse(quantityText, out var quantity) || quantity <= 0)
            {
                await DisplayAlert("Ошибка", "Количество должно быть положительным числом.", "OK");
                return;
            }

            if (quantity > currentQuantity)
            {
                await DisplayAlert("Ошибка", $"Количество не может превышать доступное ({currentQuantity} {unit}).", "OK");
                return;
            }

            // Находим склад СДХ
            var warehouses = await _viewModel.ApiService.GetAllWarehousesAsync();
            var sdhWarehouse = warehouses.FirstOrDefault(w =>
                w.IsActive && (w.Type?.Equals("СДХ", StringComparison.OrdinalIgnoreCase) == true ||
                               w.Name.Contains("СДХ", StringComparison.OrdinalIgnoreCase)));

            if (sdhWarehouse == null)
            {
                await DisplayAlert("Ошибка", "Склад СДХ не найден. Создайте склад с типом 'СДХ'.", "OK");
                return;
            }

            // Подтверждение
            var confirm = await DisplayAlert("Подтверждение",
                $"Отправить материал на СДХ?\n\nМатериал: {material.Name}\nКоличество: {quantity} {unit}\nНа склад: {sdhWarehouse.Name}",
                "Отправить",
                "Отмена");

            if (!confirm)
                return;

            // Создаем запрос
            var response = await _viewModel.ApiService.CreateSDHRequestAsync(
                warehouseId,
                sdhWarehouse.Id,
                material.Id,
                null,
                quantity,
                unit);

            if (!response.IsSuccess)
            {
                await DisplayAlert("Ошибка", response.Message ?? "Не удалось создать запрос на отправку на СДХ", "OK");
                return;
            }

            await DisplayAlert("Успех", "Запрос на отправку на СДХ создан. Материал перемещен на склад СДХ, ответственность осталась у вас до подтверждения менеджером СДХ.", "OK");
            await _viewModel.LoadMaterialsAsync();
        }
    }
}

