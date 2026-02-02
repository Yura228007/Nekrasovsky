using NekrasovskyAPP.ViewModels;
using NekrasovskyAPP.Models;
using System.Collections.Generic;

namespace NekrasovskyAPP.Pages
{
    public partial class ProductsPage : ContentPage
    {
        private readonly MainViewModel _viewModel;
        private bool _permissionsChecked;

        public ProductsPage(MainViewModel viewModel)
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

        private async Task ApplyFiltersAsync()
        {
            var searchText = _lastSearchText ?? string.Empty;

            // Если нет поиска и все фильтры не выбраны, загружаем все продукты
            if (string.IsNullOrWhiteSpace(searchText) &&
                (StatusPicker.SelectedIndex <= 0) &&
                (SortPicker.SelectedIndex < 0))
            {
                await _viewModel.LoadProductsAsync();
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

            await _viewModel.SearchProductsAsync(name, code, isActive, sortBy);
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
                // Обновить поле поиска с найденным штрих-кодом
                SearchEntry.Text = barcodeValue;
                
                // Выполнить поиск по штрих-коду
                await _viewModel.SearchProductsAsync(null, barcodeValue);
                
                await DisplayAlert(
                    "Штрих-код найден",
                    $"Найден штрих-код: {barcodeValue}\nВыполняется поиск...",
                    "OK");
            }
        }

        private async void OnAddClicked(object? sender, EventArgs e)
        {
            await ShowProductDialogAsync(null);
        }

        private async void OnProductSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is ProductDisplayItem displayItem)
            {
                var selectedProduct = displayItem.Product;
                {
                    var isPrivileged = await _viewModel.IsPrivilegedUserAsync();
                    var canDelete = await _viewModel.CanDeleteItemsAsync();
                    var canEdit = await _viewModel.CanAddOrEditItemsAsync();

                    var actions = new List<string> { "Просмотр" };

                    if (canEdit)
                    {
                        actions.Add("Редактировать");
                    }
                    if (canDelete)
                    {
                        actions.Add("Удалить");
                    }
                    if (isPrivileged)
                    {
                        actions.Add("Изменить ответственное лицо");
                        actions.Add("Снять ответственность");
                    }

                    var action = await DisplayActionSheet(
                        $"Продукт: {selectedProduct.Name}",
                        "Отмена",
                        null,
                        actions.ToArray());

                    switch (action)
                    {
                        case "Просмотр":
                            await DisplayAlert("Информация о продукте",
                                $"Название: {selectedProduct.Name}\n" +
                                $"Код: {selectedProduct.Code ?? "Не указан"}\n" +
                                $"Описание: {selectedProduct.Description ?? "Не указано"}\n" +
                                $"Единица измерения: {selectedProduct.MeasuringUnit}",
                                "OK");
                            break;

                        case "Редактировать":
                            await ShowProductDialogAsync(selectedProduct);
                            break;

                        case "Удалить":
                            var confirm = await DisplayAlert(
                                "Подтверждение удаления",
                                $"История, связанная с этим продуктом, исчезнет (остатки, ответственности, партии по продукту и т.д.). Продолжить удаление продукта «{selectedProduct.Name}»?",
                                "Удалить",
                                "Отмена");

                            if (confirm)
                            {
                                var success = await _viewModel.DeleteProductAsync(selectedProduct.Id);
                                if (!success)
                                {
                                    await DisplayAlert("Ошибка", _viewModel.ErrorMessage, "OK");
                                }
                                else
                                {
                                    await DisplayAlert("Успех", "Продукт успешно удален", "OK");
                                }
                            }
                            break;
                        case "Изменить ответственное лицо":
                            await ChangeProductResponsibilityAsync(displayItem);
                            break;
                        case "Снять ответственность":
                            await ReleaseProductResponsibilityAsync(selectedProduct);
                            break;
                    }

                    ProductsCollectionView.SelectedItem = null;
                }
            }
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

