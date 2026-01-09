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
                // Получаем всех пользователей и ищем по логину
                var users = await _apiService.GetAllUsersAsync();
                var user = users.FirstOrDefault(u => u.Login.Equals(login, StringComparison.OrdinalIgnoreCase));

                if (user == null)
                {
                    return new LoginResult
                    {
                        User = null,
                        ErrorType = LoginErrorType.UserNotFound,
                        ErrorMessage = "Пользователь с таким логином не найден"
                    };
                }

                // API возвращает расшифрованный пароль в поле EncryptedPassword
                // Сравниваем напрямую, так как сервер расшифровывает при получении
                if (!user.EncryptedPassword.Equals(password, StringComparison.Ordinal))
                {
                    return new LoginResult
                    {
                        User = null,
                        ErrorType = LoginErrorType.InvalidPassword,
                        ErrorMessage = "Неверный пароль"
                    };
                }

                _currentUser = user;
                return new LoginResult
                {
                    User = user,
                    ErrorType = LoginErrorType.None,
                    ErrorMessage = string.Empty
                };
            }
            catch (HttpRequestException ex)
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
                // Проверяем логин "admin"
                if (_currentUser != null && _currentUser.Login.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                // Проверяем через API разрешения
                var hasAdminPermission = await _apiService.CheckPermissionAsync(userId, "Admin");
                if (hasAdminPermission)
                {
                    return true;
                }

                // Проверяем наличие разрешения через список разрешений
                var permissions = await _apiService.GetUserPermissionsAsync(userId);
                return permissions.Any(p => p.Code.Equals("Admin", StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return false;
            }
        }

        public void Logout()
        {
            _currentUser = null;
        }
    }
}

