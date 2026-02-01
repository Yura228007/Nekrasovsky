using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaterialsController : ControllerBase
    {
        private readonly IMaterialService _materialService;
        private readonly ILogger<MaterialsController> _logger;
        private readonly IHistoryService _historyService;

        public MaterialsController(IMaterialService materialService, ILogger<MaterialsController> logger, IHistoryService historyService)
        {
            _materialService = materialService;
            _logger = logger;
            _historyService = historyService;
        }

        // GET: api/materials
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Material>>> GetAll()
        {
            try
            {
                var materials = await _materialService.GetAllMaterialsAsync();
                return Ok(materials);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all materials");
                return StatusCode(500, new { message = "An error occurred while retrieving materials" });
            }
        }

        // GET: api/materials/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Material>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                var material = await _materialService.GetMaterialByIdAsync(id);
                if (material == null)
                {
                    _logger.LogWarning("Material with ID {MaterialId} not found", id);
                    return NotFound(new { message = $"Material with ID {id} not found" });
                }

                return Ok(material);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting material with ID {MaterialId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the material" });
            }
        }

        // GET: api/materials/search?name=&code=&isActive=&sortBy=
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Material>>> Search(
            [FromQuery] string? name,
            [FromQuery] string? code,
            [FromQuery] bool? isActive,
            [FromQuery] string? sortBy)
        {
            try
            {
                var materials = await _materialService.SearchMaterialsAsync(name, code, isActive, sortBy);
                return Ok(materials);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching materials");
                return StatusCode(500, new { message = "An error occurred while searching materials" });
            }
        }

        // GET: api/materials/unit/{unit}
        [HttpGet("unit/{unit}")]
        public async Task<ActionResult<IEnumerable<Material>>> GetByMeasuringUnit(string unit)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(unit))
                {
                    return BadRequest(new { message = "Unit cannot be empty" });
                }

                var materials = await _materialService.GetMaterialsByMeasuringUnitAsync(unit);
                return Ok(materials);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting materials by unit '{Unit}'", unit);
                return StatusCode(500, new { message = "An error occurred while retrieving materials" });
            }
        }

        // POST: api/materials?quantity=0&measuringUnit=шт
        [HttpPost]
        public async Task<IActionResult> CreateMaterial([FromBody] Material material, [FromQuery] int? quantity = null, [FromQuery] string? measuringUnit = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid model state", errors = ModelState });
            }

            try
            {
                if (!Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) ||
                    !int.TryParse(userIdHeader.ToString(), out var userId) || userId <= 0)
                {
                    return BadRequest(new { message = "X-User-Id header is required" });
                }

                var createdMaterial = await _materialService.CreateMaterialAsync(material, userId, quantity, measuringUnit);
                _logger.LogInformation("Material created successfully with ID: {MaterialId}", createdMaterial.Id);
                await TryLogAsync(userId, new HistoryEvent
                {
                    UserId = userId,
                    Action = "Material.Created",
                    EntityType = "Material",
                    EntityId = createdMaterial.Id,
                    MaterialId = createdMaterial.Id,
                    Description = $"Создан материал: {createdMaterial.Name}"
                });
                return CreatedAtAction(nameof(GetById), new { id = createdMaterial.Id },
                    new { message = "Material created successfully", material = createdMaterial });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error while creating material");
                return Conflict(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while creating material");
                return StatusCode(500, new { message = "An error occurred while saving the material to the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating material");
                return StatusCode(500, new { message = "An unexpected error occurred while creating the material" });
            }
        }

        // PUT: api/materials/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMaterial(int id, [FromBody] Material updated)
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
                var material = await _materialService.UpdateMaterialAsync(id, updated);
                _logger.LogInformation("Material updated successfully with ID: {MaterialId}", id);
                await TryLogAsync(GetUserIdFromHeader(), new HistoryEvent
                {
                    UserId = GetUserIdFromHeader() ?? 0,
                    Action = "Material.Updated",
                    EntityType = "Material",
                    EntityId = material.Id,
                    MaterialId = material.Id,
                    Description = $"Обновлен материал: {material.Name}"
                });
                return Ok(new { message = "Material updated successfully", material });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Material with ID {MaterialId} not found for update", id);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error while updating material with ID {MaterialId}", id);
                return Conflict(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating material with ID {MaterialId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the material in the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating material with ID {MaterialId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred while updating the material" });
            }
        }

        // DELETE: api/materials/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMaterial(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                var deleted = await _materialService.DeleteMaterialAsync(id);
                if (!deleted)
                {
                    _logger.LogWarning("Material with ID {MaterialId} not found for deletion", id);
                    return NotFound(new { message = $"Material with ID {id} not found" });
                }

                _logger.LogInformation("Material deleted successfully with ID: {MaterialId}", id);
                await TryLogAsync(GetUserIdFromHeader(), new HistoryEvent
                {
                    UserId = GetUserIdFromHeader() ?? 0,
                    Action = "Material.Deleted",
                    EntityType = "Material",
                    EntityId = id,
                    MaterialId = id,
                    Description = $"Удален материал ID: {id}"
                });
                return Ok(new { message = "Material deleted successfully" });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while deleting material with ID {MaterialId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the material from the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting material with ID {MaterialId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred while deleting the material" });
            }
        }

        // GET: api/materials/{id}/movements
        [HttpGet("{id}/movements")]
        public async Task<ActionResult<IEnumerable<AccessibleMovement>>> GetMovements(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                var movements = await _materialService.GetMaterialMovementsAsync(id);
                return Ok(movements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting movements for material {MaterialId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving movements" });
            }
        }

        // GET: api/materials/{id}/usage
        [HttpGet("{id}/usage")]
        public async Task<ActionResult<object>> GetUsage(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                var usage = await _materialService.GetMaterialUsageAsync(id);
                return Ok(usage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting usage for material {MaterialId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving usage" });
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
