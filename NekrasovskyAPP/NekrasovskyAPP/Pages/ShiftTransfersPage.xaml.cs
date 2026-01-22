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
            if (!_viewModel.Users.Any())
            {
                await DisplayAlert("Ошибка", "Нет доступных пользователей", "OK");
                return;
            }

            var options = _viewModel.Users.Select(u => $"{u.Name} {u.Surname} ({u.Login})").ToArray();
            var selected = await DisplayActionSheet("Выберите пользователя для передачи смены:", "Отмена", null, options);
            if (selected == "Отмена" || string.IsNullOrWhiteSpace(selected))
            {
                return;
            }

            var user = _viewModel.Users.ElementAt(Array.IndexOf(options, selected));
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
                var success = await _viewModel.ConfirmShiftTransferAsync(transfer.Id);
                if (!success)
                {
                    await DisplayAlert("Ошибка", _viewModel.ErrorMessage, "OK");
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
