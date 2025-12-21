using NekrasovskyAPP.ViewModels;
using NekrasovskyAPP.Services;

namespace NekrasovskyAPP.Pages
{
    public partial class AdminPage : ContentPage
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IAuthService _authService;

        public AdminPage(MainViewModel mainViewModel, IAuthService authService)
        {
            InitializeComponent();
            _mainViewModel = mainViewModel;
            _authService = authService;
        }

        private async void OnUsersClicked(object sender, EventArgs e)
        {
            var viewModel = Handler?.MauiContext?.Services.GetService<MainViewModel>();
            if (viewModel != null)
            {
                var page = new UsersPage(viewModel);
                await Navigation.PushAsync(page);
            }
        }

        private async void OnProductsClicked(object sender, EventArgs e)
        {
            var viewModel = Handler?.MauiContext?.Services.GetService<MainViewModel>();
            if (viewModel != null)
            {
                var page = new ProductsPage(viewModel);
                await Navigation.PushAsync(page);
            }
        }

        private async void OnMaterialsClicked(object sender, EventArgs e)
        {
            var viewModel = Handler?.MauiContext?.Services.GetService<MainViewModel>();
            if (viewModel != null)
            {
                var page = new MaterialsPage(viewModel);
                await Navigation.PushAsync(page);
            }
        }

        private async void OnWarehousesClicked(object sender, EventArgs e)
        {
            var viewModel = Handler?.MauiContext?.Services.GetService<MainViewModel>();
            if (viewModel != null)
            {
                var page = new WarehousesPage(viewModel);
                await Navigation.PushAsync(page);
            }
        }

        private async void OnReportsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//WorkReportsPage");
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            _authService.Logout();
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}

