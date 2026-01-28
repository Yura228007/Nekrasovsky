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

            if (_viewModel.Products?.Any() == false)
            {
                await _viewModel.LoadProductsAsync();
            }
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
            SearchEntry.Text = string.Empty;
            await _viewModel.LoadProductsAsync();
        }

        private async void OnRefreshClicked(object? sender, EventArgs e)
        {
            await _viewModel.LoadProductsAsync();
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
            var searchText = SearchEntry.Text ?? string.Empty;

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
            if (e.CurrentSelection.FirstOrDefault() is Product selectedProduct)
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
                            $"Вы уверены, что хотите удалить продукт {selectedProduct.Name}?",
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
                        await ChangeProductResponsibilityAsync(selectedProduct);
                        break;
                    case "Снять ответственность":
                        await ReleaseProductResponsibilityAsync(selectedProduct);
                        break;
                }

                ProductsCollectionView.SelectedItem = null;
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

        private async Task ChangeProductResponsibilityAsync(Product product)
        {
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
            {
                return;
            }

            var index = Array.IndexOf(options, choice);
            if (index < 0 || index >= _viewModel.Users.Count)
            {
                return;
            }

            var selectedUser = _viewModel.Users[index];
            var response = await _viewModel.ApiService.AssignProductResponsibilityAsync(product.Id, selectedUser.Id);
            if (response.GetData() == null && !string.IsNullOrEmpty(response.Message))
            {
                await DisplayAlert("Ошибка", response.Message, "OK");
                return;
            }

            await DisplayAlert("Успех", "Ответственное лицо обновлено", "OK");
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
            if (!string.IsNullOrEmpty(response.Message) &&
                (response.Message.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                 response.Message.Contains("ошибка", StringComparison.OrdinalIgnoreCase)))
            {
                await DisplayAlert("Ошибка", response.Message, "OK");
                return;
            }

            await DisplayAlert("Успех", "Ответственность снята", "OK");
        }
    }
}

