using NekrasovskyAPP.ViewModels;
using NekrasovskyAPP.Models;

namespace NekrasovskyAPP.Pages
{
    public partial class DisposalPage : ContentPage
    {
        private readonly DisposalViewModel _viewModel;

        public DisposalPage(DisposalViewModel viewModel)
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

        private async void OnProcessClicked(object sender, EventArgs e)
        {
            if (sender is not Button button || button.CommandParameter is not DisposalItem item)
                return;

            if (!_viewModel.CanDelete)
            {
                await DisplayAlert("Нет прав", "У вас нет прав на обработку утиля", "OK");
                return;
            }

            var returnableQty = int.TryParse(item.ReturnableQty, out var r) ? r : 0;
            var nonReturnableQty = int.TryParse(item.NonReturnableQty, out var n) ? n : 0;

            if (returnableQty <= 0 && nonReturnableQty <= 0)
            {
                await DisplayAlert("Ошибка", "Укажите количество возвратного и/или невозвратного брака", "OK");
                return;
            }

            var msg = $"Элемент: {item.Name}\n";
            if (returnableQty > 0)
                msg += $"В ЭКО (возвратный): {returnableQty} {item.MeasuringUnit}\n";
            if (nonReturnableQty > 0)
                msg += $"Списать (невозвратный): {nonReturnableQty} {item.MeasuringUnit}";

            var confirm = await DisplayAlert("Подтверждение", $"{msg}\n\nПродолжить?", "Да", "Отмена");
            if (!confirm)
                return;

            var success = await _viewModel.ProcessDisposalAsync(item);

            if (success)
            {
                await DisplayAlert("Успех", "Утиль обработан успешно", "OK");
            }
            else if (!string.IsNullOrEmpty(_viewModel.ErrorMessage))
            {
                await DisplayAlert("Ошибка", _viewModel.ErrorMessage, "OK");
            }
        }

        private async void OnApproveRequestClicked(object sender, EventArgs e)
        {
            if (sender is not Button button || button.CommandParameter is not DisposalRequest request)
                return;

            var confirm = await DisplayAlert("Подтверждение", 
                $"Подтвердить запрос на перемещение?\n\nЭлемент: {request.ItemName}\nКоличество: {request.Quantity} {request.MeasuringUnit}\nТип: {(request.RequestType == DisposalRequestType.Defect ? "Невозвратный брак" : "Переработка")}",
                "Подтвердить", "Отмена");
            
            if (!confirm)
                return;

            var success = await _viewModel.ApproveDisposalRequestAsync(request);
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
            if (sender is not Button button || button.CommandParameter is not DisposalRequest request)
                return;

            var confirm = await DisplayAlert("Отклонение", 
                $"Отклонить запрос на перемещение?\n\nЭлемент: {request.ItemName}\nКоличество: {request.Quantity} {request.MeasuringUnit}\nТип: {(request.RequestType == DisposalRequestType.Defect ? "Невозвратный брак" : "Переработка")}\n\nОтветственность останется у создателя запроса.",
                "Отклонить", "Отмена");
            
            if (!confirm)
                return;

            var success = await _viewModel.RejectDisposalRequestAsync(request);
            if (success)
            {
                await DisplayAlert("Успех", "Запрос отклонен. Ответственность осталась у создателя.", "OK");
            }
            else if (!string.IsNullOrEmpty(_viewModel.ErrorMessage))
            {
                await DisplayAlert("Ошибка", _viewModel.ErrorMessage, "OK");
            }
        }
    }
}
