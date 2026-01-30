using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.Services;
using System;

namespace server.Controllers
{
    [ApiController]
    [Route("api/responsibilities")]
    public class ResponsibilitiesController : ControllerBase
    {
        private readonly IResponsibilityService _responsibilityService;
        private readonly ILogger<ResponsibilitiesController> _logger;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;

        public ResponsibilitiesController(
            IResponsibilityService responsibilityService,
            ILogger<ResponsibilitiesController> logger,
            IUserService userService,
            IRoleService roleService)
        {
            _responsibilityService = responsibilityService;
            _logger = logger;
            _userService = userService;
            _roleService = roleService;
        }

        // GET: api/responsibilities/user/5?activeOnly=true
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Responsibility>>> GetByUser(int userId, [FromQuery] bool activeOnly = true)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new { message = "UserId must be greater than 0" });
                }

                var responsibilities = await _responsibilityService.GetResponsibilitiesByUserAsync(userId, activeOnly);
                return Ok(responsibilities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting responsibilities for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while retrieving responsibilities" });
            }
        }

        // GET: api/responsibilities/user/5/stock
        [HttpGet("user/{userId}/stock")]
        public async Task<ActionResult<IEnumerable<ResponsibilityStockItem>>> GetStockByUser(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new { message = "UserId must be greater than 0" });
                }

                var stock = await _responsibilityService.GetResponsibilityStockAsync(userId);
                return Ok(stock);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting responsibility stock for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while retrieving responsibility stock" });
            }
        }

        // GET: api/responsibilities/material/5?activeOnly=true
        [HttpGet("material/{materialId}")]
        public async Task<ActionResult<Responsibility>> GetByMaterial(int materialId, [FromQuery] bool activeOnly = true)
        {
            try
            {
                if (materialId <= 0)
                {
                    return BadRequest(new { message = "MaterialId must be greater than 0" });
                }

                var responsibility = await _responsibilityService.GetResponsibilityByMaterialAsync(materialId, activeOnly);
                if (responsibility == null)
                {
                    return NotFound(new { message = $"Responsibility for material {materialId} not found" });
                }

                return Ok(responsibility);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting responsibility for material {MaterialId}", materialId);
                return StatusCode(500, new { message = "An error occurred while retrieving responsibility" });
            }
        }

        // GET: api/responsibilities/product/5?activeOnly=true
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<Responsibility>> GetByProduct(int productId, [FromQuery] bool activeOnly = true)
        {
            try
            {
                if (productId <= 0)
                {
                    return BadRequest(new { message = "ProductId must be greater than 0" });
                }

                var responsibility = await _responsibilityService.GetResponsibilityByProductAsync(productId, activeOnly);
                if (responsibility == null)
                {
                    return NotFound(new { message = $"Responsibility for product {productId} not found" });
                }

                return Ok(responsibility);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting responsibility for product {ProductId}", productId);
                return StatusCode(500, new { message = "An error occurred while retrieving responsibility" });
            }
        }

        // GET: api/responsibilities/materials/active
        [HttpGet("materials/active")]
        public async Task<ActionResult<IEnumerable<ResponsibilityAssignment>>> GetActiveMaterialAssignments()
        {
            try
            {
                var assignments = await _responsibilityService.GetActiveMaterialAssignmentsAsync();
                return Ok(assignments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting active material responsibilities");
                return StatusCode(500, new { message = "An error occurred while retrieving responsibilities" });
            }
        }

        // GET: api/responsibilities/products/active
        [HttpGet("products/active")]
        public async Task<ActionResult<IEnumerable<ResponsibilityAssignment>>> GetActiveProductAssignments()
        {
            try
            {
                var assignments = await _responsibilityService.GetActiveProductAssignmentsAsync();
                return Ok(assignments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting active product responsibilities");
                return StatusCode(500, new { message = "An error occurred while retrieving responsibilities" });
            }
        }

        // POST: api/responsibilities/material/5/assign
        [HttpPost("/api/responsibilities/material/{materialId}/assign")]
        public async Task<IActionResult> AssignMaterial(int materialId, [FromBody] AssignResponsibilityRequest request)
        {
            try
            {
                if (materialId <= 0 || request.UserId <= 0)
                {
                    return BadRequest(new { message = "MaterialId and UserId must be greater than 0" });
                }

                if (!await IsPrivilegedUserAsync())
                {
                    return Forbid();
                }

                var responsibility = await _responsibilityService.AssignMaterialAsync(materialId, request.UserId, request.Quantity, request.MeasuringUnit);
                return Ok(new { message = "Responsibility assigned", responsibility });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Assign material responsibility failed");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning material responsibility");
                return StatusCode(500, new { message = "An error occurred while assigning responsibility" });
            }
        }

        // POST: api/responsibilities/product/5/assign
        [HttpPost("/api/responsibilities/product/{productId}/assign")]
        public async Task<IActionResult> AssignProduct(int productId, [FromBody] AssignResponsibilityRequest request)
        {
            try
            {
                if (productId <= 0 || request.UserId <= 0)
                {
                    return BadRequest(new { message = "ProductId and UserId must be greater than 0" });
                }

                if (!await IsPrivilegedUserAsync())
                {
                    return Forbid();
                }

                var responsibility = await _responsibilityService.AssignProductAsync(productId, request.UserId, request.Quantity, request.MeasuringUnit);
                return Ok(new { message = "Responsibility assigned", responsibility });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Assign product responsibility failed");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning product responsibility");
                return StatusCode(500, new { message = "An error occurred while assigning responsibility" });
            }
        }

        // POST: api/responsibilities/material/5/release
        [HttpPost("/api/responsibilities/material/{materialId}/release")]
        public async Task<IActionResult> ReleaseMaterial(int materialId)
        {
            try
            {
                if (materialId <= 0)
                {
                    return BadRequest(new { message = "MaterialId must be greater than 0" });
                }

                if (!await IsPrivilegedUserAsync())
                {
                    return Forbid();
                }

                var released = await _responsibilityService.ReleaseMaterialAsync(materialId);
                if (!released)
                {
                    return NotFound(new { message = $"Responsibility for material {materialId} not found" });
                }

                return Ok(new { message = "Responsibility released" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing material responsibility");
                return StatusCode(500, new { message = "An error occurred while releasing responsibility" });
            }
        }

        // POST: api/responsibilities/product/5/release
        [HttpPost("/api/responsibilities/product/{productId}/release")]
        public async Task<IActionResult> ReleaseProduct(int productId)
        {
            try
            {
                if (productId <= 0)
                {
                    return BadRequest(new { message = "ProductId must be greater than 0" });
                }

                if (!await IsPrivilegedUserAsync())
                {
                    return Forbid();
                }

                var released = await _responsibilityService.ReleaseProductAsync(productId);
                if (!released)
                {
                    return NotFound(new { message = $"Responsibility for product {productId} not found" });
                }

                return Ok(new { message = "Responsibility released" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing product responsibility");
                return StatusCode(500, new { message = "An error occurred while releasing responsibility" });
            }
        }

        private async Task<bool> IsPrivilegedUserAsync()
        {
            if (!Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) ||
                !int.TryParse(userIdHeader.ToString(), out var userId))
            {
                return false;
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            if (user.Login.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (user.RoleId.HasValue)
            {
                var role = await _roleService.GetRoleByIdAsync(user.RoleId.Value);
                if (role != null &&
                    (role.Code == "Owner" || role.Code == "Admin" ||
                     role.Name == "Владелец" || role.Name == "Администратор"))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class AssignResponsibilityRequest
    {
        public int UserId { get; set; }
        public int? Quantity { get; set; }
        public string? MeasuringUnit { get; set; }
    }
}
