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
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Подписываемся на событие каждый раз, когда страница появляется
            // Это гарантирует, что событие работает даже после выхода и повторного входа
            _viewModel.LoginSuccess += OnLoginSuccess;
            
            // Очищаем состояние при возврате на страницу входа
            _viewModel.ClearState();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Отписываемся от события, когда страница скрывается
            _viewModel.LoginSuccess -= OnLoginSuccess;
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            await _viewModel.LoginAsync();
        }

        private async void OnLoginSuccess(object? sender, bool isAdmin)
        {
            // Отписываемся перед навигацией, чтобы избежать проблем
            _viewModel.LoginSuccess -= OnLoginSuccess;
            
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
    }
}

