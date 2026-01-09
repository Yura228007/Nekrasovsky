using NekrasovskyAPP.Services;

namespace NekrasovskyAPP.Pages
{
    public partial class HomePage : ContentPage
    {
        private readonly IAuthService _authService;

        public HomePage(IAuthService authService)
        {
            InitializeComponent();
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

        private async void OnWorkReportsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("WorkReportsPage");
        }

        private async void OnPartRequestsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("PartRequestsPage");
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            _authService.Logout();
            // LoginPage - это ShellContent, можно использовать абсолютную навигацию
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}

