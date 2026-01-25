using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("api/responsibilities")]
    public class ResponsibilitiesController : ControllerBase
    {
        private readonly IResponsibilityService _responsibilityService;
        private readonly ILogger<ResponsibilitiesController> _logger;

        public ResponsibilitiesController(IResponsibilityService responsibilityService, ILogger<ResponsibilitiesController> logger)
        {
            _responsibilityService = responsibilityService;
            _logger = logger;
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
    }
}
