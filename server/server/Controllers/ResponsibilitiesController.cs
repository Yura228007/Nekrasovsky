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
        private readonly IResponsibilityFillingService _responsibilityFillingService;
        private readonly ILogger<ResponsibilitiesController> _logger;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;

        public ResponsibilitiesController(
            IResponsibilityService responsibilityService,
            IResponsibilityFillingService responsibilityFillingService,
            ILogger<ResponsibilitiesController> logger,
            IUserService userService,
            IRoleService roleService)
        {
            _responsibilityService = responsibilityService;
            _responsibilityFillingService = responsibilityFillingService;
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
                var fromFilling = await _responsibilityFillingService.GetActiveMaterialAssignmentsFromFillingAsync();
                var fromResponsibility = await _responsibilityService.GetActiveMaterialAssignmentsAsync();
                var merged = MergeAssignments(fromFilling, fromResponsibility);
                return Ok(merged);
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
                var fromFilling = await _responsibilityFillingService.GetActiveProductAssignmentsFromFillingAsync();
                var fromResponsibility = await _responsibilityService.GetActiveProductAssignmentsAsync();
                var merged = MergeAssignments(fromFilling, fromResponsibility);
                return Ok(merged);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting active product responsibilities");
                return StatusCode(500, new { message = "An error occurred while retrieving responsibilities" });
            }
        }

        private static List<ResponsibilityAssignment> MergeAssignments(
            List<ResponsibilityAssignment> fromFilling,
            List<ResponsibilityAssignment> fromResponsibility)
        {
            var dict = new Dictionary<(int ItemId, int UserId), ResponsibilityAssignment>();

            foreach (var a in fromFilling)
            {
                var k = (a.ItemId, a.UserId);
                dict[k] = new ResponsibilityAssignment
                {
                    ItemId = a.ItemId,
                    UserId = a.UserId,
                    UserName = a.UserName,
                    Quantity = a.Quantity ?? 0,
                    MeasuringUnit = a.MeasuringUnit
                };
            }

            foreach (var a in fromResponsibility)
            {
                var k = (a.ItemId, a.UserId);
                if (dict.TryGetValue(k, out var existing))
                {
                    existing.Quantity = (existing.Quantity ?? 0) + (a.Quantity ?? 0);
                    if (string.IsNullOrWhiteSpace(existing.MeasuringUnit) && !string.IsNullOrWhiteSpace(a.MeasuringUnit))
                        existing.MeasuringUnit = a.MeasuringUnit;
                }
                else
                {
                    dict[k] = new ResponsibilityAssignment
                    {
                        ItemId = a.ItemId,
                        UserId = a.UserId,
                        UserName = a.UserName,
                        Quantity = a.Quantity,
                        MeasuringUnit = a.MeasuringUnit
                    };
                }
            }

            return dict.Values.ToList();
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

        // =============================
        // ResponsibilityFilling (ответственность по складам)
        // =============================

        // GET: api/responsibilities/filling/user/5
        [HttpGet("filling/user/{userId}")]
        public async Task<ActionResult<IEnumerable<ResponsibilityFilling>>> GetFillingByUser(int userId)
        {
            try
            {
                if (userId <= 0)
                    return BadRequest(new { message = "UserId must be greater than 0" });
                var list = await _responsibilityFillingService.GetResponsibilityFillingsByUserAsync(userId);
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting responsibility fillings for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while retrieving responsibility fillings" });
            }
        }

        // GET: api/responsibilities/filling/warehouse/5/material/10
        [HttpGet("filling/warehouse/{warehouseId}/material/{materialId}")]
        public async Task<ActionResult<IEnumerable<ResponsibilityFilling>>> GetFillingByWarehouseAndMaterial(int warehouseId, int materialId)
        {
            try
            {
                var list = await _responsibilityFillingService.GetByWarehouseAndMaterialAsync(warehouseId, materialId);
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting responsibility fillings for warehouse {WarehouseId} material {MaterialId}", warehouseId, materialId);
                return StatusCode(500, new { message = "An error occurred while retrieving responsibility fillings" });
            }
        }

        // POST: api/responsibilities/filling/material
        [HttpPost("filling/material")]
        public async Task<ActionResult<ResponsibilityFilling>> AssignMaterialFilling([FromBody] AssignResponsibilityFillingRequest request)
        {
            try
            {
                if (!await IsPrivilegedUserAsync())
                    return Forbid();
                if (request.UserId <= 0 || request.WarehouseId <= 0 || request.MaterialId <= 0 || request.Quantity <= 0)
                    return BadRequest(new { message = "UserId, WarehouseId, MaterialId and positive Quantity required" });
                var rf = await _responsibilityFillingService.AssignMaterialAtWarehouseAsync(
                    request.UserId, request.WarehouseId, request.MaterialId, request.Quantity, request.MeasuringUnit);
                return Ok(new { message = "Responsibility filling assigned", responsibilityFilling = rf });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning material responsibility filling");
                return StatusCode(500, new { message = "An error occurred while assigning responsibility filling" });
            }
        }

        // POST: api/responsibilities/filling/product
        [HttpPost("filling/product")]
        public async Task<ActionResult<ResponsibilityFilling>> AssignProductFilling([FromBody] AssignResponsibilityFillingProductRequest request)
        {
            try
            {
                if (!await IsPrivilegedUserAsync())
                    return Forbid();
                if (request.UserId <= 0 || request.WarehouseId <= 0 || request.ProductId <= 0 || request.Quantity <= 0)
                    return BadRequest(new { message = "UserId, WarehouseId, ProductId and positive Quantity required" });
                var rf = await _responsibilityFillingService.AssignProductAtWarehouseAsync(
                    request.UserId, request.WarehouseId, request.ProductId, request.Quantity, request.MeasuringUnit);
                return Ok(new { message = "Responsibility filling assigned", responsibilityFilling = rf });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning product responsibility filling");
                return StatusCode(500, new { message = "An error occurred while assigning responsibility filling" });
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

    public class AssignResponsibilityFillingRequest
    {
        public int UserId { get; set; }
        public int WarehouseId { get; set; }
        public int MaterialId { get; set; }
        public int Quantity { get; set; }
        public string? MeasuringUnit { get; set; }
    }

    public class AssignResponsibilityFillingProductRequest
    {
        public int UserId { get; set; }
        public int WarehouseId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string? MeasuringUnit { get; set; }
    }
}
