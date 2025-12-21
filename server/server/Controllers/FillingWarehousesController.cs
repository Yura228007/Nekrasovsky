using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FillingWarehousesController : ControllerBase
    {
        private readonly IFillingWarehouseService _fillingWarehouseService;

        public FillingWarehousesController(IFillingWarehouseService fillingWarehouseService)
        {
            _fillingWarehouseService = fillingWarehouseService;
        }

        // GET: api/filling-warehouses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FillingWarehouse>>> GetAll()
        {
            var fillings = await _fillingWarehouseService.GetAllFillingWarehousesAsync();
            return Ok(fillings);
        }

        // GET: api/filling-warehouses/{warehouseId}/{materialId}
        [HttpGet("{warehouseId}/{materialId}")]
        public async Task<ActionResult<FillingWarehouse>> GetFilling(int warehouseId, int materialId)
        {
            var filling = await _fillingWarehouseService.GetFillingWarehouseAsync(warehouseId, materialId);
            if (filling == null)
                return NotFound($"FillingWarehouse not found");

            return Ok(filling);
        }

        // GET: api/filling-warehouses/warehouse/{warehouseId}
        [HttpGet("warehouse/{warehouseId}")]
        public async Task<ActionResult<IEnumerable<FillingWarehouse>>> GetByWarehouse(int warehouseId)
        {
            var fillings = await _fillingWarehouseService.GetFillingByWarehouseAsync(warehouseId);
            return Ok(fillings);
        }

        // GET: api/filling-warehouses/material/{materialId}
        [HttpGet("material/{materialId}")]
        public async Task<ActionResult<IEnumerable<FillingWarehouse>>> GetByMaterial(int materialId)
        {
            var fillings = await _fillingWarehouseService.GetFillingByMaterialAsync(materialId);
            return Ok(fillings);
        }

        // POST: api/filling-warehouses/add
        [HttpPost("add")]
        public async Task<IActionResult> AddFilling([FromBody] FillingWarehouse filling)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdFilling = await _fillingWarehouseService.CreateFillingWarehouseAsync(filling);
                return Ok(new { message = "FillingWarehouse created successfully", filling = createdFilling });
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

        // POST: api/filling-warehouses/edit/{warehouseId}/{materialId}
        [HttpPost("edit/{warehouseId}/{materialId}")]
        public async Task<IActionResult> EditFilling(int warehouseId, int materialId, [FromBody] FillingWarehouse updated)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var filling = await _fillingWarehouseService.UpdateFillingWarehouseAsync(warehouseId, materialId, updated);
                return Ok(new { message = "FillingWarehouse updated successfully", filling });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/filling-warehouses/delete/{warehouseId}/{materialId}
        [HttpPost("delete/{warehouseId}/{materialId}")]
        public async Task<IActionResult> DeleteFilling(int warehouseId, int materialId)
        {
            var deleted = await _fillingWarehouseService.DeleteFillingWarehouseAsync(warehouseId, materialId);
            if (!deleted)
                return NotFound($"FillingWarehouse not found");

            return Ok(new { message = "FillingWarehouse deleted successfully" });
        }

        // POST: api/filling-warehouses/update-quantity
        [HttpPost("update-quantity")]
        public async Task<IActionResult> UpdateQuantity([FromBody] FillingWarehouse filling)
        {
            try
            {
                var updatedFilling = await _fillingWarehouseService.UpdateQuantityAsync(
                    filling.WarehouseId, 
                    filling.MaterialId, 
                    filling.Quantity);
                return Ok(new { message = "Quantity updated successfully", filling = updatedFilling });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET: api/filling-warehouses/warehouse/{warehouseId}/stock
        [HttpGet("warehouse/{warehouseId}/stock")]
        public async Task<ActionResult<IEnumerable<FillingWarehouse>>> GetStock(int warehouseId)
        {
            var stock = await _fillingWarehouseService.GetWarehouseStockAsync(warehouseId);
            return Ok(stock);
        }
    }
}

