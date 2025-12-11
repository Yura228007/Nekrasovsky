using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccessibleMovementsController : ControllerBase
    {
        private readonly IAccessibleMovementService _movementService;
        private readonly ILogger<AccessibleMovementsController> _logger;

        public AccessibleMovementsController(IAccessibleMovementService movementService, ILogger<AccessibleMovementsController> logger)
        {
            _movementService = movementService;
            _logger = logger;
        }

        // GET: api/accessible-movements
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccessibleMovement>>> GetAll()
        {
            try
            {
                var movements = await _movementService.GetAllMovementsAsync();
                return Ok(movements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all movements");
                return StatusCode(500, new { message = "An error occurred while retrieving movements" });
            }
        }

        // GET: api/accessible-movements/{fromWarehouseId}/{toWarehouseId}/{materialId}
        [HttpGet("{fromWarehouseId}/{toWarehouseId}/{materialId}")]
        public async Task<ActionResult<AccessibleMovement>> GetMovement(int fromWarehouseId, int toWarehouseId, int materialId)
        {
            try
            {
                if (fromWarehouseId <= 0 || toWarehouseId <= 0 || materialId <= 0)
                {
                    return BadRequest(new { message = "All IDs must be greater than 0" });
                }

                var movement = await _movementService.GetMovementAsync(fromWarehouseId, toWarehouseId, materialId);
                if (movement == null)
                {
                    _logger.LogWarning("Movement not found for FromWarehouseId {FromId}, ToWarehouseId {ToId}, MaterialId {MaterialId}", fromWarehouseId, toWarehouseId, materialId);
                    return NotFound(new { message = "Movement not found" });
                }

                return Ok(movement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting movement");
                return StatusCode(500, new { message = "An error occurred while retrieving the movement" });
            }
        }

        // GET: api/accessible-movements/from/{warehouseId}
        [HttpGet("from/{warehouseId}")]
        public async Task<ActionResult<IEnumerable<AccessibleMovement>>> GetFromWarehouse(int warehouseId)
        {
            try
            {
                if (warehouseId <= 0)
                {
                    return BadRequest(new { message = "WarehouseId must be greater than 0" });
                }

                var movements = await _movementService.GetMovementsFromWarehouseAsync(warehouseId);
                return Ok(movements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting movements from warehouse {WarehouseId}", warehouseId);
                return StatusCode(500, new { message = "An error occurred while retrieving movements" });
            }
        }

        // GET: api/accessible-movements/to/{warehouseId}
        [HttpGet("to/{warehouseId}")]
        public async Task<ActionResult<IEnumerable<AccessibleMovement>>> GetToWarehouse(int warehouseId)
        {
            try
            {
                if (warehouseId <= 0)
                {
                    return BadRequest(new { message = "WarehouseId must be greater than 0" });
                }

                var movements = await _movementService.GetMovementsToWarehouseAsync(warehouseId);
                return Ok(movements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting movements to warehouse {WarehouseId}", warehouseId);
                return StatusCode(500, new { message = "An error occurred while retrieving movements" });
            }
        }

        // GET: api/accessible-movements/material/{materialId}
        [HttpGet("material/{materialId}")]
        public async Task<ActionResult<IEnumerable<AccessibleMovement>>> GetByMaterial(int materialId)
        {
            try
            {
                if (materialId <= 0)
                {
                    return BadRequest(new { message = "MaterialId must be greater than 0" });
                }

                var movements = await _movementService.GetMovementsByMaterialAsync(materialId);
                return Ok(movements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting movements for material {MaterialId}", materialId);
                return StatusCode(500, new { message = "An error occurred while retrieving movements" });
            }
        }

        // POST: api/accessible-movements
        [HttpPost]
        public async Task<IActionResult> CreateMovement([FromBody] AccessibleMovement movement)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid model state", errors = ModelState });
            }

            try
            {
                if (movement.FromWarehouseId <= 0 || movement.ToWarehouseId <= 0 || movement.MaterialId <= 0)
                {
                    return BadRequest(new { message = "All IDs must be greater than 0" });
                }

                var createdMovement = await _movementService.CreateMovementAsync(movement);
                _logger.LogInformation("Movement created successfully for FromWarehouseId {FromId}, ToWarehouseId {ToId}, MaterialId {MaterialId}", movement.FromWarehouseId, movement.ToWarehouseId, movement.MaterialId);
                return Ok(new { message = "Movement created successfully", movement = createdMovement });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Key not found while creating movement");
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while creating movement");
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while creating movement");
                return StatusCode(500, new { message = "An error occurred while saving the movement to the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating movement");
                return StatusCode(500, new { message = "An unexpected error occurred while creating the movement" });
            }
        }

        // POST: api/accessible-movements/edit/{fromWarehouseId}/{toWarehouseId}/{materialId} - POST because of composite key
        [HttpPost("edit/{fromWarehouseId}/{toWarehouseId}/{materialId}")]
        public async Task<IActionResult> UpdateMovement(int fromWarehouseId, int toWarehouseId, int materialId, [FromBody] AccessibleMovement updated)
        {
            if (fromWarehouseId <= 0 || toWarehouseId <= 0 || materialId <= 0)
            {
                return BadRequest(new { message = "All IDs must be greater than 0" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid model state", errors = ModelState });
            }

            try
            {
                var movement = await _movementService.UpdateMovementAsync(fromWarehouseId, toWarehouseId, materialId, updated);
                _logger.LogInformation("Movement updated successfully for FromWarehouseId {FromId}, ToWarehouseId {ToId}, MaterialId {MaterialId}", fromWarehouseId, toWarehouseId, materialId);
                return Ok(new { message = "Movement updated successfully", movement });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Movement not found for update");
                return NotFound(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating movement");
                return StatusCode(500, new { message = "An error occurred while updating the movement in the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating movement");
                return StatusCode(500, new { message = "An unexpected error occurred while updating the movement" });
            }
        }

        // POST: api/accessible-movements/delete/{fromWarehouseId}/{toWarehouseId}/{materialId} - POST because of composite key
        [HttpPost("delete/{fromWarehouseId}/{toWarehouseId}/{materialId}")]
        public async Task<IActionResult> DeleteMovement(int fromWarehouseId, int toWarehouseId, int materialId)
        {
            try
            {
                if (fromWarehouseId <= 0 || toWarehouseId <= 0 || materialId <= 0)
                {
                    return BadRequest(new { message = "All IDs must be greater than 0" });
                }

                var deleted = await _movementService.DeleteMovementAsync(fromWarehouseId, toWarehouseId, materialId);
                if (!deleted)
                {
                    _logger.LogWarning("Movement not found for deletion with FromWarehouseId {FromId}, ToWarehouseId {ToId}, MaterialId {MaterialId}", fromWarehouseId, toWarehouseId, materialId);
                    return NotFound(new { message = "Movement not found" });
                }

                _logger.LogInformation("Movement deleted successfully for FromWarehouseId {FromId}, ToWarehouseId {ToId}, MaterialId {MaterialId}", fromWarehouseId, toWarehouseId, materialId);
                return Ok(new { message = "Movement deleted successfully" });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while deleting movement");
                return StatusCode(500, new { message = "An error occurred while deleting the movement from the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting movement");
                return StatusCode(500, new { message = "An unexpected error occurred while deleting the movement" });
            }
        }

        // GET: api/accessible-movements/check?fromWarehouseId=&toWarehouseId=&materialId=
        [HttpGet("check")]
        public async Task<ActionResult<bool>> CheckAllowed([FromQuery] int fromWarehouseId, [FromQuery] int toWarehouseId, [FromQuery] int materialId)
        {
            try
            {
                if (fromWarehouseId <= 0 || toWarehouseId <= 0 || materialId <= 0)
                {
                    return BadRequest(new { message = "All IDs must be greater than 0" });
                }

                var isAllowed = await _movementService.IsMovementAllowedAsync(fromWarehouseId, toWarehouseId, materialId);
                return Ok(new { isAllowed });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking movement allowance");
                return StatusCode(500, new { message = "An error occurred while checking movement allowance" });
            }
        }
    }
}
