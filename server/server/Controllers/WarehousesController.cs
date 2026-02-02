using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Services;
using server.Attributes;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehousesController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;
        private readonly ILogger<WarehousesController> _logger;
        private readonly IHistoryService _historyService;

        public WarehousesController(IWarehouseService warehouseService, ILogger<WarehousesController> logger, IHistoryService historyService)
        {
            _warehouseService = warehouseService;
            _logger = logger;
            _historyService = historyService;
        }

        // GET: api/warehouses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Warehouse>>> GetAll()
        {
            try
            {
                var warehouses = await _warehouseService.GetAllWarehousesAsync();
                return Ok(warehouses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all warehouses");
                return StatusCode(500, new { message = "An error occurred while retrieving warehouses" });
            }
        }

        // GET: api/warehouses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Warehouse>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                var warehouse = await _warehouseService.GetWarehouseByIdAsync(id);
                if (warehouse == null)
                {
                    _logger.LogWarning("Warehouse with ID {WarehouseId} not found", id);
                    return NotFound(new { message = $"Warehouse with ID {id} not found" });
                }

                return Ok(warehouse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting warehouse with ID {WarehouseId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the warehouse" });
            }
        }

        // GET: api/warehouses/type/{type}
        [HttpGet("type/{type}")]
        public async Task<ActionResult<IEnumerable<Warehouse>>> GetByType(string type)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(type))
                {
                    return BadRequest(new { message = "Type cannot be empty" });
                }

                var warehouses = await _warehouseService.GetWarehousesByTypeAsync(type);
                return Ok(warehouses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting warehouses by type '{Type}'", type);
                return StatusCode(500, new { message = "An error occurred while retrieving warehouses" });
            }
        }

        // GET: api/warehouses/search?name=&type=&isActive=&sortBy=
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Warehouse>>> Search(
            [FromQuery] string? name,
            [FromQuery] string? type,
            [FromQuery] bool? isActive,
            [FromQuery] string? sortBy)
        {
            try
            {
                var warehouses = await _warehouseService.SearchWarehousesAsync(name, type, isActive, sortBy);
                return Ok(warehouses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching warehouses");
                return StatusCode(500, new { message = "An error occurred while searching warehouses" });
            }
        }

        // POST: api/warehouses
        [HttpPost]
        [RequirePermission("ManageWarehouses")]
        public async Task<IActionResult> CreateWarehouse([FromBody] Warehouse warehouse)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid model state", errors = ModelState });
            }

            try
            {
                var createdWarehouse = await _warehouseService.CreateWarehouseAsync(warehouse);
                _logger.LogInformation("Warehouse created successfully with ID: {WarehouseId}", createdWarehouse.Id);
                await TryLogAsync(GetUserIdFromHeader(), new HistoryEvent
                {
                    Action = "Warehouse.Created",
                    EntityType = "Warehouse",
                    EntityId = createdWarehouse.Id,
                    WarehouseId = createdWarehouse.Id,
                    Description = $"Создан склад: {createdWarehouse.Name}"
                });
                return CreatedAtAction(nameof(GetById), new { id = createdWarehouse.Id },
                    new { message = "Warehouse created successfully", warehouse = createdWarehouse });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error while creating warehouse");
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while creating warehouse");
                return StatusCode(500, new { message = "An error occurred while saving the warehouse to the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating warehouse");
                return StatusCode(500, new { message = "An unexpected error occurred while creating the warehouse" });
            }
        }

        // PUT: api/warehouses/5
        [HttpPut("{id}")]
        [RequirePermission("ManageWarehouses")]
        public async Task<IActionResult> UpdateWarehouse(int id, [FromBody] Warehouse updated)
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
                var warehouse = await _warehouseService.UpdateWarehouseAsync(id, updated);
                _logger.LogInformation("Warehouse updated successfully with ID: {WarehouseId}", id);
                await TryLogAsync(GetUserIdFromHeader(), new HistoryEvent
                {
                    Action = "Warehouse.Updated",
                    EntityType = "Warehouse",
                    EntityId = warehouse.Id,
                    WarehouseId = warehouse.Id,
                    Description = $"Обновлен склад: {warehouse.Name}"
                });
                return Ok(new { message = "Warehouse updated successfully", warehouse });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Warehouse with ID {WarehouseId} not found for update", id);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error while updating warehouse with ID {WarehouseId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating warehouse with ID {WarehouseId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the warehouse in the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating warehouse with ID {WarehouseId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred while updating the warehouse" });
            }
        }

        // DELETE: api/warehouses/5
        [HttpDelete("{id}")]
        [RequirePermission("ManageWarehouses")]
        public async Task<IActionResult> DeleteWarehouse(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                var deleted = await _warehouseService.DeleteWarehouseAsync(id);
                if (!deleted)
                {
                    _logger.LogWarning("Warehouse with ID {WarehouseId} not found for deletion", id);
                    return NotFound(new { message = $"Warehouse with ID {id} not found" });
                }

                _logger.LogInformation("Warehouse deleted successfully with ID: {WarehouseId}", id);
                await TryLogAsync(GetUserIdFromHeader(), new HistoryEvent
                {
                    Action = "Warehouse.Deleted",
                    EntityType = "Warehouse",
                    EntityId = id,
                    WarehouseId = id,
                    Description = $"Удален склад ID {id}"
                });
                return Ok(new { message = "Warehouse deleted successfully" });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while deleting warehouse with ID {WarehouseId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the warehouse from the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting warehouse with ID {WarehouseId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred while deleting the warehouse" });
            }
        }

        // GET: api/warehouses/{id}/movements
        [HttpGet("{id}/movements")]
        public async Task<ActionResult<IEnumerable<AccessibleMovement>>> GetMovements(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                var movements = await _warehouseService.GetWarehouseMovementsAsync(id);
                return Ok(movements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting movements for warehouse {WarehouseId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving movements" });
            }
        }

        // GET: api/warehouses/{id}/requests
        [HttpGet("{id}/requests")]
        public async Task<ActionResult<IEnumerable<PartRequest>>> GetRequests(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                var requests = await _warehouseService.GetWarehouseRequestsAsync(id);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting requests for warehouse {WarehouseId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving requests" });
            }
        }

        // POST: api/warehouses/{id}/stop
        [HttpPost("{id}/stop")]
        [RequirePermission("ManageWarehouses")]
        public async Task<IActionResult> StopWarehouse(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                var warehouse = await _warehouseService.StopWarehouseAsync(id);
                _logger.LogInformation("Warehouse stopped successfully with ID: {WarehouseId}", id);
                await TryLogAsync(GetUserIdFromHeader(), new HistoryEvent
                {
                    Action = "Warehouse.Stopped",
                    EntityType = "Warehouse",
                    EntityId = warehouse.Id,
                    WarehouseId = warehouse.Id,
                    Description = $"Остановлен склад: {warehouse.Name}"
                });
                return Ok(new { message = "Warehouse stopped successfully", warehouse });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Warehouse with ID {WarehouseId} not found for stopping", id);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while stopping warehouse with ID {WarehouseId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while stopping warehouse with ID {WarehouseId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred while stopping the warehouse" });
            }
        }

        // POST: api/warehouses/{id}/start
        [HttpPost("{id}/start")]
        [RequirePermission("ManageWarehouses")]
        public async Task<IActionResult> StartWarehouse(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                var warehouse = await _warehouseService.StartWarehouseAsync(id);
                _logger.LogInformation("Warehouse started successfully with ID: {WarehouseId}", id);
                await TryLogAsync(GetUserIdFromHeader(), new HistoryEvent
                {
                    Action = "Warehouse.Started",
                    EntityType = "Warehouse",
                    EntityId = warehouse.Id,
                    WarehouseId = warehouse.Id,
                    Description = $"Запущен склад: {warehouse.Name}"
                });
                return Ok(new { message = "Warehouse started successfully", warehouse });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Warehouse with ID {WarehouseId} not found for starting", id);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while starting warehouse with ID {WarehouseId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while starting warehouse with ID {WarehouseId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred while starting the warehouse" });
            }
        }

        private int? GetUserIdFromHeader()
        {
            if (Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) &&
                int.TryParse(userIdHeader.ToString(), out var userId))
            {
                return userId;
            }
            return null;
        }

        private async Task TryLogAsync(int? userId, HistoryEvent historyEvent)
        {
            if (!userId.HasValue || userId.Value <= 0)
            {
                return;
            }

            historyEvent.UserId = userId.Value;
            try
            {
                await _historyService.AddEventAsync(historyEvent);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to write history event");
            }
        }
    }
}
