using NekrasovskyAPP.Models;
using NekrasovskyAPP.ViewModels;
using System.Linq;

namespace NekrasovskyAPP.Pages
{
    public partial class ShiftTransfersPage : ContentPage
    {
        private readonly ShiftTransfersViewModel _viewModel;

        public ShiftTransfersPage(ShiftTransfersViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadUsersAsync();
            await _viewModel.LoadTransfersAsync();
        }

        private async void OnRefreshClicked(object? sender, EventArgs e)
        {
            await _viewModel.LoadTransfersAsync();
        }

        private async void OnCreateClicked(object? sender, EventArgs e)
        {
            if (_viewModel.CurrentUser == null)
            {
                await DisplayAlert("Ошибка", "Пользователь не авторизован", "OK");
                return;
            }

            var activeReports = await _viewModel.ApiService.GetActiveWorkReportsAsync(_viewModel.CurrentUser.Id);
            if (!activeReports.Any())
            {
                await DisplayAlert("Смена не начата", "Нельзя передать смену без активной смены.", "OK");
                return;
            }

            if (!_viewModel.Users.Any())
            {
                await DisplayAlert("Ошибка", "Нет доступных пользователей", "OK");
                return;
            }

            var availableUsers = _viewModel.Users
                .Where(u => u.Id != _viewModel.CurrentUser.Id)
                .ToList();

            if (!availableUsers.Any())
            {
                await DisplayAlert("Ошибка", "Нет других пользователей для передачи смены", "OK");
                return;
            }

            var options = availableUsers.Select(u => $"{u.Name} {u.Surname} ({u.Login})").ToArray();
            var selected = await DisplayActionSheet("Выберите пользователя для передачи смены:", "Отмена", null, options);
            if (selected == "Отмена" || string.IsNullOrWhiteSpace(selected))
            {
                return;
            }

            var user = availableUsers.ElementAt(Array.IndexOf(options, selected));
            var success = await _viewModel.CreateShiftTransferAsync(user.Id);
            if (success)
            {
                await DisplayAlert("Успех", "Передача смены создана", "OK");
            }
            else
            {
                await DisplayAlert("Ошибка", _viewModel.ErrorMessage, "OK");
            }
        }

        private async void OnConfirmClicked(object? sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is ShiftTransfer transfer)
            {
                // Остатки загружаются из ResponsibilityFilling (то же, что передаётся при подтверждении смены)
                var stockItems = await _viewModel.LoadResponsibilityStockAsync(transfer.FromUserId);
                if (stockItems.Count > 0)
                {
                    var reviewPage = new ShiftTransferReviewPage(stockItems);
                    await Navigation.PushModalAsync(reviewPage);
                    var confirmed = await reviewPage.ConfirmationTask;
                    if (!confirmed)
                    {
                        return;
                    }
                }

                var success = await _viewModel.ConfirmShiftTransferAsync(transfer.Id);
                if (!success)
                {
                    await DisplayAlert("Ошибка", _viewModel.ErrorMessage, "OK");
                }
                else
                {
                    await DisplayAlert("Готово", "Смена принята. Ответственность переведена на вас, у вас начата новая смена.", "OK");
                }
            }
        }

        private async void OnCancelClicked(object? sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is ShiftTransfer transfer)
            {
                var success = await _viewModel.CancelShiftTransferAsync(transfer.Id);
                if (!success)
                {
                    await DisplayAlert("Ошибка", _viewModel.ErrorMessage, "OK");
                }
            }
        }
    }
}
