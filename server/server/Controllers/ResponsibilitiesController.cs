using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;
using server.Services;
using System;

namespace server.Controllers
{
    [ApiController]
    [Route("api/responsibilities")]
    public class ResponsibilitiesController : ControllerBase
    {
        private readonly IResponsibilityFillingService _responsibilityFillingService;
        private readonly ILogger<ResponsibilitiesController> _logger;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IUserPermissionsService _userPermissionsService;
        private readonly AppDbContext _context;

        public ResponsibilitiesController(
            IResponsibilityFillingService responsibilityFillingService,
            ILogger<ResponsibilitiesController> logger,
            IUserService userService,
            IRoleService roleService,
            IUserPermissionsService userPermissionsService,
            AppDbContext context)
        {
            _responsibilityFillingService = responsibilityFillingService;
            _logger = logger;
            _userService = userService;
            _roleService = roleService;
            _userPermissionsService = userPermissionsService;
            _context = context;
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

                var stock = await _responsibilityFillingService.GetResponsibilityStockForUserAsync(userId);
                return Ok(stock);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting responsibility stock for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while retrieving responsibility stock" });
            }
        }

        // GET: api/responsibilities/materials/active
        [HttpGet("materials/active")]
        public async Task<ActionResult<IEnumerable<ResponsibilityAssignment>>> GetActiveMaterialAssignments()
        {
            try
            {
                var assignments = await _responsibilityFillingService.GetActiveMaterialAssignmentsFromFillingAsync();
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
                var assignments = await _responsibilityFillingService.GetActiveProductAssignmentsFromFillingAsync();
                return Ok(assignments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting active product responsibilities");
                return StatusCode(500, new { message = "An error occurred while retrieving responsibilities" });
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

        // POST: api/responsibilities/filling/material/transfer
        [HttpPost("filling/material/transfer")]
        public async Task<IActionResult> TransferMaterialFilling([FromBody] TransferResponsibilityFillingRequest request)
        {
            try
            {
                if (!await IsPrivilegedUserAsync())
                    return Forbid();
                if (request.WarehouseId <= 0 || request.MaterialId <= 0 || request.FromUserId <= 0 || request.ToUserId <= 0)
                    return BadRequest(new { message = "WarehouseId, MaterialId, FromUserId and ToUserId required" });
                await _responsibilityFillingService.TransferMaterialResponsibilityAsync(
                    request.WarehouseId, request.MaterialId, request.FromUserId, request.ToUserId, request.QuantityToTransfer);
                return Ok(new { message = "Responsibility transferred" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transferring material responsibility");
                return StatusCode(500, new { message = "An error occurred while transferring responsibility", detail = ex.Message });
            }
        }

        // POST: api/responsibilities/filling/product/transfer
        [HttpPost("filling/product/transfer")]
        public async Task<IActionResult> TransferProductFilling([FromBody] TransferResponsibilityFillingProductRequest request)
        {
            try
            {
                if (!await IsPrivilegedUserAsync())
                    return Forbid();
                if (request.WarehouseId <= 0 || request.ProductId <= 0 || request.FromUserId <= 0 || request.ToUserId <= 0)
                    return BadRequest(new { message = "WarehouseId, ProductId, FromUserId and ToUserId required" });
                await _responsibilityFillingService.TransferProductResponsibilityAsync(
                    request.WarehouseId, request.ProductId, request.FromUserId, request.ToUserId, request.QuantityToTransfer);
                return Ok(new { message = "Responsibility transferred" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transferring product responsibility");
                return StatusCode(500, new { message = "An error occurred while transferring responsibility", detail = ex.Message });
            }
        }

        // POST: api/responsibilities/filling/batch/transfer
        [HttpPost("filling/batch/transfer")]
        public async Task<IActionResult> TransferBatchFilling([FromBody] TransferBatchResponsibilityRequest request)
        {
            try
            {
                if (!await IsPrivilegedUserAsync())
                    return Forbid();
                if (request.BatchId <= 0 || request.FromUserId <= 0 || request.ToUserId <= 0)
                    return BadRequest(new { message = "BatchId, FromUserId and ToUserId required" });
                await _responsibilityFillingService.TransferBatchResponsibilityAsync(
                    request.BatchId, request.FromUserId, request.ToUserId, request.QuantityToTransfer);
                return Ok(new { message = "Responsibility transferred" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transferring batch responsibility");
                return StatusCode(500, new { message = "An error occurred while transferring responsibility", detail = ex.Message });
            }
        }

        // POST: api/responsibilities/filling/batch/release
        [HttpPost("filling/batch/release")]
        public async Task<IActionResult> ReleaseBatchFilling([FromBody] ReleaseBatchResponsibilityRequest request)
        {
            try
            {
                if (!await IsPrivilegedUserAsync())
                    return Forbid();
                if (request.BatchId <= 0 || request.UserId <= 0)
                    return BadRequest(new { message = "BatchId and UserId required" });
                
                if (request.QuantityToRelease.HasValue && request.QuantityToRelease.Value > 0)
                {
                    await _responsibilityFillingService.DecreaseBatchResponsibilityAsync(request.BatchId, request.QuantityToRelease.Value, request.UserId);
                }
                else
                {
                    await _responsibilityFillingService.ReleaseBatchResponsibilityAsync(request.BatchId, request.UserId);
                }
                return Ok(new { message = "Responsibility released" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing batch responsibility");
                return StatusCode(500, new { message = "An error occurred while releasing responsibility", detail = ex.Message });
            }
        }

        // POST: api/responsibilities/filling/material/release
        [HttpPost("filling/material/release")]
        public async Task<IActionResult> ReleaseMaterialFilling([FromBody] ReleaseMaterialResponsibilityRequest request)
        {
            try
            {
                if (!await IsPrivilegedUserAsync())
                    return Forbid();
                if (request.WarehouseId <= 0 || request.MaterialId <= 0 || request.UserId <= 0)
                    return BadRequest(new { message = "WarehouseId, MaterialId and UserId required" });
                
                if (request.QuantityToRelease.HasValue && request.QuantityToRelease.Value > 0)
                {
                    await _responsibilityFillingService.DecreaseMaterialResponsibilityAtWarehouseAsync(
                        request.WarehouseId, request.MaterialId, request.QuantityToRelease.Value, request.UserId);
                }
                else
                {
                    // Снимаем всю ответственность пользователя за материал на складе
                    var totalQty = await _responsibilityFillingService.GetUserResponsibleQuantityAtWarehouseAsync(
                        request.UserId, request.WarehouseId, request.MaterialId);
                    if (totalQty > 0)
                    {
                        await _responsibilityFillingService.DecreaseMaterialResponsibilityAtWarehouseAsync(
                            request.WarehouseId, request.MaterialId, totalQty, request.UserId);
                    }
                }
                return Ok(new { message = "Responsibility released" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing material responsibility");
                return StatusCode(500, new { message = "An error occurred while releasing responsibility", detail = ex.Message });
            }
        }

        // PUT: api/responsibilities/filling/material/update
        [HttpPut("filling/material/update")]
        public async Task<IActionResult> UpdateMaterialFilling([FromBody] UpdateMaterialResponsibilityRequest request)
        {
            try
            {
                if (!await IsPrivilegedUserAsync())
                    return Forbid();
                if (request.WarehouseId <= 0 || request.MaterialId <= 0 || request.UserId <= 0 || request.NewQuantity < 0)
                    return BadRequest(new { message = "WarehouseId, MaterialId, UserId and non-negative NewQuantity required" });
                
                // Получаем текущее количество ответственности
                var currentQty = await _responsibilityFillingService.GetUserResponsibleQuantityAtWarehouseAsync(
                    request.UserId, request.WarehouseId, request.MaterialId);
                
                if (request.NewQuantity > currentQty)
                {
                    // Увеличиваем ответственность
                    var diff = request.NewQuantity - currentQty;
                    await _responsibilityFillingService.AssignMaterialAtWarehouseAsync(
                        request.UserId, request.WarehouseId, request.MaterialId, diff, request.MeasuringUnit);
                }
                else if (request.NewQuantity < currentQty)
                {
                    // Уменьшаем ответственность
                    var diff = currentQty - request.NewQuantity;
                    await _responsibilityFillingService.DecreaseMaterialResponsibilityAtWarehouseAsync(
                        request.WarehouseId, request.MaterialId, diff, request.UserId);
                }
                // Если равны, ничего не делаем
                
                return Ok(new { message = "Responsibility updated" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating material responsibility");
                return StatusCode(500, new { message = "An error occurred while updating responsibility", detail = ex.Message });
            }
        }

        // DELETE: api/responsibilities/filling/{id}
        [HttpDelete("filling/{id}")]
        public async Task<IActionResult> DeleteResponsibilityFilling(int id)
        {
            try
            {
                if (!await IsPrivilegedUserAsync())
                    return Forbid();
                if (id <= 0)
                    return BadRequest(new { message = "Id must be greater than 0" });
                
                var filling = await _context.ResponsibilityFillings.FindAsync(id);
                if (filling == null)
                    return NotFound(new { message = "ResponsibilityFilling not found" });
                
                // Помечаем как неактивную и уменьшаем количество на складе
                filling.IsActive = false;
                filling.ReleasedAt = DateTime.UtcNow;
                
                // Уменьшаем количество на складе
                if (filling.MaterialId.HasValue)
                {
                    var materialFilling = await _context.FillingWarehouses
                        .FirstOrDefaultAsync(fw => fw.WarehouseId == filling.WarehouseId && fw.MaterialId == filling.MaterialId);
                    if (materialFilling != null)
                    {
                        materialFilling.Quantity = Math.Max(0, materialFilling.Quantity - filling.Quantity);
                    }
                }
                else if (filling.ProductId.HasValue)
                {
                    var productFilling = await _context.FillingWarehouses
                        .FirstOrDefaultAsync(fw => fw.WarehouseId == filling.WarehouseId && fw.ProductId == filling.ProductId);
                    if (productFilling != null)
                    {
                        productFilling.Quantity = Math.Max(0, productFilling.Quantity - filling.Quantity);
                    }
                }
                
                await _context.SaveChangesAsync();
                return Ok(new { message = "ResponsibilityFilling deleted" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting responsibility filling");
                return StatusCode(500, new { message = "An error occurred while deleting responsibility filling", detail = ex.Message });
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
                    (string.Equals(role.Code, "Owner", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(role.Code, "Admin", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(role.Name, "Владелец", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(role.Name, "Администратор", StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            if (await _userPermissionsService.HasPermissionAsync(userId, "ManageResponsibility"))
            {
                return true;
            }

            return false;
        }
    }

    public class AssignResponsibilityFillingRequest
    {
        public int UserId { get; set; }
        public int WarehouseId { get; set; }
        public int MaterialId { get; set; }
        public double Quantity { get; set; }
        public string? MeasuringUnit { get; set; }
    }

    public class AssignResponsibilityFillingProductRequest
    {
        public int UserId { get; set; }
        public int WarehouseId { get; set; }
        public int ProductId { get; set; }
        public double Quantity { get; set; }
        public string? MeasuringUnit { get; set; }
    }

    public class TransferResponsibilityFillingRequest
    {
        public int WarehouseId { get; set; }
        public int MaterialId { get; set; }
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        /// <summary>Сколько передать; null или не указано — передать всё.</summary>
        public double? QuantityToTransfer { get; set; }
    }

    public class TransferResponsibilityFillingProductRequest
    {
        public int WarehouseId { get; set; }
        public int ProductId { get; set; }
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public double? QuantityToTransfer { get; set; }
    }

    public class TransferBatchResponsibilityRequest
    {
        public int BatchId { get; set; }
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public double? QuantityToTransfer { get; set; }
    }

    public class ReleaseBatchResponsibilityRequest
    {
        public int BatchId { get; set; }
        public int UserId { get; set; }
        public double? QuantityToRelease { get; set; }
    }

    public class ReleaseMaterialResponsibilityRequest
    {
        public int WarehouseId { get; set; }
        public int MaterialId { get; set; }
        public int UserId { get; set; }
        public double? QuantityToRelease { get; set; }
    }

    public class UpdateMaterialResponsibilityRequest
    {
        public int WarehouseId { get; set; }
        public int MaterialId { get; set; }
        public int UserId { get; set; }
        public double NewQuantity { get; set; }
        public string? MeasuringUnit { get; set; }
    }
}
