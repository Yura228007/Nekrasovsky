using NekrasovskyAPP.ViewModels;
using NekrasovskyAPP.Models;

namespace NekrasovskyAPP.Pages
{
    public partial class FinishedGoodsPage : ContentPage
    {
        private readonly FinishedGoodsViewModel _viewModel;

        public FinishedGoodsPage(FinishedGoodsViewModel viewModel)
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
            if (sender is not Button button || button.CommandParameter is not FinishedGoodsRequest request)
                return;

            var confirm = await DisplayAlert("Подтверждение", 
                $"Подтвердить запрос на перемещение?\n\nПродукт: {request.ProductName}\nКоличество: {request.Quantity} {request.MeasuringUnit}\nТип: {request.RequestTypeDisplay}",
                "Подтвердить", "Отмена");
            
            if (!confirm)
                return;

            var success = await _viewModel.ApproveFinishedGoodsRequestAsync(request);
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
            if (sender is not Button button || button.CommandParameter is not FinishedGoodsRequest request)
                return;

            var confirm = await DisplayAlert("Отклонение", 
                $"Отклонить запрос на перемещение?\n\nПродукт: {request.ProductName}\nКоличество: {request.Quantity} {request.MeasuringUnit}\nТип: {request.RequestTypeDisplay}\n\nОтветственность останется у создателя запроса.",
                "Отклонить", "Отмена");
            
            if (!confirm)
                return;

            var success = await _viewModel.RejectFinishedGoodsRequestAsync(request);
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
            if (sender is not Button button || button.CommandParameter is not FinishedGoodsItem item)
                return;

            if (!_viewModel.CanManage)
            {
                await DisplayAlert("Нет прав", "У вас нет прав на управление складом готовой продукции", "OK");
                return;
            }

            var saleQty = double.TryParse(item.SaleQty, out var s) ? s : 0;
            var disposalQty = double.TryParse(item.DisposalQty, out var d) ? d : 0;

            if (saleQty <= 0 && disposalQty <= 0)
            {
                await DisplayAlert("Ошибка", "Укажите количество для продажи и/или отправки в утиль", "OK");
                return;
            }

            var msg = $"Продукт: {item.Product?.Name ?? "Неизвестно"}\n";
            if (saleQty > 0)
                msg += $"Продажа: {saleQty} {item.MeasuringUnit}\n";
            if (disposalQty > 0)
                msg += $"Утиль: {disposalQty} {item.MeasuringUnit}";

            var confirm = await DisplayAlert("Подтверждение", $"{msg}\n\nПродолжить?", "Да", "Отмена");
            if (!confirm)
                return;

            var success = await _viewModel.ProcessFinishedGoodsAsync(item);

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
