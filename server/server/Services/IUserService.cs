using server.Models;

namespace server.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<IEnumerable<User>> SearchUsersAsync(string? name, string? surname);
        Task<User?> GetUserByIdAsync(int id);
        Task<User> CreateUserAsync(User user);
        Task<User> UpdateUserAsync(int id, User updatedUser);
        Task<bool> DeleteUserAsync(int id);
    }
}

