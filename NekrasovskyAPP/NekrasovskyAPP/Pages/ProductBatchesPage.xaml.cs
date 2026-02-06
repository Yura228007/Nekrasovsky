using NekrasovskyAPP.ViewModels;
using NekrasovskyAPP.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;

namespace NekrasovskyAPP.Pages;

public partial class ProductBatchesPage : ContentPage
{
    private readonly MainViewModel _viewModel;
    private string _lastSearchText = string.Empty;
    private List<ProductBatch> _allBatches = new();

    public ObservableCollection<ProductBatch> ProductBatches { get; } = new();

    public ProductBatchesPage(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadWarehousesAsync();
        await LoadUsersAsync();
        await LoadBatchesAsync();
    }

    private async Task LoadBatchesAsync()
    {
        try
        {
            _viewModel.IsLoading = true;
            var batches = await _viewModel.ApiService.GetAllProductBatchesAsync();
            var filtered = await _viewModel.FilterProductBatchesByResponsibilityAsync(batches);
            _allBatches = filtered.ToList();
            FilterBatches();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось загрузить партии: {ex.Message}", "OK");
        }
        finally
        {
            _viewModel.IsLoading = false;
        }
    }

    private async Task LoadWarehousesAsync()
    {
        if (_viewModel.Warehouses.Count == 0)
        {
            await _viewModel.LoadWarehousesAsync();
        }
        WarehousePicker.ItemsSource = null;
        WarehousePicker.ItemsSource = _viewModel.Warehouses;
    }