        private async Task ShowProductDialogAsync(Product? existingProduct)
        {
            bool isEdit = existingProduct != null;
            string title = isEdit ? "Редактирование продукта" : "Создание продукта";

            var name = await DisplayPromptAsync(title, "Название:", "Далее", "Отмена", "Название", -1, Keyboard.Default, existingProduct?.Name ?? "");
            if (string.IsNullOrWhiteSpace(name))
                return;

            var (code, cancelled) = await GetCodeAsync(title, existingProduct?.Code);
            if (cancelled)
                return;
            if (string.IsNullOrWhiteSpace(code))
            {
                await DisplayAlert("Ошибка", "Код обязателен для заполнения.", "OK");
                return;
            }
            
            var description = await DisplayPromptAsync(title, "Описание (необязательно):", "Далее", "Отмена", "Описание", -1, Keyboard.Default, existingProduct?.Description ?? "");
            if (description == null)
                return;
            
            var unitOptions = new[] { "шт", "кг", "г", "л", "мл", "м", "см" };
            var currentUnit = existingProduct?.MeasuringUnit ?? "шт";
            var measuringUnit = await DisplayActionSheet($"Единица измерения (текущая: {currentUnit}):", "Отмена", null, unitOptions);
            if (measuringUnit == "Отмена" || string.IsNullOrEmpty(measuringUnit))
                return;
            if (string.IsNullOrWhiteSpace(measuringUnit))
                measuringUnit = "шт";

            var product = existingProduct ?? new Product();
            product.Name = name;
            product.Code = code;
            product.Description = description;
            product.MeasuringUnit = measuringUnit;

            bool success;
            if (isEdit)
            {
                success = await _viewModel.UpdateProductAsync(existingProduct!.Id, product);
            }
            else
            {
                success = await _viewModel.CreateProductAsync(product);
            }

            if (success)
            {
                await DisplayAlert("Успех", isEdit ? "Продукт успешно обновлен" : "Продукт успешно создан", "OK");
            }
            else
            {
                await DisplayAlert("Ошибка", _viewModel.ErrorMessage, "OK");
            }
        }

        private async Task ChangeProductResponsibilityAsync(ProductDisplayItem displayItem)
        {
            var product = displayItem.Product;

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
                var unit = displayItem.Responsibility.MeasuringUnit ?? product.MeasuringUnit ?? "ед.";

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

                var response = await _viewModel.ApiService.TransferProductResponsibilityFillingAsync(
                    warehouseId, product.Id, fromUserId, selectedUser.Id, quantityToTransfer);
                if (!response.IsSuccess)
                {
                    await DisplayAlert("Ошибка", response.Message ?? "Произошла ошибка", "OK");
                    return;
                }
                var msg = quantityToTransfer.HasValue
                    ? $"Передано {quantityToTransfer} {unit}. У предыдущего ответственного осталось {currentQty - quantityToTransfer.Value} {unit}."
                    : "Вся ответственность передана новому лицу.";
                await DisplayAlert("Успех", msg, "OK");
                await _viewModel.LoadProductsAsync();
                return;
            }

            // Назначение без склада (неответственная часть или старая модель Responsibility)
            int? quantity = displayItem.Responsibility == null
                ? displayItem.UnassignedQuantity
                : displayItem.Responsibility.Quantity;
            string? measuringUnit = displayItem.Responsibility == null
                ? (displayItem.UnassignedMeasuringUnit ?? product.MeasuringUnit)
                : (displayItem.Responsibility.MeasuringUnit ?? product.MeasuringUnit);

            if (!quantity.HasValue || quantity <= 0)
            {
                var quantityText = await DisplayPromptAsync(
                    "Количество",
                    "Укажите количество, за которое будет отвечать выбранное лицо.\nОставьте пустым для ответственности за весь продукт.",
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
                        measuringUnit = product.MeasuringUnit;
                    }
                    else
                    {
                        await DisplayAlert("Ошибка", "Количество должно быть положительным числом", "OK");
                        return;
                    }
                }
            }

            var assignResponse = await _viewModel.ApiService.AssignProductResponsibilityAsync(product.Id, selectedUser.Id, quantity, measuringUnit);
            if (!assignResponse.IsSuccess)
            {
                await DisplayAlert("Ошибка", assignResponse.Message ?? "Произошла ошибка", "OK");
                return;
            }
            var message = quantity.HasValue
                ? $"Ответственность передана. Количество: {quantity} {measuringUnit}"
                : "Ответственное лицо обновлено (за весь продукт)";
            await DisplayAlert("Успех", message, "OK");
            await _viewModel.LoadProductsAsync();
        }

        private async Task ReleaseProductResponsibilityAsync(Product product)
        {
            var confirm = await DisplayAlert(
                "Снять ответственность",
                $"Снять ответственность с продукта {product.Name}?",
                "Снять",
                "Отмена");
            if (!confirm)
            {
                return;
            }

            var response = await _viewModel.ApiService.ReleaseProductResponsibilityAsync(product.Id);
            if (!response.IsSuccess)
            {
                await DisplayAlert("Ошибка", response.Message ?? "Произошла ошибка", "OK");
                return;
            }

            await DisplayAlert("Успех", "Ответственность снята", "OK");
            // Перезагружаем продукты для обновления списка
            await _viewModel.LoadProductsAsync();
        }
    }
}

