using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccessibleMovementsController : ControllerBase
    {
        private readonly IAccessibleMovementService _movementService;

        public AccessibleMovementsController(IAccessibleMovementService movementService)
        {
            _movementService = movementService;
        }

        // GET: api/accessible-movements
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccessibleMovement>>> GetAll()
        {
            var movements = await _movementService.GetAllMovementsAsync();
            return Ok(movements);
        }

        // GET: api/accessible-movements/{fromWarehouseId}/{toWarehouseId}/{materialId}
        [HttpGet("{fromWarehouseId}/{toWarehouseId}/{materialId}")]
        public async Task<ActionResult<AccessibleMovement>> GetMovement(int fromWarehouseId, int toWarehouseId, int materialId)
        {
            var movement = await _movementService.GetMovementAsync(fromWarehouseId, toWarehouseId, materialId);
            if (movement == null)
                return NotFound($"Movement not found");

            return Ok(movement);
        }

        // GET: api/accessible-movements/from/{warehouseId}
        [HttpGet("from/{warehouseId}")]
        public async Task<ActionResult<IEnumerable<AccessibleMovement>>> GetFromWarehouse(int warehouseId)
        {
            var movements = await _movementService.GetMovementsFromWarehouseAsync(warehouseId);
            return Ok(movements);
        }

        // GET: api/accessible-movements/to/{warehouseId}
        [HttpGet("to/{warehouseId}")]
        public async Task<ActionResult<IEnumerable<AccessibleMovement>>> GetToWarehouse(int warehouseId)
        {
            var movements = await _movementService.GetMovementsToWarehouseAsync(warehouseId);
            return Ok(movements);
        }

        // GET: api/accessible-movements/material/{materialId}
        [HttpGet("material/{materialId}")]
        public async Task<ActionResult<IEnumerable<AccessibleMovement>>> GetByMaterial(int materialId)
        {
            var movements = await _movementService.GetMovementsByMaterialAsync(materialId);
            return Ok(movements);
        }

        // POST: api/accessible-movements/add
        [HttpPost("add")]
        public async Task<IActionResult> AddMovement([FromBody] AccessibleMovement movement)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdMovement = await _movementService.CreateMovementAsync(movement);
                return Ok(new { message = "Movement created successfully", movement = createdMovement });
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

        // POST: api/accessible-movements/edit/{fromWarehouseId}/{toWarehouseId}/{materialId}
        [HttpPost("edit/{fromWarehouseId}/{toWarehouseId}/{materialId}")]
        public async Task<IActionResult> EditMovement(int fromWarehouseId, int toWarehouseId, int materialId, [FromBody] AccessibleMovement updated)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var movement = await _movementService.UpdateMovementAsync(fromWarehouseId, toWarehouseId, materialId, updated);
                return Ok(new { message = "Movement updated successfully", movement });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/accessible-movements/delete/{fromWarehouseId}/{toWarehouseId}/{materialId}
        [HttpPost("delete/{fromWarehouseId}/{toWarehouseId}/{materialId}")]
        public async Task<IActionResult> DeleteMovement(int fromWarehouseId, int toWarehouseId, int materialId)
        {
            var deleted = await _movementService.DeleteMovementAsync(fromWarehouseId, toWarehouseId, materialId);
            if (!deleted)
                return NotFound($"Movement not found");

            return Ok(new { message = "Movement deleted successfully" });
        }

        // GET: api/accessible-movements/check?fromWarehouseId=&toWarehouseId=&materialId=
        [HttpGet("check")]
        public async Task<ActionResult<bool>> CheckAllowed([FromQuery] int fromWarehouseId, [FromQuery] int toWarehouseId, [FromQuery] int materialId)
        {
            var isAllowed = await _movementService.IsMovementAllowedAsync(fromWarehouseId, toWarehouseId, materialId);
            return Ok(new { isAllowed });
        }
    }
}

