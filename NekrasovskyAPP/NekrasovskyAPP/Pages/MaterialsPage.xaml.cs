using NekrasovskyAPP.ViewModels;
using NekrasovskyAPP.Models;
using System.Linq;

namespace NekrasovskyAPP.Pages
{
    public partial class MaterialsPage : ContentPage
    {
        private readonly MainViewModel _viewModel;

        public MaterialsPage(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadMaterialsAsync();
        }

        private async void OnRefreshing(object? sender, EventArgs e)
        {
            await _viewModel.LoadMaterialsAsync();
        }

        private async void OnRefreshClicked(object? sender, EventArgs e)
        {
            await _viewModel.LoadMaterialsAsync();
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
            var searchText = SearchEntry.Text ?? string.Empty;

            // Если нет поиска и все фильтры не выбраны, загружаем все материалы
            if (string.IsNullOrWhiteSpace(searchText) &&
                (StatusPicker.SelectedIndex <= 0) &&
                (SortPicker.SelectedIndex < 0))
            {
                await _viewModel.LoadMaterialsAsync();
                return;
            }

            // Получаем имя и код из строки поиска
            string? name = null;
            string? code = null;
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var parts = searchText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                name = parts.Length > 0 ? parts[0] : null;
                code = parts.Length > 1 ? parts[1] : null;
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
            if (e.CurrentSelection.FirstOrDefault() is Material selectedMaterial)
            {
                var action = await DisplayActionSheet(
                    $"Материал: {selectedMaterial.Name}",
                    "Отмена",
                    null,
                    "Просмотр",
                    "Редактировать",
                    "Изменить количество",
                    "Удалить");

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
                }

                MaterialsCollectionView.SelectedItem = null;
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

            if (_viewModel.Warehouses.Count == 0)
            {
                await _viewModel.LoadWarehousesAsync();
            }

            var warehouseMap = _viewModel.Warehouses.ToDictionary(
                w => w.Id,
                w => $"{w.Name} ({w.Type})");

            var options = fillings.Select(filling =>
            {
                var name = warehouseMap.TryGetValue(filling.WarehouseId, out var label)
                    ? label
                    : $"Склад #{filling.WarehouseId}";
                var unit = string.IsNullOrWhiteSpace(filling.MeasuringType)
                    ? material.MeasuringUnit
                    : filling.MeasuringType;
                return $"{name}: {filling.Quantity} {unit}";
            }).ToArray();

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
            
            var measuringUnit = await DisplayPromptAsync(title, "Единица измерения (шт, кг, л и т.д.):", "Сохранить", "Отмена", "Единица измерения", -1, Keyboard.Default, existingMaterial?.MeasuringUnit ?? "шт");
            if (measuringUnit == null)
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

                var createResponse = await _viewModel.ApiService.AddMaterialAsync(material);
                var createdMaterial = createResponse.Material ?? createResponse.GetData();
                if (createdMaterial == null)
                {
                    await DisplayAlert("Ошибка", createResponse.Message ?? "Ошибка создания материала", "OK");
                    return;
                }

                var filling = new FillingWarehouse
                {
                    WarehouseId = selectedWarehouse.Id,
                    MaterialId = createdMaterial.Id,
                    Quantity = Math.Max(0, quantity),
                    MeasuringType = createdMaterial.MeasuringUnit
                };

                var fillingResponse = await _viewModel.ApiService.AddFillingWarehouseAsync(filling);
                if (fillingResponse.GetData() == null && !string.IsNullOrEmpty(fillingResponse.Message))
                {
                    await DisplayAlert("Ошибка", $"Материал создан, но не удалось привязать склад: {fillingResponse.Message}", "OK");
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
            if (_viewModel.Warehouses.Count == 0)
            {
                await _viewModel.LoadWarehousesAsync();
            }

            if (_viewModel.Warehouses.Count == 0)
            {
                await DisplayAlert("Ошибка", "Нет доступных складов для привязки материала.", "OK");
                return null;
            }

            var options = _viewModel.Warehouses
                .Select(w => $"{w.Name} ({w.Type})")
                .ToArray();

            var choice = await DisplayActionSheet("Выберите склад", "Отмена", null, options);
            if (string.IsNullOrWhiteSpace(choice) || choice == "Отмена")
            {
                return null;
            }

            var index = Array.IndexOf(options, choice);
            if (index < 0 || index >= _viewModel.Warehouses.Count)
            {
                return null;
            }

            return _viewModel.Warehouses[index];
        }
    }
}

