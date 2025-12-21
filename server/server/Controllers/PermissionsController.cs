using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionsController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        // GET: api/permissions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Permission>>> GetAll()
        {
            var permissions = await _permissionService.GetAllPermissionsAsync();
            return Ok(permissions);
        }

        // GET: api/permissions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Permission>> GetById(int id)
        {
            var permission = await _permissionService.GetPermissionByIdAsync(id);
            if (permission == null)
                return NotFound($"Permission with ID {id} not found");

            return Ok(permission);
        }

        // GET: api/permissions/code/{code}
        [HttpGet("code/{code}")]
        public async Task<ActionResult<Permission>> GetByCode(string code)
        {
            var permission = await _permissionService.GetPermissionByCodeAsync(code);
            if (permission == null)
                return NotFound($"Permission with code '{code}' not found");

            return Ok(permission);
        }

        // POST: api/permissions/add
        [HttpPost("add")]
        public async Task<IActionResult> AddPermission([FromBody] Permission permission)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdPermission = await _permissionService.CreatePermissionAsync(permission);
                return Ok(new { message = "Permission created successfully", permission = createdPermission });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/permissions/edit/{id}
        [HttpPost("edit/{id}")]
        public async Task<IActionResult> EditPermission(int id, [FromBody] Permission updated)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var permission = await _permissionService.UpdatePermissionAsync(id, updated);
                return Ok(new { message = "Permission updated successfully", permission });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/permissions/delete/{id}
        [HttpPost("delete/{id}")]
        public async Task<IActionResult> DeletePermission(int id)
        {
            var deleted = await _permissionService.DeletePermissionAsync(id);
            if (!deleted)
                return NotFound($"Permission with ID {id} not found");

            return Ok(new { message = "Permission deleted successfully" });
        }
    }
}

