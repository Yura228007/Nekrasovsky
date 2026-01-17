using NekrasovskyAPP.ViewModels;
using NekrasovskyAPP.Models;

namespace NekrasovskyAPP.Pages
{
    public partial class ProductsPage : ContentPage
    {
        private readonly MainViewModel _viewModel;

        public ProductsPage(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadProductsAsync();
        }

        private async void OnRefreshing(object? sender, EventArgs e)
        {
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

            if (string.IsNullOrWhiteSpace(searchText))
            {
                await _viewModel.LoadProductsAsync();
            }
            else
            {
                var parts = searchText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var name = parts.Length > 0 ? parts[0] : null;
                var code = parts.Length > 1 ? parts[1] : null;
                
                await _viewModel.SearchProductsAsync(name, code);
            }
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
                var action = await DisplayActionSheet(
                    $"Продукт: {selectedProduct.Name}",
                    "Отмена",
                    null,
                    "Просмотр",
                    "Редактировать",
                    "Удалить");

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
                }

                ProductsCollectionView.SelectedItem = null;
            }
        }

        private async Task ShowProductDialogAsync(Product? existingProduct)
        {
            bool isEdit = existingProduct != null;
            string title = isEdit ? "Редактирование продукта" : "Создание продукта";

            var name = await DisplayPromptAsync(title, "Название:", "Далее", "Отмена", "Название", -1, Keyboard.Default, existingProduct?.Name ?? "");
            if (string.IsNullOrWhiteSpace(name))
                return;

            var code = await DisplayPromptAsync(title, "Код (необязательно):", "Далее", "Отмена", "Код", -1, Keyboard.Default, existingProduct?.Code ?? "");
            
            var description = await DisplayPromptAsync(title, "Описание (необязательно):", "Далее", "Отмена", "Описание", -1, Keyboard.Default, existingProduct?.Description ?? "");
            
            var measuringUnit = await DisplayPromptAsync(title, "Единица измерения (шт, кг, л и т.д.):", "Сохранить", "Отмена", "Единица измерения", -1, Keyboard.Default, existingProduct?.MeasuringUnit ?? "шт");
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
    }
}

