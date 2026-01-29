using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Services;

namespace server.Controllers
{
    [Route("api/machines")]
    [ApiController]
    public class MachinesController : ControllerBase
    {
        private readonly IMachineService _machineService;
        private readonly IHistoryService _historyService;
        private readonly ILogger<MachinesController> _logger;

        public MachinesController(
            IMachineService machineService,
            IHistoryService historyService,
            ILogger<MachinesController> logger)
        {
            _machineService = machineService;
            _historyService = historyService;
            _logger = logger;
        }

        // GET: api/machines
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var machines = await _machineService.GetAllAsync();
            return Ok(machines);
        }

        // GET: api/machines/active
        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            var machines = await _machineService.GetActiveAsync();
            return Ok(machines);
        }

        // GET: api/machines/warehouse/{warehouseId}
        [HttpGet("warehouse/{warehouseId}")]
        public async Task<IActionResult> GetByWarehouse(int warehouseId)
        {
            if (warehouseId <= 0)
            {
                return BadRequest(new { message = "Invalid warehouse ID" });
            }

            var machines = await _machineService.GetByWarehouseAsync(warehouseId);
            return Ok(machines);
        }

        // GET: api/machines/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid ID" });
            }

            var machine = await _machineService.GetByIdAsync(id);
            if (machine == null)
            {
                return NotFound(new { message = $"Machine with ID {id} not found" });
            }

            return Ok(machine);
        }

        // POST: api/machines
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Machine machine)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid model state", errors = ModelState });
            }

            try
            {
                var created = await _machineService.CreateAsync(machine);
                _logger.LogInformation("Machine created with ID: {Id}", created.Id);

                await TryLogAsync(GetUserIdFromHeader(), new HistoryEvent
                {
                    UserId = GetUserIdFromHeader() ?? 0,
                    Action = "Machine.Created",
                    EntityType = "Machine",
                    EntityId = created.Id,
                    WarehouseId = created.WarehouseId,
                    Description = $"Создан станок: {created.Name} ({created.Code ?? "без кода"})"
                });

                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while creating machine");
                return StatusCode(500, new { message = "An error occurred while saving" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating machine");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        // PUT: api/machines/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Machine updated)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid ID" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid model state", errors = ModelState });
            }

            try
            {
                var result = await _machineService.UpdateAsync(id, updated);
                _logger.LogInformation("Machine updated with ID: {Id}", id);

                await TryLogAsync(GetUserIdFromHeader(), new HistoryEvent
                {
                    UserId = GetUserIdFromHeader() ?? 0,
                    Action = "Machine.Updated",
                    EntityType = "Machine",
                    EntityId = result.Id,
                    WarehouseId = result.WarehouseId,
                    Description = $"Обновлен станок: {result.Name} ({result.Code ?? "без кода"})"
                });

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating machine");
                return StatusCode(500, new { message = "An error occurred while updating" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating machine");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        // DELETE: api/machines/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid ID" });
            }

            try
            {
                var machine = await _machineService.GetByIdAsync(id);
                await _machineService.DeleteAsync(id);
                _logger.LogInformation("Machine deleted with ID: {Id}", id);

                await TryLogAsync(GetUserIdFromHeader(), new HistoryEvent
                {
                    UserId = GetUserIdFromHeader() ?? 0,
                    Action = "Machine.Deleted",
                    EntityType = "Machine",
                    EntityId = id,
                    Description = $"Удален станок: {machine?.Name ?? "ID " + id}"
                });

                return Ok(new { message = "Machine deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting machine");
                return StatusCode(500, new { message = "An error occurred while deleting" });
            }
        }

        private int? GetUserIdFromHeader()
        {
            if (Request.Headers.TryGetValue("X-User-Id", out var userIdValue) &&
                int.TryParse(userIdValue.FirstOrDefault(), out var userId))
            {
                return userId;
            }
            return null;
        }

        private async Task TryLogAsync(int? userId, HistoryEvent evt)
        {
            try
            {
                if (userId.HasValue)
                {
                    evt.UserId = userId.Value;
                    await _historyService.AddEventAsync(evt);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to log history event");
            }
        }
    }
}
