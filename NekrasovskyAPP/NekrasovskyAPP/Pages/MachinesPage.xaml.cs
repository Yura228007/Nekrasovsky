using NekrasovskyAPP.Models;
using NekrasovskyAPP.Services;

namespace NekrasovskyAPP.Pages
{
    public partial class MachinesPage : ContentPage
    {
        private readonly IApiService _apiService;
        private List<Machine> _machines = new();
        private List<Machine> _filteredMachines = new();
        private List<Warehouse> _warehouses = new();

        public MachinesPage(IApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                LoadingIndicator.IsRunning = true;
                LoadingIndicator.IsVisible = true;

                _machines = await _apiService.GetAllMachinesAsync();
                _warehouses = await _apiService.GetAllWarehousesAsync();
                _filteredMachines = _machines.ToList();

                MachinesCollectionView.ItemsSource = _filteredMachines;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить данные: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
                RefreshViewControl.IsRefreshing = false;
            }
        }

        private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
        {
            FilterMachines();
        }

        private void FilterMachines()
        {
            var searchText = SearchEntry.Text?.Trim().ToLower() ?? "";

            _filteredMachines = _machines.Where(m =>
                (string.IsNullOrEmpty(searchText) ||
                 m.Name.ToLower().Contains(searchText) ||
                 (m.Code?.ToLower().Contains(searchText) ?? false))
            ).ToList();

            MachinesCollectionView.ItemsSource = _filteredMachines;
        }

        private async void OnRefreshClicked(object? sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        private async void OnRefreshing(object? sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        private async void OnAddClicked(object? sender, EventArgs e)
        {
            await ShowMachineDialogAsync(null);
        }

        private async void OnMachineSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Machine selected)
            {
                MachinesCollectionView.SelectedItem = null;

                var action = await DisplayActionSheet(
                    selected.Name,
                    "Отмена",
                    null,
                    "Редактировать",
                    selected.IsActive ? "Деактивировать" : "Активировать",
                    "Удалить");

                switch (action)
                {
                    case "Редактировать":
                        await ShowMachineDialogAsync(selected);
                        break;
                    case "Деактивировать":
                    case "Активировать":
                        await ToggleActiveAsync(selected);
                        break;
                    case "Удалить":
                        await DeleteMachineAsync(selected);
                        break;
                }
            }
        }

        private async Task ShowMachineDialogAsync(Machine? existing)
        {
            var title = existing == null ? "Новый станок" : "Редактировать станок";

            // Name
            var name = await DisplayPromptAsync(
                title,
                "Название станка:",
                "Далее",
                "Отмена",
                existing?.Name ?? "",
                100,
                Keyboard.Text,
                existing?.Name ?? "");
            if (string.IsNullOrWhiteSpace(name))
                return;

            // Code (optional)
            var code = await DisplayPromptAsync(
                title,
                "Код станка (опционально):",
                "Далее",
                "Отмена",
                existing?.Code ?? "",
                50,
                Keyboard.Text,
                existing?.Code ?? "");
            if (code == null) // User pressed cancel
                return;

            // Type (optional)
            var typeOptions = new[] { "Экструдер", "Линия", "Станок", "Пресс", "Другое" };
            var selectedType = await DisplayActionSheet("Тип оборудования:", "Отмена", null, typeOptions);
            if (selectedType == "Отмена")
                return;

            // Warehouse (optional)
            var warehouseOptions = new[] { "Без склада" }.Concat(_warehouses.Where(w => w.IsActive).Select(w => w.Name)).ToArray();
            var selectedWarehouseName = await DisplayActionSheet("Привязать к складу:", "Отмена", null, warehouseOptions);
            if (selectedWarehouseName == "Отмена")
                return;

            var selectedWarehouse = selectedWarehouseName == "Без склада" ? null : _warehouses.FirstOrDefault(w => w.Name == selectedWarehouseName);

            // Description (optional)
            var description = await DisplayPromptAsync(
                title,
                "Описание (опционально):",
                "Сохранить",
                "Отмена",
                existing?.Description ?? "",
                200,
                Keyboard.Text,
                existing?.Description ?? "");
            if (description == null)
                return;

            var machine = new Machine
            {
                Id = existing?.Id ?? 0,
                Name = name.Trim(),
                Code = string.IsNullOrWhiteSpace(code) ? null : code.Trim(),
                Type = selectedType,
                Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                WarehouseId = selectedWarehouse?.Id,
                IsActive = existing?.IsActive ?? true
            };

            try
            {
                LoadingIndicator.IsRunning = true;
                LoadingIndicator.IsVisible = true;

                ApiResponse<Machine> response;
                if (existing == null)
                {
                    response = await _apiService.AddMachineAsync(machine);
                }
                else
                {
                    response = await _apiService.EditMachineAsync(existing.Id, machine);
                }

                if (response.Machine != null)
                {
                    await DisplayAlert("Успех", response.Message, "OK");
                    await LoadDataAsync();
                }
                else
                {
                    await DisplayAlert("Ошибка", response.Message ?? "Не удалось сохранить", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось сохранить: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            }
        }

        private async Task ToggleActiveAsync(Machine machine)
        {
            try
            {
                LoadingIndicator.IsRunning = true;
                LoadingIndicator.IsVisible = true;

                machine.IsActive = !machine.IsActive;
                var response = await _apiService.EditMachineAsync(machine.Id, machine);

                if (response.Machine != null)
                {
                    await DisplayAlert("Успех", $"Станок {(machine.IsActive ? "активирован" : "деактивирован")}", "OK");
                    await LoadDataAsync();
                }
                else
                {
                    machine.IsActive = !machine.IsActive; // Revert
                    await DisplayAlert("Ошибка", response.Message ?? "Не удалось обновить статус", "OK");
                }
            }
            catch (Exception ex)
            {
                machine.IsActive = !machine.IsActive; // Revert
                await DisplayAlert("Ошибка", $"Не удалось обновить: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            }
        }

        private async Task DeleteMachineAsync(Machine machine)
        {
            var confirm = await DisplayAlert(
                "Удаление",
                $"Удалить станок \"{machine.Name}\"?",
                "Удалить",
                "Отмена");

            if (!confirm)
                return;

            try
            {
                LoadingIndicator.IsRunning = true;
                LoadingIndicator.IsVisible = true;

                var response = await _apiService.DeleteMachineAsync(machine.Id);
                await DisplayAlert("Успех", response.Message ?? "Станок удален", "OK");
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось удалить: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            }
        }
    }
}
