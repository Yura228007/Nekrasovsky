using server.Models;

namespace server.Services
{
    public interface IUserPermissionsService
    {
        Task<IEnumerable<Permission>> GetUserPermissionsAsync(int userId);
        Task<IEnumerable<User>> GetUsersWithPermissionAsync(int permissionId);
        Task AddPermissionToUserAsync(int userId, int permissionId);
        Task RemovePermissionFromUserAsync(int userId, int permissionId);
        Task<bool> HasPermissionAsync(int userId, string permissionCode);
        Task UpdateUserPermissionsAsync(int userId, IEnumerable<int> permissionIds);
    }
}

