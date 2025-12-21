using NekrasovskyAPP.ViewModels;

namespace NekrasovskyAPP.Pages
{
    public partial class MaterialsPage : ContentPage
    {
        private readonly MainViewModel _viewModel;

        public MaterialsPage(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadMaterialsAsync();
        }

        private async void OnRefreshing(object? sender, EventArgs e)
        {
            await _viewModel.LoadMaterialsAsync();
        }

        private async void OnRefreshClicked(object? sender, EventArgs e)
        {
            await _viewModel.LoadMaterialsAsync();
        }

        private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
        {
            // Search implementation
        }
    }
}

