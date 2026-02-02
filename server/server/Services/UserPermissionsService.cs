using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Data;
using System.Linq;

namespace server.Services
{
    public class UserPermissionsService : IUserPermissionsService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserPermissionsService> _logger;

        public UserPermissionsService(AppDbContext context, ILogger<UserPermissionsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(int userId)
        {
            return await _context.UserPermissions
                .Where(up => up.UserId == userId)
                .Select(up => up.Permission)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetUsersWithPermissionAsync(int permissionId)
        {
            return await _context.UserPermissions
                .Where(up => up.PermissionId == permissionId)
                .Select(up => up.User)
                .ToListAsync();
        }

        public async Task AddPermissionToUserAsync(int userId, int permissionId)
        {
            // Check if user exists
            if (!await _context.Users.AnyAsync(u => u.Id == userId))
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            // Check if permission exists
            if (!await _context.Permissions.AnyAsync(p => p.Id == permissionId))
            {
                throw new KeyNotFoundException($"Permission with ID {permissionId} not found");
            }

            // Check if already exists
            if (await _context.UserPermissions.AnyAsync(up => up.UserId == userId && up.PermissionId == permissionId))
            {
                throw new InvalidOperationException($"User already has this permission");
            }

            var userPermission = new UserPermissions
            {
                UserId = userId,
                PermissionId = permissionId
            };

            _context.UserPermissions.Add(userPermission);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Permission {PermissionId} added to user {UserId}", permissionId, userId);
        }

        public async Task RemovePermissionFromUserAsync(int userId, int permissionId)
        {
            var userPermission = await _context.UserPermissions
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionId);

            if (userPermission == null)
            {
                throw new KeyNotFoundException($"User does not have this permission");
            }

            _context.UserPermissions.Remove(userPermission);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Permission {PermissionId} removed from user {UserId}", permissionId, userId);
        }

        public async Task<bool> HasPermissionAsync(int userId, string permissionCode)
        {
            return await _context.UserPermissions
                .AnyAsync(up => up.UserId == userId && up.Permission != null && up.Permission.Code == permissionCode);
        }

        public async Task UpdateUserPermissionsAsync(int userId, IEnumerable<int> permissionIds)
        {
            // Check if user exists
            if (!await _context.Users.AnyAsync(u => u.Id == userId))
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            // Remove all existing permissions
            var existingPermissions = await _context.UserPermissions
                .Where(up => up.UserId == userId)
                .ToListAsync();

            _context.UserPermissions.RemoveRange(existingPermissions);

            // Add new permissions
            var permissionIdsList = permissionIds.ToList();
            foreach (var permissionId in permissionIdsList)
            {
                if (!await _context.Permissions.AnyAsync(p => p.Id == permissionId))
                {
                    throw new KeyNotFoundException($"Permission with ID {permissionId} not found");
                }

                _context.UserPermissions.Add(new UserPermissions
                {
                    UserId = userId,
                    PermissionId = permissionId
                });
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Permissions updated for user {UserId}", userId);
        }
    }
}

