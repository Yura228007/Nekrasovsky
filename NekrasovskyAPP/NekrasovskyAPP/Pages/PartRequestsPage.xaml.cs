using NekrasovskyAPP.ViewModels;
using NekrasovskyAPP.Models;
using System.Linq;

namespace NekrasovskyAPP.Pages
{
    public partial class PartRequestsPage : ContentPage
    {
        private readonly PartRequestsViewModel _viewModel;

        public PartRequestsPage(PartRequestsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadDependenciesAsync();
            await _viewModel.LoadPartRequestsAsync();
        }

        private async void OnRefreshing(object? sender, EventArgs e)
        {
            await _viewModel.LoadPartRequestsAsync();
        }

        private async void OnRefreshClicked(object? sender, EventArgs e)
        {
            await _viewModel.LoadPartRequestsAsync();
        }

        private async void OnAddClicked(object? sender, EventArgs e)
        {
            await ShowCreatePartRequestDialogAsync();
        }

        private async Task ShowCreatePartRequestDialogAsync()
        {
            if (_viewModel.CurrentUser == null)
            {
                await DisplayAlert("Ошибка", "Пользователь не авторизован", "OK");
                return;
            }

            // Загружаем зависимости, если еще не загружены
            if (!_viewModel.Users.Any() || !_viewModel.Warehouses.Any() || !_viewModel.Materials.Any())
            {
                await _viewModel.LoadDependenciesAsync();
            }

            var currentUser = _viewModel.CurrentUser;
            if (currentUser == null)
            {
                await DisplayAlert("Ошибка", "Пользователь не авторизован", "OK");
                return;
            }

            if (!_viewModel.Warehouses.Any())
            {
                await DisplayAlert("Ошибка", "Нет активных складов. Запросы можно создавать только по работающим складам.", "OK");
                return;
            }

            if (!_viewModel.Materials.Any())
            {
                await DisplayAlert("Ошибка", "Нет доступных материалов. Убедитесь, что в системе есть материалы.", "OK");
                return;
            }

            var availableUsers = _viewModel.Users
                .Where(u => u.Id != currentUser.Id)
                .ToList();
            if (!availableUsers.Any())
            {
                await DisplayAlert("Ошибка", "Нет доступных пользователей для выбора получателя.", "OK");
                return;
            }

            // Выбор пользователя-получателя
            var toUserOptions = availableUsers.Select(u => $"{u.Name} {u.Surname} ({u.Login})").ToArray();
            var toUserIndex = await DisplayActionSheet("Выберите получателя:", "Отмена", null, toUserOptions);
            if (toUserIndex == "Отмена" || string.IsNullOrEmpty(toUserIndex))
                return;

            var toUser = availableUsers.ElementAt(Array.IndexOf(toUserOptions, toUserIndex));
            if (toUser == null)
                return;

            // Выбор склада-отправителя
            var fromWarehouseOptions = _viewModel.Warehouses.Select(w => $"{w.Name} ({w.Type})").ToArray();
            var fromWarehouseIndex = await DisplayActionSheet("Выберите склад-отправитель:", "Отмена", null, fromWarehouseOptions);
            if (fromWarehouseIndex == "Отмена" || string.IsNullOrEmpty(fromWarehouseIndex))
                return;

            var fromWarehouse = _viewModel.Warehouses.ElementAt(Array.IndexOf(fromWarehouseOptions, fromWarehouseIndex));
            if (fromWarehouse == null)
                return;

            // Выбор склада-получателя
            var toWarehouseOptions = _viewModel.Warehouses.Select(w => $"{w.Name} ({w.Type})").ToArray();
            var toWarehouseIndex = await DisplayActionSheet("Выберите склад-получатель:", "Отмена", null, toWarehouseOptions);
            if (toWarehouseIndex == "Отмена" || string.IsNullOrEmpty(toWarehouseIndex))
                return;

            var toWarehouse = _viewModel.Warehouses.ElementAt(Array.IndexOf(toWarehouseOptions, toWarehouseIndex));
            if (toWarehouse == null)
                return;

            // Выбор материала
            var materialOptions = _viewModel.Materials.Select(m => $"{m.Name} ({m.Code ?? "без кода"})").ToArray();
            var materialIndex = await DisplayActionSheet("Выберите материал:", "Отмена", null, materialOptions);
            if (materialIndex == "Отмена" || string.IsNullOrEmpty(materialIndex))
                return;

            var material = _viewModel.Materials.ElementAt(Array.IndexOf(materialOptions, materialIndex));
            if (material == null)
                return;

            // Ввод количества
            var quantityStr = await DisplayPromptAsync("Создание запроса", "Введите количество:", "Создать", "Отмена", "Количество", -1, Keyboard.Numeric, "0");
            if (string.IsNullOrWhiteSpace(quantityStr) || !int.TryParse(quantityStr, out int quantity) || quantity <= 0)
            {
                await DisplayAlert("Ошибка", "Необходимо указать количество больше 0", "OK");
                return;
            }

            // Создание запроса
            var request = new PartRequest
            {
                FromUserId = _viewModel.CurrentUser.Id,
                ToUserId = toUser.Id,
                FromWarehouseId = fromWarehouse.Id,
                ToWarehouseId = toWarehouse.Id,
                MaterialId = material.Id,
                Quantity = quantity,
                MeasuringType = material.MeasuringUnit,
                Status = PartRequestStatus.Pending
            };

            var success = await _viewModel.CreatePartRequestAsync(request);
            if (success)
            {
                await DisplayAlert("Успех", "Запрос на детали успешно создан", "OK");
            }
            else
            {
                await DisplayAlert("Ошибка", _viewModel.ErrorMessage, "OK");
            }
        }

        private async void OnApproveClicked(object? sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is PartRequest request)
            {
                await _viewModel.ApprovePartRequestAsync(request.Id);
            }
        }

        private async void OnRejectClicked(object? sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is PartRequest request)
            {
                var reason = await DisplayPromptAsync("Отклонение запроса", "Укажите причину отклонения:", "Отклонить", "Отмена", "Причина");
                if (!string.IsNullOrWhiteSpace(reason))
                {
                    await _viewModel.RejectPartRequestAsync(request.Id, reason);
                }
            }
        }

        private async void OnCancelClicked(object? sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is PartRequest request)
            {
                await _viewModel.CancelPartRequestAsync(request.Id);
            }
        }
    }
}