    private async Task LoadUsersAsync()
    {
        if (_viewModel.Users.Count == 0)
        {
            await _viewModel.LoadUsersAsync();
        }
        UserPicker.Items.Clear();
        UserPicker.Items.Add("Все");
        foreach (var user in _viewModel.Users)
        {
            UserPicker.Items.Add($"{user.Surname} {user.Name}");
        }
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        try
        {
            await LoadBatchesAsync();
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
        await LoadBatchesAsync();
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        var searchText = e.NewTextValue ?? string.Empty;
        if (searchText == _lastSearchText)
            return;
        _lastSearchText = searchText;
        FilterBatches();
    }

    private void OnFilterChanged(object? sender, EventArgs e)
    {
        FilterBatches();
    }

    private void FilterBatches()
    {
        var searchText = (_lastSearchText ?? string.Empty).Trim();
        var statusIndex = StatusPicker.SelectedIndex;
        var sortIndex = SortPicker.SelectedIndex;
        var warehouse = WarehousePicker.SelectedItem as Warehouse;
        User? selectedUser = null;
        if (UserPicker.SelectedIndex > 0 && UserPicker.SelectedIndex <= _viewModel.Users.Count)
        {
            selectedUser = _viewModel.Users[UserPicker.SelectedIndex - 1];
        }

        var filtered = _allBatches.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var term = searchText.ToLowerInvariant();
            filtered = filtered.Where(b =>
                (b.Product?.Name?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (b.Product?.Code?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        if (warehouse != null)
        {
            filtered = filtered.Where(b => b.WarehouseId == warehouse.Id);
        }

        if (selectedUser != null)
        {
            filtered = filtered.Where(b => b.CreatedByUserId == selectedUser.Id);
        }

        if (statusIndex == 1)
            filtered = filtered.Where(b => b.IsActive);
        else if (statusIndex == 2)
            filtered = filtered.Where(b => !b.IsActive);

        filtered = sortIndex switch
        {
            0 => filtered.OrderBy(b => b.Product?.Name ?? ""),
            1 => filtered.OrderByDescending(b => b.Product?.Name ?? ""),
            2 => filtered.OrderByDescending(b => b.CreatedAt),
            3 => filtered.OrderBy(b => b.CreatedAt),
            4 => filtered.OrderByDescending(b => b.Quantity),
            _ => filtered.OrderByDescending(b => b.CreatedAt)
        };

        var list = filtered.ToList();
        ProductBatches.Clear();
        foreach (var batch in list)
        {
            ProductBatches.Add(batch);
        }
    }

    private async void OnBatchSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not ProductBatch selectedBatch)
        {
            BatchesCollectionView.SelectedItem = null;
            return;
        }

        var canDelete = await _viewModel.CanDeleteItemsAsync();
        var canManageResponsibility = await _viewModel.HasManageResponsibilityAsync();

        var actions = new List<string> { "Просмотр" };
        if (canManageResponsibility)
        {
            actions.Add("Изменить ответственное лицо");
            actions.Add("Снять ответственность");
        }
        if (canDelete)
            actions.Add("Удалить");

        var action = await DisplayActionSheet(
            $"Партия: {selectedBatch.DisplayName}",
            "Отмена",
            null,
            actions.ToArray());

        switch (action)
        {
            case "Просмотр":
                await DisplayAlert("Информация о партии",
                    $"Продукт: {selectedBatch.Product?.Name ?? "—"}\n" +
                    $"Артикул: {selectedBatch.ProductCode ?? "—"}\n" +
                    $"Номер партии: {selectedBatch.BatchNumber}\n" +
                    $"Количество: {selectedBatch.QuantityDisplay}\n" +
                    $"Склад: {selectedBatch.WarehouseDisplay}\n" +
                    $"Ответственный: {selectedBatch.CreatedByDisplay}\n" +
                    $"Создано: {selectedBatch.CreatedAt:dd.MM.yyyy HH:mm}\n" +
                    $"Примечание: {selectedBatch.Note ?? "—"}",
                    "OK");
                break;
            case "Изменить ответственное лицо":
                await ChangeBatchResponsibleAsync(selectedBatch);
                break;
            case "Снять ответственность":
                await ReleaseBatchResponsibilityAsync(selectedBatch);
                break;
            case "Удалить":
                var confirm = await DisplayAlert(
                    "Подтверждение удаления",
                    $"Вы уверены, что хотите удалить партию {selectedBatch.DisplayName}?",
                    "Удалить",
                    "Отмена");
                if (confirm)
                {
                    var success = await _viewModel.DeleteProductBatchAsync(selectedBatch.Id);
                    if (!success)
                        await DisplayAlert("Ошибка", _viewModel.ErrorMessage, "OK");
                    else
                    {
                        await LoadBatchesAsync();
                        await DisplayAlert("Успех", "Партия успешно удалена", "OK");
                    }
                }
                break;
        }

        BatchesCollectionView.SelectedItem = null;
    }

    private async Task ChangeBatchResponsibleAsync(ProductBatch batch)
    {
        if (_viewModel.Users.Count == 0)
            await _viewModel.LoadUsersAsync();
        if (_viewModel.Users.Count == 0)
        {
            await DisplayAlert("Ошибка", "Нет доступных пользователей для назначения ответственности.", "OK");
            return;
        }

        var fromUserId = batch.CreatedByUserId;
        if (!fromUserId.HasValue)
        {
            await DisplayAlert("Ошибка", "У партии нет назначенного ответственного. Сначала назначьте ответственного при создании партии или через API.", "OK");
            return;
        }

        var options = _viewModel.Users.Select(u => $"{u.Surname} {u.Name}").ToArray();
        var choice = await DisplayActionSheet("Выберите ответственное лицо:", "Отмена", null, options);
        if (string.IsNullOrWhiteSpace(choice) || choice == "Отмена")
            return;

        var index = System.Array.IndexOf(options, choice);
        if (index < 0 || index >= _viewModel.Users.Count)
            return;

        var selectedUser = _viewModel.Users[index];
        if (selectedUser.Id == fromUserId.Value)
        {
            await DisplayAlert("Ошибка", "Выберите другого пользователя (не текущего ответственного).", "OK");
            return;
        }

        var currentQty = batch.Quantity;
        var unit = batch.MeasuringUnit ?? "ед.";
        double? quantityToTransfer = null;

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
                if (!double.TryParse(qtyText, out var qty) || qty <= 0 || qty > currentQty)
                {
                    await DisplayAlert("Ошибка", $"Введите число от 1 до {currentQty}.", "OK");
                    return;
                }
                quantityToTransfer = qty;
            }
        }

        var response = await _viewModel.ApiService.TransferBatchResponsibilityFillingAsync(
            batch.Id, fromUserId.Value, selectedUser.Id, quantityToTransfer);
        if (!response.IsSuccess)
        {
            await DisplayAlert("Ошибка", response.Message ?? "Произошла ошибка", "OK");
            return;
        }
        var msg = quantityToTransfer.HasValue
            ? $"Передано {quantityToTransfer} {unit}. У предыдущего ответственного осталось {currentQty - quantityToTransfer.Value} {unit}."
            : "Вся ответственность передана новому лицу.";
        await DisplayAlert("Успех", msg, "OK");
        await LoadBatchesAsync();
    }

    private async Task ReleaseBatchResponsibilityAsync(ProductBatch batch)
    {
        if (!batch.CreatedByUserId.HasValue)
        {
            await DisplayAlert("Ошибка", "У партии нет назначенного ответственного.", "OK");
            return;
        }

        var confirm = await DisplayAlert(
            "Снять ответственность",
            $"Снять ответственность с партии {batch.DisplayName}?",
            "Снять",
            "Отмена");
        if (!confirm)
            return;

        var response = await _viewModel.ApiService.ReleaseBatchResponsibilityAsync(batch.Id, batch.CreatedByUserId.Value);
        if (!response.IsSuccess)
        {
            await DisplayAlert("Ошибка", response.Message ?? "Произошла ошибка", "OK");
            return;
        }
        await DisplayAlert("Успех", "Ответственность снята", "OK");
        await LoadBatchesAsync();
    }
}
