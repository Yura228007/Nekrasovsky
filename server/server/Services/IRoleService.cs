using server.Models;

namespace server.Services
{
    public interface IRoleService
    {
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task<Role?> GetRoleByIdAsync(int id);
        Task<Role?> GetRoleByCodeAsync(string code);
        Task<IEnumerable<Permission>> GetRolePermissionsAsync(int roleId);
        Task<IEnumerable<Permission>> GetRolePermissionsByCodeAsync(string roleCode);
        Task<Role> CreateRoleAsync(Role role);
        Task<Role> UpdateRoleAsync(int id, Role updatedRole);
        Task<bool> DeleteRoleAsync(int id);
        Task<bool> AssignPermissionToRoleAsync(int roleId, int permissionId);
        Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId);
        Task<bool> UserHasRoleAsync(int userId, string roleCode);
        Task<IEnumerable<Role>> GetUserRolesAsync(int userId);
    }
}
