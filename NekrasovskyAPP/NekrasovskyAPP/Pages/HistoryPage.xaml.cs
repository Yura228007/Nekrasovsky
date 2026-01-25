using NekrasovskyAPP.ViewModels;

namespace NekrasovskyAPP.Pages
{
    public partial class HistoryPage : ContentPage
    {
        private readonly HistoryViewModel _viewModel;

        public HistoryPage(HistoryViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadFiltersDataAsync();
            await _viewModel.LoadHistoryAsync();
        }

        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            await _viewModel.LoadHistoryAsync();
        }

        private async void OnSearchCompleted(object sender, EventArgs e)
        {
            await _viewModel.ApplyFiltersAsync();
        }

        private void OnResetFiltersClicked(object sender, EventArgs e)
        {
            _viewModel.ResetFilters();
        }
    }
}
