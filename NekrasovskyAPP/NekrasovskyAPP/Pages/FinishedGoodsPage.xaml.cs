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

        private async void OnItemTapped(object sender, EventArgs e)
        {
            if (sender is not Border border || border.BindingContext is not FillingWarehouse item)
                return;

            if (!_viewModel.CanManage)
            {
                await DisplayAlert("Ошибка", "У вас нет прав на управление складом готовой продукции", "OK");
                return;
            }

            if (item.ProductId == null || item.Quantity <= 0)
                return;

            // Выбор действия: продажа или в утиль
            var action = await DisplayActionSheet(
                $"Продукт: {item.Product?.Name ?? "Неизвестно"}\nОстаток: {item.Quantity} {item.MeasuringType ?? "шт"}",
                "Отмена",
                null,
                "Продажа",
                "Отправить в утиль");

            if (action == "Отмена" || string.IsNullOrEmpty(action))
                return;

            if (action == "Продажа")
            {
                await _viewModel.ProcessSaleAsync(item);
            }
            else if (action == "Отправить в утиль")
            {
                await _viewModel.ProcessDisposalAsync(item);
            }
        }
    }
}
