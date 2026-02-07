using NekrasovskyAPP.ViewModels;
using NekrasovskyAPP.Models;

namespace NekrasovskyAPP.Pages
{
    public partial class SDHPage : ContentPage
    {
        private readonly SDHViewModel _viewModel;

        public SDHPage(SDHViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadDataAsync();
        }

        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            await _viewModel.LoadDataAsync();
        }

        private async void OnRefreshing(object sender, EventArgs e)
        {
            await _viewModel.LoadDataAsync();
        }

        private async void OnApproveRequestClicked(object sender, EventArgs e)
        {
            if (sender is not Button button || button.CommandParameter is not SDHRequest request)
                return;

            var confirm = await DisplayAlert("Подтверждение", 
                $"Подтвердить запрос на перемещение?\n\nЭлемент: {request.ItemName}\nКоличество: {request.Quantity} {request.MeasuringUnit}",
                "Подтвердить", "Отмена");
            
            if (!confirm)
                return;

            var success = await _viewModel.ApproveSDHRequestAsync(request);
            if (success)
            {
                await DisplayAlert("Успех", "Запрос подтвержден. Ответственность передана вам.", "OK");
            }
            else if (!string.IsNullOrEmpty(_viewModel.ErrorMessage))
            {
                await DisplayAlert("Ошибка", _viewModel.ErrorMessage, "OK");
            }
        }

        private async void OnRejectRequestClicked(object sender, EventArgs e)
        {
            if (sender is not Button button || button.CommandParameter is not SDHRequest request)
                return;

            var confirm = await DisplayAlert("Отклонение", 
                $"Отклонить запрос на перемещение?\n\nЭлемент: {request.ItemName}\nКоличество: {request.Quantity} {request.MeasuringUnit}\n\nОтветственность останется у создателя запроса.",
                "Отклонить", "Отмена");
            
            if (!confirm)
                return;

            var success = await _viewModel.RejectSDHRequestAsync(request);
            if (success)
            {
                await DisplayAlert("Успех", "Запрос отклонен. Ответственность осталась у создателя.", "OK");
            }
            else if (!string.IsNullOrEmpty(_viewModel.ErrorMessage))
            {
                await DisplayAlert("Ошибка", _viewModel.ErrorMessage, "OK");
            }
        }

        private async void OnProcessClicked(object sender, EventArgs e)
        {
            if (sender is not Button button || button.CommandParameter is not SDHItem item)
                return;

            if (!_viewModel.CanManage)
            {
                await DisplayAlert("Нет прав", "У вас нет прав на управление складом СДХ", "OK");
                return;
            }

            var nonReturnableDefectQty = double.TryParse(item.NonReturnableDefectQty, out var nrd) ? nrd : 0;
            var saleQty = double.TryParse(item.SaleQty, out var s) ? s : 0;
            var transferQty = double.TryParse(item.TransferQty, out var t) ? t : 0;

            if (nonReturnableDefectQty <= 0 && saleQty <= 0 && transferQty <= 0)
            {
                await DisplayAlert("Ошибка", "Укажите количество для обработки", "OK");
                return;
            }

            int? transferToWarehouseId = null;
            int? transferToUserId = null;

            if (transferQty > 0)
            {
                // Выбор склада для перемещения
                var warehouses = await _viewModel.GetWarehousesAsync();
                if (warehouses == null || !warehouses.Any())
                {
                    await DisplayAlert("Ошибка", "Нет доступных складов", "OK");
                    return;
                }

                var warehouseOptions = warehouses.Select(w => $"{w.Name} ({w.Type})").ToArray();
                var selectedWarehouse = await DisplayActionSheet("Выберите склад для перемещения:", "Отмена", null, warehouseOptions);
                if (selectedWarehouse == "Отмена" || string.IsNullOrEmpty(selectedWarehouse))
                    return;

                var selectedWarehouseObj = warehouses.FirstOrDefault(w => $"{w.Name} ({w.Type})" == selectedWarehouse);
                if (selectedWarehouseObj == null)
                    return;

                transferToWarehouseId = selectedWarehouseObj.Id;

                // Выбор пользователя для перемещения
                var users = await _viewModel.GetUsersAsync();
                if (users == null || !users.Any())
                {
                    await DisplayAlert("Ошибка", "Нет доступных пользователей", "OK");
                    return;
                }

                var userOptions = users.Select(u => $"{u.Surname} {u.Name}").ToArray();
                var selectedUser = await DisplayActionSheet("Выберите получателя:", "Отмена", null, userOptions);
                if (selectedUser == "Отмена" || string.IsNullOrEmpty(selectedUser))
                    return;

                var selectedUserObj = users.FirstOrDefault(u => $"{u.Surname} {u.Name}" == selectedUser);
                if (selectedUserObj == null)
                    return;

                transferToUserId = selectedUserObj.Id;
            }

            var msg = $"Элемент: {item.ItemName}\n";
            if (nonReturnableDefectQty > 0)
                msg += $"Невозвратный брак: {nonReturnableDefectQty} {item.MeasuringUnit}\n";
            if (saleQty > 0)
                msg += $"Продажа: {saleQty} {item.MeasuringUnit}\n";
            if (transferQty > 0)
                msg += $"Перемещение: {transferQty} {item.MeasuringUnit}";

            var confirm = await DisplayAlert("Подтверждение", $"{msg}\n\nПродолжить?", "Да", "Отмена");
            if (!confirm)
                return;

            var success = await _viewModel.ProcessSDHItemAsync(item, transferToWarehouseId, transferToUserId);

            if (success)
            {
                await DisplayAlert("Успех", "Обработка завершена успешно", "OK");
            }
            else if (!string.IsNullOrEmpty(_viewModel.ErrorMessage))
            {
                await DisplayAlert("Ошибка", _viewModel.ErrorMessage, "OK");
            }
        }
    }
}
