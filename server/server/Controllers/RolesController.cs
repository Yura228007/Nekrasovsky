using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly IUserService _userService;
        private readonly ILogger<RolesController> _logger;

        public RolesController(IRoleService roleService, IUserService userService, ILogger<RolesController> logger)
        {
            _roleService = roleService;
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Проверяет, является ли текущий пользователь владельцем (Owner)
        /// </summary>
        private async Task<bool> IsCurrentUserOwnerAsync()
        {
            var userIdHeader = Request.Headers["X-User-Id"].FirstOrDefault();
            if (!int.TryParse(userIdHeader, out int userId))
                return false;

            var user = await _userService.GetUserByIdAsync(userId);
            if (user?.RoleId == null)
                return false;

            var role = await _roleService.GetRoleByIdAsync(user.RoleId.Value);
            return role?.Code == "Owner";
        }

        // GET: api/roles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Role>>> GetAll()
        {
            try
            {
                var roles = await _roleService.GetAllRolesAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all roles");
                return StatusCode(500, new { message = "An error occurred while retrieving roles" });
            }
        }

        // GET: api/roles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                var role = await _roleService.GetRoleByIdAsync(id);
                if (role == null)
                {
                    _logger.LogWarning("Role with ID {RoleId} not found", id);
                    return NotFound(new { message = $"Role with ID {id} not found" });
                }

                return Ok(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting role with ID {RoleId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the role" });
            }
        }

        // GET: api/roles/code/{code}
        [HttpGet("code/{code}")]
        public async Task<ActionResult<Role>> GetByCode(string code)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                {
                    return BadRequest(new { message = "Code cannot be empty" });
                }

                var role = await _roleService.GetRoleByCodeAsync(code);
                if (role == null)
                {
                    _logger.LogWarning("Role with Code {RoleCode} not found", code);
                    return NotFound(new { message = $"Role with Code {code} not found" });
                }

                return Ok(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting role with Code {RoleCode}", code);
                return StatusCode(500, new { message = "An error occurred while retrieving the role" });
            }
        }

        // GET: api/roles/{id}/permissions
        [HttpGet("{id}/permissions")]
        public async Task<ActionResult<IEnumerable<Permission>>> GetPermissions(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                var permissions = await _roleService.GetRolePermissionsAsync(id);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting permissions for role {RoleId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving permissions" });
            }
        }

        // GET: api/roles/code/{code}/permissions
        [HttpGet("code/{code}/permissions")]
        public async Task<ActionResult<IEnumerable<Permission>>> GetPermissionsByCode(string code)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                {
                    return BadRequest(new { message = "Code cannot be empty" });
                }

                var permissions = await _roleService.GetRolePermissionsByCodeAsync(code);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting permissions for role {RoleCode}", code);
                return StatusCode(500, new { message = "An error occurred while retrieving permissions" });
            }
        }

        // POST: api/roles
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] Role role)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid model state", errors = ModelState });
            }

            try
            {
                var createdRole = await _roleService.CreateRoleAsync(role);
                _logger.LogInformation("Role created successfully with ID: {RoleId}", createdRole.Id);
                return CreatedAtAction(nameof(GetById), new { id = createdRole.Id },
                    new { message = "Role created successfully", role = createdRole });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while creating role");
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while creating role");
                return StatusCode(500, new { message = "An error occurred while saving the role to the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating role");
                return StatusCode(500, new { message = "An unexpected error occurred while creating the role" });
            }
        }

        // PUT: api/roles/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] Role updated)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Id must be greater than 0" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid model state", errors = ModelState });
            }

            try
            {
                var role = await _roleService.UpdateRoleAsync(id, updated);
                _logger.LogInformation("Role updated successfully with ID: {RoleId}", id);
                return Ok(new { message = "Role updated successfully", role });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Role with ID {RoleId} not found for update", id);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while updating role {RoleId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating role with ID {RoleId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the role in the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating role with ID {RoleId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred while updating the role" });
            }
        }

        // DELETE: api/roles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                var deleted = await _roleService.DeleteRoleAsync(id);
                if (!deleted)
                {
                    _logger.LogWarning("Role with ID {RoleId} not found for deletion", id);
                    return NotFound(new { message = $"Role with ID {id} not found" });
                }

                _logger.LogInformation("Role deleted successfully with ID: {RoleId}", id);
                return Ok(new { message = "Role deleted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot delete role with ID {RoleId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while deleting role with ID {RoleId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the role from the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting role with ID {RoleId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred while deleting the role" });
            }
        }

        // POST: api/roles/{roleId}/permissions/{permissionId}
        [HttpPost("{roleId}/permissions/{permissionId}")]
        public async Task<IActionResult> AssignPermission(int roleId, int permissionId)
        {
            try
            {
                // Проверка что только Owner может изменять права ролей
                if (!await IsCurrentUserOwnerAsync())
                {
                    _logger.LogWarning("Non-owner user attempted to assign permission to role");
                    return StatusCode(403, new { message = "Только владелец может изменять права ролей" });
                }

                if (roleId <= 0 || permissionId <= 0)
                {
                    return BadRequest(new { message = "RoleId and PermissionId must be greater than 0" });
                }

                var assigned = await _roleService.AssignPermissionToRoleAsync(roleId, permissionId);
                if (!assigned)
                {
                    return BadRequest(new { message = "Permission is already assigned to this role" });
                }

                _logger.LogInformation("Permission {PermissionId} assigned to role {RoleId}", permissionId, roleId);
                return Ok(new { message = "Permission assigned successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Key not found while assigning permission");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while assigning permission {PermissionId} to role {RoleId}", permissionId, roleId);
                return StatusCode(500, new { message = "An error occurred while assigning the permission" });
            }
        }

        // DELETE: api/roles/{roleId}/permissions/{permissionId}
        [HttpDelete("{roleId}/permissions/{permissionId}")]
        public async Task<IActionResult> RemovePermission(int roleId, int permissionId)
        {
            try
            {
                // Проверка что только Owner может изменять права ролей
                if (!await IsCurrentUserOwnerAsync())
                {
                    _logger.LogWarning("Non-owner user attempted to remove permission from role");
                    return StatusCode(403, new { message = "Только владелец может изменять права ролей" });
                }

                if (roleId <= 0 || permissionId <= 0)
                {
                    return BadRequest(new { message = "RoleId and PermissionId must be greater than 0" });
                }

                var removed = await _roleService.RemovePermissionFromRoleAsync(roleId, permissionId);
                if (!removed)
                {
                    return NotFound(new { message = "Permission is not assigned to this role" });
                }

                _logger.LogInformation("Permission {PermissionId} removed from role {RoleId}", permissionId, roleId);
                return Ok(new { message = "Permission removed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while removing permission {PermissionId} from role {RoleId}", permissionId, roleId);
                return StatusCode(500, new { message = "An error occurred while removing the permission" });
            }
        }

        // PUT: api/roles/{roleId}/permissions
        [HttpPut("{roleId}/permissions")]
        public async Task<IActionResult> UpdateRolePermissions(int roleId, [FromBody] List<int> permissionIds)
        {
            try
            {
                // Проверка что только Owner может изменять права ролей
                if (!await IsCurrentUserOwnerAsync())
                {
                    _logger.LogWarning("Non-owner user attempted to update role permissions");
                    return StatusCode(403, new { message = "Только владелец может изменять права ролей" });
                }

                if (roleId <= 0)
                {
                    return BadRequest(new { message = "RoleId must be greater than 0" });
                }

                if (permissionIds == null)
                {
                    return BadRequest(new { message = "PermissionIds cannot be null" });
                }

                await _roleService.UpdateRolePermissionsAsync(roleId, permissionIds);

                _logger.LogInformation("Role {RoleId} permissions updated with {Count} permissions", roleId, permissionIds.Count);
                return Ok(new { message = "Права роли обновлены", permissionCount = permissionIds.Count });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Role with ID {RoleId} not found", roleId);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating permissions for role {RoleId}", roleId);
                return StatusCode(500, new { message = "An error occurred while updating role permissions" });
            }
        }
    }
}
