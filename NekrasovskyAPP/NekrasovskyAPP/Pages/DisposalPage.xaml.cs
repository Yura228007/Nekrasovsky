using NekrasovskyAPP.ViewModels;

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

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is not Button button || button.CommandParameter is not DisposalItem item)
                return;

            if (!_viewModel.CanDelete)
            {
                await DisplayAlert("Нет прав", "У вас нет прав на списание", "OK");
                return;
            }

            // Запрашиваем причину списания
            var reason = await DisplayPromptAsync(
                "Списание",
                $"Укажите причину списания \"{item.Name}\" ({item.Quantity} {item.MeasuringUnit}):",
                "Списать",
                "Отмена",
                "Причина списания",
                -1,
                Keyboard.Default);

            if (string.IsNullOrWhiteSpace(reason))
                return;

            // Подтверждение
            var confirm = await DisplayAlert(
                "Подтверждение",
                $"Вы уверены, что хотите списать \"{item.Name}\" ({item.Quantity} {item.MeasuringUnit})?\n\nПричина: {reason}",
                "Да, списать",
                "Отмена");

            if (!confirm)
                return;

            var success = await _viewModel.DeleteItemAsync(item, reason);

            if (success)
            {
                await DisplayAlert("Успех", $"Элемент \"{item.Name}\" успешно списан", "OK");
            }
        }
    }
}
