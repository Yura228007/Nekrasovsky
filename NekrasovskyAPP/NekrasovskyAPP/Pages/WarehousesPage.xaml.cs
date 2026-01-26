using NekrasovskyAPP.ViewModels;
using NekrasovskyAPP.Models;
using System.Linq;

namespace NekrasovskyAPP.Pages
{
    public partial class WarehousesPage : ContentPage
    {
        private readonly MainViewModel _viewModel;
        private bool _permissionsChecked;

        public WarehousesPage(MainViewModel viewModel)
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

            await _viewModel.LoadWarehousesAsync();
        }

        private async Task UpdateToolbarPermissionsAsync()
        {
            // Добавление складов только для привилегированных или с правом ManageRecipes
            var canAdd = await _viewModel.HasManageRecipesPermissionAsync();

            if (!canAdd && ToolbarItems.Contains(AddToolbarItem))
            {
                ToolbarItems.Remove(AddToolbarItem);
            }
        }

        private async void OnRefreshing(object? sender, EventArgs e)
        {
            await _viewModel.LoadWarehousesAsync();
        }

        private async void OnRefreshClicked(object? sender, EventArgs e)
        {
            await _viewModel.LoadWarehousesAsync();
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

            // Если нет поиска и все фильтры не выбраны, загружаем все склады
            if (string.IsNullOrWhiteSpace(searchText) &&
                (TypePicker.SelectedIndex <= 0) &&
                (StatusPicker.SelectedIndex <= 0) &&
                (SortPicker.SelectedIndex < 0))
            {
                await _viewModel.LoadWarehousesAsync();
                return;
            }

            // Получаем имя из строки поиска
            string? name = null;
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var parts = searchText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                name = parts.Length > 0 ? parts[0] : null;
            }

