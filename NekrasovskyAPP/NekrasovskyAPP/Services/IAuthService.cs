using NekrasovskyAPP.Models;

namespace NekrasovskyAPP.Services
{
    public interface IAuthService
    {
        Task<User?> LoginAsync(string login, string password);
        Task<bool> IsAdminAsync(int userId);
        void Logout();
        User? CurrentUser { get; }
        bool IsAuthenticated { get; }
    }
}

