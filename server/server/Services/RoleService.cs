using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Data;

namespace server.Services
{
    public class RoleService : IRoleService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RoleService> _logger;

        public RoleService(AppDbContext context, ILogger<RoleService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await _context.Roles
                .OrderBy(r => r.Rank)
                .ToListAsync();
        }

        public async Task<Role?> GetRoleByIdAsync(int id)
        {
            return await _context.Roles.FindAsync(id);
        }

        public async Task<Role?> GetRoleByCodeAsync(string code)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Code == code);
        }

        public async Task<IEnumerable<Permission>> GetRolePermissionsAsync(int roleId)
        {
            return await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission)
                .ToListAsync();
        }

        public async Task<IEnumerable<Permission>> GetRolePermissionsByCodeAsync(string roleCode)
        {
            var role = await GetRoleByCodeAsync(roleCode);
            if (role == null)
                return Enumerable.Empty<Permission>();

            return await GetRolePermissionsAsync(role.Id);
        }

        public async Task<Role> CreateRoleAsync(Role role)
        {
            // Проверяем, существует ли роль с таким кодом
            if (await _context.Roles.AnyAsync(r => r.Code == role.Code))
            {
                throw new InvalidOperationException($"Role with code '{role.Code}' already exists");
            }

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Role created with ID: {RoleId}, Code: {Code}", role.Id, role.Code);
            return role;
        }

        public async Task<Role> UpdateRoleAsync(int id, Role updatedRole)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                throw new KeyNotFoundException($"Role with ID {id} not found");
            }

            // Проверяем, не используется ли новый код другой ролью
            if (role.Code != updatedRole.Code && 
                await _context.Roles.AnyAsync(r => r.Code == updatedRole.Code))
            {
                throw new InvalidOperationException($"Role with code '{updatedRole.Code}' already exists");
            }

            role.Name = updatedRole.Name;
            role.Code = updatedRole.Code;
            role.Description = updatedRole.Description;
            role.Rank = updatedRole.Rank;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Role updated with ID: {RoleId}", role.Id);
            return role;
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return false;
            }

            // Проверяем, есть ли пользователи с этой ролью
            var usersWithRole = await _context.Users.AnyAsync(u => u.RoleId == id);
            if (usersWithRole)
            {
                throw new InvalidOperationException($"Cannot delete role with ID {id} because there are users assigned to this role");
            }

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Role deleted with ID: {RoleId}", id);
            return true;
        }

        public async Task<bool> AssignPermissionToRoleAsync(int roleId, int permissionId)
        {
            // Проверяем, существует ли связь
            if (await _context.RolePermissions.AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId))
            {
                return false; // Связь уже существует
            }

            // Проверяем, существуют ли роль и право
            if (!await _context.Roles.AnyAsync(r => r.Id == roleId))
            {
                throw new KeyNotFoundException($"Role with ID {roleId} not found");
            }

            if (!await _context.Permissions.AnyAsync(p => p.Id == permissionId))
            {
                throw new KeyNotFoundException($"Permission with ID {permissionId} not found");
            }

            _context.RolePermissions.Add(new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            });

            await _context.SaveChangesAsync();

            _logger.LogInformation("Permission {PermissionId} assigned to role {RoleId}", permissionId, roleId);
            return true;
        }

        public async Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId)
        {
            var rolePermission = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

            if (rolePermission == null)
            {
                return false;
            }

            _context.RolePermissions.Remove(rolePermission);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Permission {PermissionId} removed from role {RoleId}", permissionId, roleId);
            return true;
        }

        public async Task<bool> UserHasRoleAsync(int userId, string roleCode)
        {
            return await _context.Users
                .Where(u => u.Id == userId)
                .Include(u => u.Role)
                .AnyAsync(u => u.Role != null && u.Role.Code == roleCode);
        }

        public async Task<IEnumerable<Role>> GetUserRolesAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.Role == null)
                return Enumerable.Empty<Role>();

            return new[] { user.Role };
        }
    }
}
