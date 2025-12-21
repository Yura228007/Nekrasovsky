using NekrasovskyAPP.ViewModels;

namespace NekrasovskyAPP.Pages
{
    public partial class ProductsPage : ContentPage
    {
        private readonly MainViewModel _viewModel;

        public ProductsPage(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadProductsAsync();
        }

        private async void OnRefreshing(object? sender, EventArgs e)
        {
            await _viewModel.LoadProductsAsync();
        }

        private async void OnRefreshClicked(object? sender, EventArgs e)
        {
            await _viewModel.LoadProductsAsync();
        }

        private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
        {
            // Search implementation
        }
    }
}

