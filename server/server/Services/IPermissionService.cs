using server.Models;

namespace server.Services
{
    public interface IPermissionService
    {
        Task<IEnumerable<Permission>> GetAllPermissionsAsync();
        Task<Permission?> GetPermissionByIdAsync(int id);
        Task<Permission?> GetPermissionByCodeAsync(string code);
        Task<Permission> CreatePermissionAsync(Permission permission);
        Task<Permission> UpdatePermissionAsync(int id, Permission updatedPermission);
        Task<bool> DeletePermissionAsync(int id);
    }
}

