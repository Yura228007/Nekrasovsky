using NekrasovskyAPP.ViewModels;
using NekrasovskyAPP.Models;

namespace NekrasovskyAPP.Pages;

public partial class ProductCatalogPage : ContentPage
{
    private readonly MainViewModel _viewModel;
    private bool _permissionsChecked;
    private string _lastSearchText = string.Empty;

    public ProductCatalogPage(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_permissionsChecked)
        {
            await UpdateToolbarPermissionsAsync();
            _permissionsChecked = true;
        }

        await ApplyFiltersAsync();
    }

    private async Task UpdateToolbarPermissionsAsync()
    {
        var canAdd = await _viewModel.CanAddOrEditItemsAsync();

        if (!canAdd && ToolbarItems.Contains(AddToolbarItem))
        {
            ToolbarItems.Remove(AddToolbarItem);
        }
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        try
        {
            await ApplyFiltersAsync();
        }
        finally
        {
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

        if (string.IsNullOrWhiteSpace(searchText) &&
            (StatusPicker.SelectedIndex <= 0) &&
            (SortPicker.SelectedIndex < 0))
        {
            await _viewModel.LoadProductsForCatalogAsync();
            return;
        }

        string? name = null;
        string? code = null;
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var searchTerm = searchText.Trim();
            name = searchTerm;
            code = searchTerm;
        }

        bool? isActive = null;
        if (StatusPicker.SelectedIndex == 1)
            isActive = true;
        else if (StatusPicker.SelectedIndex == 2)
            isActive = false;

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

        await _viewModel.SearchProductsForCatalogAsync(name, code, isActive, sortBy);
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        await ShowProductDialogAsync(null);
    }

    private async void OnProductSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Product selectedProduct)
        {
            var canEdit = await _viewModel.CanAddOrEditItemsAsync();
            var canDelete = await _viewModel.CanDeleteItemsAsync();

            var actions = new List<string> { "Просмотр" };

            if (canEdit)
            {
                actions.Add("Редактировать");
            }
            if (canDelete)
            {
                actions.Add("Удалить");
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
                        $"Артикул: {selectedProduct.Code ?? "Не указан"}\n" +
                        $"Описание: {selectedProduct.Description ?? "Не указано"}\n" +
                        $"Единица измерения: {selectedProduct.MeasuringUnit}\n" +
                        $"Статус: {(selectedProduct.IsActive ? "Активен" : "Неактивен")}",
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

        var code = await DisplayPromptAsync(title, "Артикул (обязательно):", "Далее", "Отмена", "Артикул", -1, Keyboard.Default, existingProduct?.Code ?? "");
        if (string.IsNullOrWhiteSpace(code))
        {
            await DisplayAlert("Ошибка", "Артикул обязателен для заполнения.", "OK");
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
}
