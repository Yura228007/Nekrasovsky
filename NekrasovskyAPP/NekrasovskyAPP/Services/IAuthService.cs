using NekrasovskyAPP.Models;
using System;

namespace NekrasovskyAPP.Services
{
    public enum LoginErrorType
    {
        None,
        UserNotFound,
        InvalidPassword,
        ConnectionError,
        ServerError
    }

    public class LoginResult
    {
        public User? User { get; set; }
        public LoginErrorType ErrorType { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public bool IsSuccess => User != null && ErrorType == LoginErrorType.None;
    }

    public interface IAuthService
    {
        Task<LoginResult> LoginAsync(string login, string password);
        Task<bool> IsAdminAsync(int userId);
        void Logout();
        User? CurrentUser { get; }
        bool IsAuthenticated { get; }
        event EventHandler? CurrentUserChanged;
    }
}

