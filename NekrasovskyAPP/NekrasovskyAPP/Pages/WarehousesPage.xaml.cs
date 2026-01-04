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

        private string _lastSearchText = string.Empty;

        private async void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
        {
            var searchText = e.NewTextValue ?? string.Empty;
            
            if (searchText == _lastSearchText)
                return;
            
            _lastSearchText = searchText;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                await _viewModel.LoadWarehousesAsync();
            }
            else
            {
                var parts = searchText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var name = parts.Length > 0 ? parts[0] : null;
                var type = parts.Length > 1 ? parts[1] : null;
                
                await _viewModel.SearchWarehousesAsync(name, type);
            }
        }
    }
}

