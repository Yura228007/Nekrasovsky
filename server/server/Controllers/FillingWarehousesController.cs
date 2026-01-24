using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FillingWarehousesController : ControllerBase
    {
        private readonly IFillingWarehouseService _fillingWarehouseService;
        private readonly ILogger<FillingWarehousesController> _logger;

        public FillingWarehousesController(IFillingWarehouseService fillingWarehouseService, ILogger<FillingWarehousesController> logger)
        {
            _fillingWarehouseService = fillingWarehouseService;
            _logger = logger;
        }

        // GET: api/filling-warehouses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FillingWarehouse>>> GetAll()
        {
            try
            {
                var fillings = await _fillingWarehouseService.GetAllFillingWarehousesAsync();
                return Ok(fillings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all filling warehouses");
                return StatusCode(500, new { message = "An error occurred while retrieving filling warehouses" });
            }
        }

        // GET: api/filling-warehouses/{warehouseId}/{materialId}
        [HttpGet("{warehouseId}/{materialId}")]
        public async Task<ActionResult<FillingWarehouse>> GetFillingMaterial(int warehouseId, int materialId)
        {
            try
            {
                if (warehouseId <= 0 || materialId <= 0)
                {
                    return BadRequest(new { message = "All IDs must be greater than 0" });
                }

                var filling = await _fillingWarehouseService.GetFillingByMaterialAsync(warehouseId, materialId);
                if (filling == null)
                {
                    _logger.LogWarning("FillingWarehouse not found for WarehouseId {WarehouseId} and MaterialId {MaterialId}", warehouseId, materialId);
                    return NotFound(new { message = "FillingWarehouse not found" });
                }

                return Ok(filling);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting filling warehouse");
                return StatusCode(500, new { message = "An error occurred while retrieving the filling warehouse" });
            }
        }

        // GET: api/filling-warehouses/{warehouseId}/product/{productId}
        [HttpGet("{warehouseId}/product/{productId}")]
        public async Task<ActionResult<FillingWarehouse>> GetFillingProduct(int warehouseId, int productId)
        {
            try
            {
                if (warehouseId <= 0 || productId <= 0)
                {
                    return BadRequest(new { message = "All IDs must be greater than 0" });
                }

                var filling = await _fillingWarehouseService.GetFillingByProductAsync(warehouseId, productId);
                if (filling == null)
                {
                    _logger.LogWarning("FillingWarehouse not found for WarehouseId {WarehouseId} and ProductId {ProductId}", warehouseId, productId);
                    return NotFound(new { message = "FillingWarehouse not found" });
                }

                return Ok(filling);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting filling warehouse");
                return StatusCode(500, new { message = "An error occurred while retrieving the filling warehouse" });
            }
        }

        // GET: api/filling-warehouses/warehouse/{warehouseId}
        [HttpGet("warehouse/{warehouseId}")]
        public async Task<ActionResult<IEnumerable<FillingWarehouse>>> GetByWarehouse(int warehouseId)
        {
            try
            {
                if (warehouseId <= 0)
                {
                    return BadRequest(new { message = "WarehouseId must be greater than 0" });
                }

                var fillings = await _fillingWarehouseService.GetFillingByWarehouseAsync(warehouseId);
                return Ok(fillings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting filling warehouses for warehouse {WarehouseId}", warehouseId);
                return StatusCode(500, new { message = "An error occurred while retrieving filling warehouses" });
            }
        }

        // GET: api/filling-warehouses/material/{materialId}
        [HttpGet("material/{materialId}")]
        public async Task<ActionResult<IEnumerable<FillingWarehouse>>> GetByMaterial(int materialId)
        {
            try
            {
                if (materialId <= 0)
                {
                    return BadRequest(new { message = "MaterialId must be greater than 0" });
                }

                var fillings = await _fillingWarehouseService.GetFillingsByMaterialAsync(materialId);
                return Ok(fillings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting filling warehouses for material {MaterialId}", materialId);
                return StatusCode(500, new { message = "An error occurred while retrieving filling warehouses" });
            }
        }

        // GET: api/filling-warehouses/product/{productId}
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<FillingWarehouse>>> GetByProduct(int productId)
        {
            try
            {
                if (productId <= 0)
                {
                    return BadRequest(new { message = "ProductId must be greater than 0" });
                }

                var fillings = await _fillingWarehouseService.GetFillingsByProductAsync(productId);
                return Ok(fillings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting filling warehouses for product {ProductId}", productId);
                return StatusCode(500, new { message = "An error occurred while retrieving filling warehouses" });
            }
        }

        // POST: api/filling-warehouses
        [HttpPost]
        public async Task<IActionResult> CreateFilling([FromBody] FillingWarehouse filling)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid model state", errors = ModelState });
            }

            try
            {
                if (filling.WarehouseId <= 0)
                {
                    return BadRequest(new { message = "All IDs must be greater than 0" });
                }

                if (!IsValidXor(filling.MaterialId, filling.ProductId))
                {
                    return BadRequest(new { message = "Either MaterialId or ProductId must be set, but not both." });
                }
                if ((filling.MaterialId.HasValue && filling.MaterialId.Value <= 0) ||
                    (filling.ProductId.HasValue && filling.ProductId.Value <= 0))
                {
                    return BadRequest(new { message = "All IDs must be greater than 0" });
                }

                var createdFilling = await _fillingWarehouseService.CreateFillingWarehouseAsync(filling);
                _logger.LogInformation("FillingWarehouse created successfully for WarehouseId {WarehouseId}, MaterialId {MaterialId}, ProductId {ProductId}", filling.WarehouseId, filling.MaterialId, filling.ProductId);
                return Ok(new { message = "FillingWarehouse created successfully", filling = createdFilling });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Key not found while creating filling warehouse");
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while creating filling warehouse");
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while creating filling warehouse");
                return StatusCode(500, new { message = "An error occurred while saving the filling warehouse to the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating filling warehouse");
                return StatusCode(500, new { message = "An unexpected error occurred while creating the filling warehouse" });
            }
        }

        // POST: api/filling-warehouses/edit/{warehouseId}/{materialId} - POST because of composite key
        [HttpPost("edit/{warehouseId}/{materialId}")]
        public async Task<IActionResult> UpdateFillingMaterial(int warehouseId, int materialId, [FromBody] FillingWarehouse updated)
        {
            if (warehouseId <= 0 || materialId <= 0)
            {
                return BadRequest(new { message = "All IDs must be greater than 0" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid model state", errors = ModelState });
            }

            try
            {
                var filling = await _fillingWarehouseService.UpdateFillingWarehouseByMaterialAsync(warehouseId, materialId, updated);
                _logger.LogInformation("FillingWarehouse updated successfully for WarehouseId {WarehouseId} and MaterialId {MaterialId}", warehouseId, materialId);
                return Ok(new { message = "FillingWarehouse updated successfully", filling });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "FillingWarehouse not found for update with WarehouseId {WarehouseId} and MaterialId {MaterialId}", warehouseId, materialId);
                return NotFound(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating filling warehouse");
                return StatusCode(500, new { message = "An error occurred while updating the filling warehouse in the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating filling warehouse");
                return StatusCode(500, new { message = "An unexpected error occurred while updating the filling warehouse" });
            }
        }

        // POST: api/filling-warehouses/edit-product/{warehouseId}/{productId}
        [HttpPost("edit-product/{warehouseId}/{productId}")]
        public async Task<IActionResult> UpdateFillingProduct(int warehouseId, int productId, [FromBody] FillingWarehouse updated)
        {
            if (warehouseId <= 0 || productId <= 0)
            {
                return BadRequest(new { message = "All IDs must be greater than 0" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid model state", errors = ModelState });
            }

            try
            {
                var filling = await _fillingWarehouseService.UpdateFillingWarehouseByProductAsync(warehouseId, productId, updated);
                _logger.LogInformation("FillingWarehouse updated successfully for WarehouseId {WarehouseId} and ProductId {ProductId}", warehouseId, productId);
                return Ok(new { message = "FillingWarehouse updated successfully", filling });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "FillingWarehouse not found for update with WarehouseId {WarehouseId} and ProductId {ProductId}", warehouseId, productId);
                return NotFound(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating filling warehouse");
                return StatusCode(500, new { message = "An error occurred while updating the filling warehouse in the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating filling warehouse");
                return StatusCode(500, new { message = "An unexpected error occurred while updating the filling warehouse" });
            }
        }

        // POST: api/filling-warehouses/delete/{warehouseId}/{materialId} - POST because of composite key
        [HttpPost("delete/{warehouseId}/{materialId}")]
        public async Task<IActionResult> DeleteFillingMaterial(int warehouseId, int materialId)
        {
            try
            {
                if (warehouseId <= 0 || materialId <= 0)
                {
                    return BadRequest(new { message = "All IDs must be greater than 0" });
                }

                var deleted = await _fillingWarehouseService.DeleteFillingWarehouseByMaterialAsync(warehouseId, materialId);
                if (!deleted)
                {
                    _logger.LogWarning("FillingWarehouse not found for deletion with WarehouseId {WarehouseId} and MaterialId {MaterialId}", warehouseId, materialId);
                    return NotFound(new { message = "FillingWarehouse not found" });
                }

                _logger.LogInformation("FillingWarehouse deleted successfully for WarehouseId {WarehouseId} and MaterialId {MaterialId}", warehouseId, materialId);
                return Ok(new { message = "FillingWarehouse deleted successfully" });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while deleting filling warehouse");
                return StatusCode(500, new { message = "An error occurred while deleting the filling warehouse from the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting filling warehouse");
                return StatusCode(500, new { message = "An unexpected error occurred while deleting the filling warehouse" });
            }
        }

        // POST: api/filling-warehouses/delete-product/{warehouseId}/{productId}
        [HttpPost("delete-product/{warehouseId}/{productId}")]
        public async Task<IActionResult> DeleteFillingProduct(int warehouseId, int productId)
        {
            try
            {
                if (warehouseId <= 0 || productId <= 0)
                {
                    return BadRequest(new { message = "All IDs must be greater than 0" });
                }

                var deleted = await _fillingWarehouseService.DeleteFillingWarehouseByProductAsync(warehouseId, productId);
                if (!deleted)
                {
                    _logger.LogWarning("FillingWarehouse not found for deletion with WarehouseId {WarehouseId} and ProductId {ProductId}", warehouseId, productId);
                    return NotFound(new { message = "FillingWarehouse not found" });
                }

                _logger.LogInformation("FillingWarehouse deleted successfully for WarehouseId {WarehouseId} and ProductId {ProductId}", warehouseId, productId);
                return Ok(new { message = "FillingWarehouse deleted successfully" });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while deleting filling warehouse");
                return StatusCode(500, new { message = "An error occurred while deleting the filling warehouse from the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting filling warehouse");
                return StatusCode(500, new { message = "An unexpected error occurred while deleting the filling warehouse" });
            }
        }

        // POST: api/filling-warehouses/update-quantity
        [HttpPost("update-quantity")]
        public async Task<IActionResult> UpdateQuantityMaterial([FromBody] FillingWarehouse filling)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid model state", errors = ModelState });
            }

            try
            {
                if (filling.WarehouseId <= 0 || !filling.MaterialId.HasValue || filling.MaterialId.Value <= 0)
                {
                    return BadRequest(new { message = "All IDs must be greater than 0" });
                }

                var updatedFilling = await _fillingWarehouseService.UpdateQuantityByMaterialAsync(
                    filling.WarehouseId,
                    filling.MaterialId.Value,
                    filling.Quantity);
                _logger.LogInformation("Quantity updated successfully for WarehouseId {WarehouseId} and MaterialId {MaterialId}", filling.WarehouseId, filling.MaterialId);
                return Ok(new { message = "Quantity updated successfully", filling = updatedFilling });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "FillingWarehouse not found for quantity update");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating quantity");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        // POST: api/filling-warehouses/update-quantity-product
        [HttpPost("update-quantity-product")]
        public async Task<IActionResult> UpdateQuantityProduct([FromBody] FillingWarehouse filling)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid model state", errors = ModelState });
            }

            try
            {
                if (filling.WarehouseId <= 0 || !filling.ProductId.HasValue || filling.ProductId.Value <= 0)
                {
                    return BadRequest(new { message = "All IDs must be greater than 0" });
                }

                var updatedFilling = await _fillingWarehouseService.UpdateQuantityByProductAsync(
                    filling.WarehouseId,
                    filling.ProductId.Value,
                    filling.Quantity);
                _logger.LogInformation("Quantity updated successfully for WarehouseId {WarehouseId} and ProductId {ProductId}", filling.WarehouseId, filling.ProductId);
                return Ok(new { message = "Quantity updated successfully", filling = updatedFilling });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "FillingWarehouse not found for quantity update");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating quantity");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        // GET: api/filling-warehouses/warehouse/{warehouseId}/stock
        [HttpGet("warehouse/{warehouseId}/stock")]
        public async Task<ActionResult<IEnumerable<FillingWarehouse>>> GetStock(int warehouseId)
        {
            try
            {
                if (warehouseId <= 0)
                {
                    return BadRequest(new { message = "WarehouseId must be greater than 0" });
                }

                var stock = await _fillingWarehouseService.GetWarehouseStockAsync(warehouseId);
                return Ok(stock);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting stock for warehouse {WarehouseId}", warehouseId);
                return StatusCode(500, new { message = "An error occurred while retrieving stock" });
            }
        }

        private static bool IsValidXor(int? materialId, int? productId)
        {
            return (materialId.HasValue && !productId.HasValue) || (!materialId.HasValue && productId.HasValue);
        }
    }
}
