using NekrasovskyAPP.ViewModels;

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
            if (isAdmin)
            {
                await Shell.Current.GoToAsync("//AdminPage");
            }
            else
            {
                await Shell.Current.GoToAsync("//HomePage");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.LoginSuccess -= OnLoginSuccess;
        }
    }
}

