using server.Models;

namespace server.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<(IEnumerable<User> Users, int TotalCount)> GetAllUsersPagedAsync(int page, int pageSize);
        Task<IEnumerable<User>> SearchUsersAsync(string? name, string? surname);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByLoginAsync(string login);
        Task<User> CreateUserAsync(User user);
        Task<User> UpdateUserAsync(int id, User updatedUser);
        Task<bool> DeleteUserAsync(int id);
    }
}

