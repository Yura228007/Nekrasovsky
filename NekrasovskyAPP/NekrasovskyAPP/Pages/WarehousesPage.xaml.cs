using NekrasovskyAPP.ViewModels;

namespace NekrasovskyAPP.Pages
{
    public partial class WarehousesPage : ContentPage
    {
        private readonly MainViewModel _viewModel;

        public WarehousesPage(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadWarehousesAsync();
        }

        private async void OnRefreshing(object? sender, EventArgs e)
        {
            await _viewModel.LoadWarehousesAsync();
        }

        private async void OnRefreshClicked(object? sender, EventArgs e)
        {
            await _viewModel.LoadWarehousesAsync();
        }

        private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
        {
            // Search implementation
        }
    }
}