            // Фильтр по типу
            string? type = null;
            if (TypePicker.SelectedIndex > 0 && TypePicker.SelectedItem != null)
            {
                var selectedType = TypePicker.SelectedItem.ToString();
                if (selectedType != "Все")
                    type = selectedType;
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
                    2 => "type",
                    3 => "type_desc",
                    _ => null
                };
            }

            await _viewModel.SearchWarehousesAsync(name, type, isActive, sortBy);
        }

        private async void OnAddClicked(object? sender, EventArgs e)
        {
            await ShowWarehouseDialogAsync(null);
        }

        private async void OnWarehouseSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Warehouse selectedWarehouse)
            {
                var canManage = await _viewModel.HasManageRecipesPermissionAsync();
                var canDelete = await _viewModel.CanDeleteItemsAsync();

                var actions = new List<string> { "Просмотр" };

                if (canManage)
                {
                    actions.Add("Редактировать");
                    var statusAction = selectedWarehouse.IsActive ? "Остановить работу" : "Запустить работу";
                    actions.Add(statusAction);
                }

                if (canDelete)
                {
                    actions.Add("Удалить");
                }

                var action = await DisplayActionSheet(
                    $"Склад: {selectedWarehouse.Name}",
                    "Отмена",
                    null,
                    actions.ToArray());

                switch (action)
                {
                    case "Просмотр":
                        var statusText = selectedWarehouse.IsActive ? "Работает" : "Остановлен";
                        await DisplayAlert("Информация о складе",
                            $"Название: {selectedWarehouse.Name}\n" +
                            $"Тип: {selectedWarehouse.Type}\n" +
                            $"Статус: {statusText}",
                            "OK");
                        break;

                    case "Редактировать":
                        await ShowWarehouseDialogAsync(selectedWarehouse);
                        break;

                    case "Остановить работу":
                        var hasStock = await HasWarehouseStockAsync(selectedWarehouse.Id);
                        var stopMessage = hasStock
                            ? $"На складе {selectedWarehouse.Name} есть остатки. Если остановить склад, с этими остатками нельзя будет взаимодействовать. Остановить склад?"
                            : $"Вы уверены, что хотите остановить работу склада {selectedWarehouse.Name}?";
                        var stopConfirm = await DisplayAlert(
                            "Подтверждение остановки",
                            stopMessage,
                            "Остановить",
                            "Отмена");

                        if (stopConfirm)
                        {
                            var success = await _viewModel.StopWarehouseAsync(selectedWarehouse.Id);
                            if (!success)
                            {
                                await DisplayAlert("Ошибка", _viewModel.ErrorMessage, "OK");
                            }
                            else
                            {
                                await DisplayAlert("Успех", "Работа склада остановлена", "OK");
                            }
                        }
                        break;

                    case "Запустить работу":
                        var startConfirm = await DisplayAlert(
                            "Подтверждение запуска",
                            $"Вы уверены, что хотите запустить работу склада {selectedWarehouse.Name}?",
                            "Запустить",
                            "Отмена");

                        if (startConfirm)
                        {
                            var success = await _viewModel.StartWarehouseAsync(selectedWarehouse.Id);
                            if (!success)
                            {
                                await DisplayAlert("Ошибка", _viewModel.ErrorMessage, "OK");
                            }
                            else
                            {
                                await DisplayAlert("Успех", "Работа склада запущена", "OK");
                            }
                        }
                        break;

                    case "Удалить":
                        var confirm = await DisplayAlert(
                            "Подтверждение удаления",
                            $"Вы уверены, что хотите удалить склад {selectedWarehouse.Name}?",
                            "Удалить",
                            "Отмена");

                        if (confirm)
                        {
                            var success = await _viewModel.DeleteWarehouseAsync(selectedWarehouse.Id);
                            if (!success)
                            {
                                await DisplayAlert("Ошибка", _viewModel.ErrorMessage, "OK");
                            }
                            else
                            {
                                await DisplayAlert("Успех", "Склад успешно удален", "OK");
                            }
                        }
                        break;
                }

                WarehousesCollectionView.SelectedItem = null;
            }
        }

        private async Task ShowWarehouseDialogAsync(Warehouse? existingWarehouse)
        {
            bool isEdit = existingWarehouse != null;
            string title = isEdit ? "Редактирование склада" : "Создание склада";

            var name = await DisplayPromptAsync(title, "Название склада:", "Далее", "Отмена", "Название", -1, Keyboard.Default, existingWarehouse?.Name ?? "");
            if (string.IsNullOrWhiteSpace(name))
                return;

            var typeOptions = new[] { "Цех", "Склад", "Производство", "Готовой продукции", "Сырья" };
            var currentTypeIndex = Array.IndexOf(typeOptions, existingWarehouse?.Type ?? "Цех");
            if (currentTypeIndex < 0) currentTypeIndex = 0;

            var type = await DisplayActionSheet("Выберите тип склада:", "Отмена", null, typeOptions);
            if (type == "Отмена" || string.IsNullOrEmpty(type))
                return;

            var warehouse = existingWarehouse ?? new Warehouse();
            warehouse.Name = name;
            warehouse.Type = type;

            bool success;
            if (isEdit)
            {
                success = await _viewModel.UpdateWarehouseAsync(existingWarehouse!.Id, warehouse);
            }
            else
            {
                success = await _viewModel.CreateWarehouseAsync(warehouse);
            }

            if (success)
            {
                await DisplayAlert("Успех", isEdit ? "Склад успешно обновлен" : "Склад успешно создан", "OK");
            }
            else
            {
                await DisplayAlert("Ошибка", _viewModel.ErrorMessage, "OK");
            }
        }

        private async Task<bool> HasWarehouseStockAsync(int warehouseId)
        {
            try
            {
                var fillings = await _viewModel.ApiService.GetFillingsByWarehouseAsync(warehouseId);
                return fillings.Any(f => f.Quantity > 0);
            }
            catch
            {
                return false;
            }
        }
    }
}

