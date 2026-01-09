using NekrasovskyAPP.ViewModels;
using NekrasovskyAPP.Models;

namespace NekrasovskyAPP.Pages
{
    public partial class WarehousesPage : ContentPage
    {
        private readonly MainViewModel _viewModel;

        public WarehousesPage(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadWarehousesAsync();
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

            if (string.IsNullOrWhiteSpace(searchText))
            {
                await _viewModel.LoadWarehousesAsync();
            }
            else
            {
                var parts = searchText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var name = parts.Length > 0 ? parts[0] : null;
                var type = parts.Length > 1 ? parts[1] : null;
                
                await _viewModel.SearchWarehousesAsync(name, type);
            }
        }

        private async void OnAddClicked(object? sender, EventArgs e)
        {
            await ShowWarehouseDialogAsync(null);
        }

        private async void OnWarehouseSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Warehouse selectedWarehouse)
            {
                var action = await DisplayActionSheet(
                    $"Склад: {selectedWarehouse.Name}",
                    "Отмена",
                    null,
                    "Просмотр",
                    "Редактировать",
                    "Удалить");

                switch (action)
                {
                    case "Просмотр":
                        await DisplayAlert("Информация о складе",
                            $"Название: {selectedWarehouse.Name}\n" +
                            $"Тип: {selectedWarehouse.Type}",
                            "OK");
                        break;

                    case "Редактировать":
                        await ShowWarehouseDialogAsync(selectedWarehouse);
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
    }
}

