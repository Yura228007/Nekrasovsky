using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;
        private readonly ILogger<PermissionsController> _logger;

        public PermissionsController(IPermissionService permissionService, ILogger<PermissionsController> logger)
        {
            _permissionService = permissionService;
            _logger = logger;
        }

        // GET: api/permissions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Permission>>> GetAll()
        {
            try
            {
                var permissions = await _permissionService.GetAllPermissionsAsync();
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all permissions");
                return StatusCode(500, new { message = "An error occurred while retrieving permissions" });
            }
        }

        // GET: api/permissions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Permission>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                var permission = await _permissionService.GetPermissionByIdAsync(id);
                if (permission == null)
                {
                    _logger.LogWarning("Permission with ID {PermissionId} not found", id);
                    return NotFound(new { message = $"Permission with ID {id} not found" });
                }

                return Ok(permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting permission with ID {PermissionId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the permission" });
            }
        }

        // GET: api/permissions/code/{code}
        [HttpGet("code/{code}")]
        public async Task<ActionResult<Permission>> GetByCode(string code)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                {
                    return BadRequest(new { message = "Code cannot be empty" });
                }

                var permission = await _permissionService.GetPermissionByCodeAsync(code);
                if (permission == null)
                {
                    _logger.LogWarning("Permission with code '{Code}' not found", code);
                    return NotFound(new { message = $"Permission with code '{code}' not found" });
                }

                return Ok(permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting permission with code '{Code}'", code);
                return StatusCode(500, new { message = "An error occurred while retrieving the permission" });
            }
        }

        // POST: api/permissions
        [HttpPost]
        public async Task<IActionResult> CreatePermission([FromBody] Permission permission)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid model state", errors = ModelState });
            }

            try
            {
                var createdPermission = await _permissionService.CreatePermissionAsync(permission);
                _logger.LogInformation("Permission created successfully with ID: {PermissionId}", createdPermission.Id);
                return CreatedAtAction(nameof(GetById), new { id = createdPermission.Id },
                    new { message = "Permission created successfully", permission = createdPermission });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while creating permission");
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while creating permission");
                return StatusCode(500, new { message = "An error occurred while saving the permission to the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating permission");
                return StatusCode(500, new { message = "An unexpected error occurred while creating the permission" });
            }
        }

        // PUT: api/permissions/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePermission(int id, [FromBody] Permission updated)
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
                var permission = await _permissionService.UpdatePermissionAsync(id, updated);
                _logger.LogInformation("Permission updated successfully with ID: {PermissionId}", id);
                return Ok(new { message = "Permission updated successfully", permission });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Permission with ID {PermissionId} not found for update", id);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while updating permission with ID {PermissionId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating permission with ID {PermissionId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the permission in the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating permission with ID {PermissionId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred while updating the permission" });
            }
        }

        // DELETE: api/permissions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermission(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                var deleted = await _permissionService.DeletePermissionAsync(id);
                if (!deleted)
                {
                    _logger.LogWarning("Permission with ID {PermissionId} not found for deletion", id);
                    return NotFound(new { message = $"Permission with ID {id} not found" });
                }

                _logger.LogInformation("Permission deleted successfully with ID: {PermissionId}", id);
                return Ok(new { message = "Permission deleted successfully" });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while deleting permission with ID {PermissionId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the permission from the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting permission with ID {PermissionId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred while deleting the permission" });
            }
        }
    }
}

