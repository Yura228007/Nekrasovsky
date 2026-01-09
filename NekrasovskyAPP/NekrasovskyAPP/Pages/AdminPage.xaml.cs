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
            // Относительная навигация к глобальному маршруту (без слешей)
            await Shell.Current.GoToAsync("UsersPage");
        }

        private async void OnProductsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("ProductsPage");
        }

        private async void OnMaterialsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("MaterialsPage");
        }

        private async void OnWarehousesClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("WarehousesPage");
        }

        private async void OnReportsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("WorkReportsPage");
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            // Относительная навигация к глобальному маршруту (без слешей)
            await Shell.Current.GoToAsync("SettingsPage");
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            // Выполняем выход
            _authService.Logout();
            
            // Навигация к LoginPage и сброс стека навигации
            await Shell.Current.GoToAsync("///LoginPage");
        }
    }
}

