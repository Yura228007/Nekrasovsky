using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehousesController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;

        public WarehousesController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        // GET: api/warehouses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Warehouse>>> GetAll()
        {
            var warehouses = await _warehouseService.GetAllWarehousesAsync();
            return Ok(warehouses);
        }

        // GET: api/warehouses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Warehouse>> GetById(int id)
        {
            var warehouse = await _warehouseService.GetWarehouseByIdAsync(id);
            if (warehouse == null)
                return NotFound($"Warehouse with ID {id} not found");

            return Ok(warehouse);
        }

        // GET: api/warehouses/type/{type}
        [HttpGet("type/{type}")]
        public async Task<ActionResult<IEnumerable<Warehouse>>> GetByType(string type)
        {
            var warehouses = await _warehouseService.GetWarehousesByTypeAsync(type);
            return Ok(warehouses);
        }

        // GET: api/warehouses/search?name=&type=
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Warehouse>>> Search([FromQuery] string? name, [FromQuery] string? type)
        {
            var warehouses = await _warehouseService.SearchWarehousesAsync(name, type);
            return Ok(warehouses);
        }

        // POST: api/warehouses/add
        [HttpPost("add")]
        public async Task<IActionResult> AddWarehouse([FromBody] Warehouse warehouse)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdWarehouse = await _warehouseService.CreateWarehouseAsync(warehouse);
            return Ok(new { message = "Warehouse created successfully", warehouse = createdWarehouse });
        }

        // POST: api/warehouses/edit/{id}
        [HttpPost("edit/{id}")]
        public async Task<IActionResult> EditWarehouse(int id, [FromBody] Warehouse updated)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var warehouse = await _warehouseService.UpdateWarehouseAsync(id, updated);
                return Ok(new { message = "Warehouse updated successfully", warehouse });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/warehouses/delete/{id}
        [HttpPost("delete/{id}")]
        public async Task<IActionResult> DeleteWarehouse(int id)
        {
            var deleted = await _warehouseService.DeleteWarehouseAsync(id);
            if (!deleted)
                return NotFound($"Warehouse with ID {id} not found");

            return Ok(new { message = "Warehouse deleted successfully" });
        }

        // GET: api/warehouses/{id}/movements
        [HttpGet("{id}/movements")]
        public async Task<ActionResult<IEnumerable<AccessibleMovement>>> GetMovements(int id)
        {
            var movements = await _warehouseService.GetWarehouseMovementsAsync(id);
            return Ok(movements);
        }

        // GET: api/warehouses/{id}/requests
        [HttpGet("{id}/requests")]
        public async Task<ActionResult<IEnumerable<PartRequest>>> GetRequests(int id)
        {
            var requests = await _warehouseService.GetWarehouseRequestsAsync(id);
            return Ok(requests);
        }
    }
}

