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
                            await ShowMaterialQuantityDialogAsync(selectedMaterial);
                            break;

                        case "Удалить":
                            var confirm = await DisplayAlert(
                                "Подтверждение удаления",
                                $"Вы уверены, что хотите удалить материал {selectedMaterial.Name}?",
                                "Удалить",
                                "Отмена");

                            if (confirm)
                            {
                                var success = await _viewModel.DeleteMaterialAsync(selectedMaterial.Id);
                                if (!success)
                                {
                                    await DisplayAlert("Ошибка", _viewModel.ErrorMessage, "OK");
                                }
                                else
                                {
                                    await DisplayAlert("Успех", "Материал успешно удален", "OK");
                                }
                            }
                            break;
                        case "Изменить ответственное лицо":
                            await ChangeMaterialResponsibilityAsync(displayItem);
                            break;
                        case "Снять ответственность":
                            await ReleaseMaterialResponsibilityAsync(selectedMaterial);
                            break;
                    }

                    MaterialsCollectionView.SelectedItem = null;
                }
            }
        }

        private async Task ShowMaterialQuantityDialogAsync(Material material)
        {
            var fillings = await _viewModel.ApiService.GetFillingsByMaterialAsync(material.Id);
            if (fillings.Count == 0)
            {
                await DisplayAlert("Нет остатков", "Для этого материала нет записей по складам.", "OK");
                return;
            }

            var activeWarehouses = await GetActiveWarehousesAsync();
            var warehouseMap = activeWarehouses.ToDictionary(
                w => w.Id,
                w => $"{w.Name} ({w.Type})");

            var options = fillings
                .Where(filling => warehouseMap.ContainsKey(filling.WarehouseId))
                .Select(filling =>
            {
                var name = warehouseMap.TryGetValue(filling.WarehouseId, out var label)
                    ? label
                    : $"Склад #{filling.WarehouseId}";
                var unit = string.IsNullOrWhiteSpace(filling.MeasuringType)
                    ? material.MeasuringUnit
                    : filling.MeasuringType;
                return $"{name}: {filling.Quantity} {unit}";
            }).ToArray();

            if (options.Length == 0)
            {
                await DisplayAlert("Нет доступных складов", "Все склады с остатками остановлены. Нельзя изменять количество.", "OK");
                return;
            }

            var choice = await DisplayActionSheet("Выберите склад", "Отмена", null, options);
            if (string.IsNullOrWhiteSpace(choice) || choice == "Отмена")
            {
                return;
            }

            var index = Array.IndexOf(options, choice);
            if (index < 0 || index >= fillings.Count)
            {
                return;
            }

            var selectedFilling = fillings[index];
            var quantityText = await DisplayPromptAsync(
                "Изменить количество",
                $"Новое количество (сейчас {selectedFilling.Quantity}):",
                "Сохранить",
                "Отмена",
                selectedFilling.Quantity.ToString(),
                -1,
                Keyboard.Numeric);

            if (quantityText == null)
            {
                return;
            }

            if (!int.TryParse(quantityText, out var quantity))
            {
                await DisplayAlert("Ошибка", "Количество должно быть числом.", "OK");
                return;
            }

            if (quantity < 0)
            {
                await DisplayAlert("Ошибка", "Количество не может быть отрицательным.", "OK");
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

            var update = new FillingWarehouse
            {
                WarehouseId = selectedFilling.WarehouseId,
                MaterialId = material.Id,
                Quantity = quantity,
                MeasuringType = string.IsNullOrWhiteSpace(selectedFilling.MeasuringType)
                    ? material.MeasuringUnit
                    : selectedFilling.MeasuringType
            };

            var response = await _viewModel.ApiService.UpdateFillingQuantityByMaterialAsync(update);
            if (response.GetData() == null && !string.IsNullOrEmpty(response.Message))
            {
                await DisplayAlert("Ошибка", response.Message, "OK");
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

            // Количество из карточки: для неответственной части — UnassignedQuantity, иначе — из назначения
            int? quantity = displayItem.Responsibility == null
                ? displayItem.UnassignedQuantity
                : displayItem.Responsibility.Quantity;
            string? measuringUnit = displayItem.Responsibility == null
                ? (displayItem.UnassignedMeasuringUnit ?? material.MeasuringUnit)
                : (displayItem.Responsibility.MeasuringUnit ?? material.MeasuringUnit);

            // Если в карточке нет количества (редкий случай), спрашиваем пользователя
            if (!quantity.HasValue || quantity <= 0)
            {
                var quantityText = await DisplayPromptAsync(
                    "Количество",
                    "Укажите количество, за которое будет отвечать выбранное лицо.\nОставьте пустым для ответственности за весь материал.",
                    "Назначить",
                    "Отмена",
                    "Количество",
                    -1,
                    Keyboard.Numeric);

                if (quantityText == null)
                    return;

                quantity = null;
                measuringUnit = null;
                if (!string.IsNullOrWhiteSpace(quantityText))
                {
                    if (int.TryParse(quantityText, out var qty) && qty > 0)
                    {
                        quantity = qty;
                        measuringUnit = material.MeasuringUnit;
                    }
                    else
                    {
                        await DisplayAlert("Ошибка", "Количество должно быть положительным числом", "OK");
                        return;
                    }
                }
            }

            var options = _viewModel.Users
                .Select(u => $"{u.Surname} {u.Name}")
                .ToArray();

            var choice = await DisplayActionSheet("Выберите ответственное лицо:", "Отмена", null, options);
            if (string.IsNullOrWhiteSpace(choice) || choice == "Отмена")
            {
                return;
            }

            var index = Array.IndexOf(options, choice);
            if (index < 0 || index >= _viewModel.Users.Count)
            {
                return;
            }

            var selectedUser = _viewModel.Users[index];

            var response = await _viewModel.ApiService.AssignMaterialResponsibilityAsync(material.Id, selectedUser.Id, quantity, measuringUnit);
            if (response.GetData() == null && !string.IsNullOrEmpty(response.Message))
            {
                await DisplayAlert("Ошибка", response.Message, "OK");
                return;
            }

            var message = quantity.HasValue
                ? $"Ответственность передана. Количество: {quantity} {measuringUnit}"
                : "Ответственное лицо обновлено (за весь материал)";
            await DisplayAlert("Успех", message, "OK");
            await _viewModel.LoadMaterialsAsync();
        }

        private async Task ReleaseMaterialResponsibilityAsync(Material material)
        {
            var confirm = await DisplayAlert(
                "Снять ответственность",
                $"Снять ответственность с материала {material.Name}?",
                "Снять",
                "Отмена");
            if (!confirm)
            {
                return;
            }

            var response = await _viewModel.ApiService.ReleaseMaterialResponsibilityAsync(material.Id);
            if (!string.IsNullOrEmpty(response.Message) &&
                (response.Message.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                 response.Message.Contains("ошибка", StringComparison.OrdinalIgnoreCase)))
            {
                await DisplayAlert("Ошибка", response.Message, "OK");
                return;
            }

            await DisplayAlert("Успех", "Ответственность снята", "OK");
            // Перезагружаем материалы для обновления списка
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
    }
}

