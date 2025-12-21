using System.ComponentModel;
using System.Runtime.CompilerServices;
using NekrasovskyAPP.Services;

namespace NekrasovskyAPP.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private string _login = string.Empty;
        private string _password = string.Empty;
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        private bool _hasError;

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
        }

        public string Login
        {
            get => _login;
            set
            {
                _login = value;
                OnPropertyChanged();
                HasError = false;
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
                HasError = false;
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public bool HasError
        {
            get => _hasError;
            set
            {
                _hasError = value;
                OnPropertyChanged();
            }
        }

        public event EventHandler<bool>? LoginSuccess;

        public async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Пожалуйста, введите логин и пароль";
                HasError = true;
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                HasError = false;

                var user = await _authService.LoginAsync(Login, Password);

                if (user != null)
                {
                    var isAdmin = await _authService.IsAdminAsync(user.Id);
                    LoginSuccess?.Invoke(this, isAdmin);
                }
                else
                {
                    ErrorMessage = "Неверный логин или пароль";
                    HasError = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка входа: {ex.Message}";
                HasError = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

