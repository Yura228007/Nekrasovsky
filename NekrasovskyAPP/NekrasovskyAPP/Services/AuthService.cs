using System;
using NekrasovskyAPP.Models;
using System.Net.Http.Json;
using System.Text.Json;
using System.Net.Http;

namespace NekrasovskyAPP.Services
{
    public class AuthService : IAuthService
    {
        private readonly IApiService _apiService;
        private User? _currentUser;
        private readonly JsonSerializerOptions _jsonOptions;
        public event EventHandler? CurrentUserChanged;

        public AuthService(IApiService apiService)
        {
            _apiService = apiService;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public User? CurrentUser => _currentUser;
        public bool IsAuthenticated => _currentUser != null;

        public async Task<LoginResult> LoginAsync(string login, string password)
        {
            try
            {
                var response = await _apiService.AuthenticateAsync(login, password);
                if (response.User == null)
                {
                    return new LoginResult
                    {
                        User = null,
                        ErrorType = LoginErrorType.InvalidPassword,
                        ErrorMessage = string.IsNullOrWhiteSpace(response.Message)
                            ? "Неверный логин или пароль"
                            : response.Message
                    };
                }

                _currentUser = response.User;
                // Устанавливаем userId в ApiService для автоматического добавления в заголовки
                if (_apiService is ApiService apiService)
                {
                    apiService.SetCurrentUserId(_currentUser.Id);
                }
                NotifyCurrentUserChanged();
                return new LoginResult
                {
                    User = _currentUser,
                    ErrorType = LoginErrorType.None,
                    ErrorMessage = string.Empty
                };
            }
            catch (HttpRequestException)
            {
                return new LoginResult
                {
                    User = null,
                    ErrorType = LoginErrorType.ConnectionError,
                    ErrorMessage = "Не удалось подключиться к серверу. Проверьте подключение к интернету и убедитесь, что сервер запущен."
                };
            }
            catch (TaskCanceledException)
            {
                return new LoginResult
                {
                    User = null,
                    ErrorType = LoginErrorType.ConnectionError,
                    ErrorMessage = "Превышено время ожидания ответа от сервера. Проверьте подключение к интернету."
                };
            }
            catch (Exception ex)
            {
                return new LoginResult
                {
                    User = null,
                    ErrorType = LoginErrorType.ServerError,
                    ErrorMessage = $"Ошибка сервера: {ex.Message}"
                };
            }
        }

        public async Task<bool> IsAdminAsync(int userId)
        {
            try
            {
                if (_currentUser?.Role != null)
                {
                    var roleCode = _currentUser.Role.Code ?? string.Empty;
                    var roleName = _currentUser.Role.Name ?? string.Empty;
                    if (roleCode.Equals("Owner", StringComparison.OrdinalIgnoreCase) ||
                        roleCode.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                        roleName.Equals("Владелец", StringComparison.OrdinalIgnoreCase) ||
                        roleName.Equals("Администратор", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public void Logout()
        {
            _currentUser = null;
            // Очищаем userId в ApiService
            if (_apiService is ApiService apiService)
            {
                apiService.SetCurrentUserId(null);
            }
            NotifyCurrentUserChanged();
        }

        private void NotifyCurrentUserChanged()
        {
            CurrentUserChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

