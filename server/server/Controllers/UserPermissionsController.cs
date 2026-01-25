using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Services;
using server.Attributes;
using System.Linq;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserPermissionsController : ControllerBase
    {
        private readonly IUserPermissionsService _userPermissionsService;
        private readonly ILogger<UserPermissionsController> _logger;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;

        public UserPermissionsController(
            IUserPermissionsService userPermissionsService,
            ILogger<UserPermissionsController> logger,
            IUserService userService,
            IRoleService roleService)
        {
            _userPermissionsService = userPermissionsService;
            _logger = logger;
            _userService = userService;
            _roleService = roleService;
        }

        // GET: api/user-permissions/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Permission>>> GetUserPermissions(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new { message = "UserId must be greater than 0" });
                }

                var permissions = await _userPermissionsService.GetUserPermissionsAsync(userId);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting permissions for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while retrieving permissions" });
            }
        }

        // GET: api/user-permissions/permission/5
        [HttpGet("permission/{permissionId}")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsersWithPermission(int permissionId)
        {
            try
            {
                if (permissionId <= 0)
                {
                    return BadRequest(new { message = "PermissionId must be greater than 0" });
                }

                var users = await _userPermissionsService.GetUsersWithPermissionAsync(permissionId);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting users with permission {PermissionId}", permissionId);
                return StatusCode(500, new { message = "An error occurred while retrieving users" });
            }
        }

        // POST: api/user-permissions
        [HttpPost]
        [RequirePermission("ManageUsers")]
        public async Task<IActionResult> AddPermissionToUser([FromBody] UserPermissions userPermission)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid model state", errors = ModelState });
            }

            try
            {
                if (userPermission.UserId <= 0)
                {
                    return BadRequest(new { message = "UserId must be greater than 0" });
                }

                if (userPermission.PermissionId <= 0)
                {
                    return BadRequest(new { message = "PermissionId must be greater than 0" });
                }

                await _userPermissionsService.AddPermissionToUserAsync(userPermission.UserId, userPermission.PermissionId);
                _logger.LogInformation("Permission {PermissionId} added to user {UserId}", userPermission.PermissionId, userPermission.UserId);
                return Ok(new { message = "Permission added to user successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Key not found while adding permission to user");
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while adding permission to user");
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while adding permission to user");
                return StatusCode(500, new { message = "An error occurred while saving to the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while adding permission to user");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        // DELETE: api/user-permissions?userId=5&permissionId=3
        [HttpDelete]
        [RequirePermission("ManageUsers")]
        public async Task<IActionResult> RemovePermissionFromUser([FromQuery] int userId, [FromQuery] int permissionId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new { message = "UserId must be greater than 0" });
                }

                if (permissionId <= 0)
                {
                    return BadRequest(new { message = "PermissionId must be greater than 0" });
                }

                await _userPermissionsService.RemovePermissionFromUserAsync(userId, permissionId);
                _logger.LogInformation("Permission {PermissionId} removed from user {UserId}", permissionId, userId);
                return Ok(new { message = "Permission removed from user successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Key not found while removing permission from user");
                return NotFound(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while removing permission from user");
                return StatusCode(500, new { message = "An error occurred while saving to the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while removing permission from user");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        // GET: api/user-permissions/check?userId=5&permissionCode=Admin
        [HttpGet("check")]
        public async Task<ActionResult<bool>> CheckPermission([FromQuery] int userId, [FromQuery] string permissionCode)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new { message = "UserId must be greater than 0" });
                }

                if (string.IsNullOrWhiteSpace(permissionCode))
                {
                    return BadRequest(new { message = "PermissionCode cannot be empty" });
                }

                var hasPermission = await HasPermissionAsync(userId, permissionCode);
                return Ok(new { hasPermission });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking permission for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while checking permission" });
            }
        }

        private async Task<bool> HasPermissionAsync(int userId, string permissionCode)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            if (user.RoleId.HasValue)
            {
                var rolePermissions = await _roleService.GetRolePermissionsAsync(user.RoleId.Value);
                if (rolePermissions.Any(p => p.Code == permissionCode))
                {
                    return true;
                }
            }

            if (await _userPermissionsService.HasPermissionAsync(userId, permissionCode))
            {
                return true;
            }

            if (user.Login.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (user.RoleId.HasValue)
            {
                var role = await _roleService.GetRoleByIdAsync(user.RoleId.Value);
                if (role != null && (role.Code == "Owner" || role.Code == "Admin"))
                {
                    return true;
                }
            }

            return false;
        }

        // PUT: api/user-permissions/5
        [HttpPut("{userId}")]
        [RequirePermission("ManageUsers")]
        public async Task<IActionResult> UpdateUserPermissions(int userId, [FromBody] IEnumerable<int> permissionIds)
        {
            if (userId <= 0)
            {
                return BadRequest(new { message = "UserId must be greater than 0" });
            }

            try
            {
                await _userPermissionsService.UpdateUserPermissionsAsync(userId, permissionIds);
                _logger.LogInformation("User permissions updated successfully for user {UserId}", userId);
                return Ok(new { message = "User permissions updated successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User with ID {UserId} not found for updating permissions", userId);
                return NotFound(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating user permissions for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while saving to the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating user permissions for user {UserId}", userId);
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }
    }
}
