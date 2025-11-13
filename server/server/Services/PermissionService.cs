using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Data;

namespace server.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PermissionService> _logger;

        public PermissionService(AppDbContext context, ILogger<PermissionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Permission>> GetAllPermissionsAsync()
        {
            return await _context.Permissions.ToListAsync();
        }

        public async Task<Permission?> GetPermissionByIdAsync(int id)
        {
            return await _context.Permissions.FindAsync(id);
        }

        public async Task<Permission?> GetPermissionByCodeAsync(string code)
        {
            return await _context.Permissions
                .FirstOrDefaultAsync(p => p.Code == code);
        }

        public async Task<Permission> CreatePermissionAsync(Permission permission)
        {
            // Business logic: Check if code already exists
            if (await _context.Permissions.AnyAsync(p => p.Code == permission.Code))
            {
                throw new InvalidOperationException($"Permission with code '{permission.Code}' already exists");
            }

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Permission created with ID: {PermissionId}, Code: {Code}", permission.Id, permission.Code);
            return permission;
        }

        public async Task<Permission> UpdatePermissionAsync(int id, Permission updatedPermission)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null)
            {
                throw new KeyNotFoundException($"Permission with ID {id} not found");
            }

            // Business logic: Check if code is being changed and if new code already exists
            if (permission.Code != updatedPermission.Code)
            {
                if (await _context.Permissions.AnyAsync(p => p.Code == updatedPermission.Code && p.Id != id))
                {
                    throw new InvalidOperationException($"Permission with code '{updatedPermission.Code}' already exists");
                }
            }

            // Update permission properties
            permission.Code = updatedPermission.Code;
            permission.Name = updatedPermission.Name;
            permission.Description = updatedPermission.Description;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Permission updated with ID: {PermissionId}", permission.Id);
            return permission;
        }

        public async Task<bool> DeletePermissionAsync(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null)
            {
                return false;
            }

            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Permission deleted with ID: {PermissionId}", id);
            return true;
        }
    }
}

