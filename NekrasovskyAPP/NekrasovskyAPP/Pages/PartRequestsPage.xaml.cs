using NekrasovskyAPP.ViewModels;
using NekrasovskyAPP.Models;

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
            // TODO: Implement add part request dialog
            await DisplayAlert("Информация", "Функция создания запроса будет реализована позже", "OK");
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
    }
}
