using NekrasovskyAPP.ViewModels;
using Microsoft.Maui.Controls;

namespace NekrasovskyAPP.Pages
{
    public partial class LoginPage : ContentPage
    {
        private readonly LoginViewModel _viewModel;

        public LoginPage(LoginViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
            
            // Subscribe to login success event
            _viewModel.LoginSuccess += OnLoginSuccess;
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            await _viewModel.LoginAsync();
        }

        private async void OnLoginSuccess(object? sender, bool isAdmin)
        {
            // Используем ShellNavigationState для явного указания навигации к ShellContent
            // ShellContent определены в AppShell.xaml и доступны через их Route
            if (isAdmin)
            {
                // Навигация к AdminPage (ShellContent)
                await Shell.Current.GoToAsync(new ShellNavigationState("//AdminPage"));
            }
            else
            {
                // Навигация к HomePage (ShellContent)
                await Shell.Current.GoToAsync(new ShellNavigationState("//HomePage"));
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.LoginSuccess -= OnLoginSuccess;
        }
    }
}

