using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("api/reprocessings")]
    public class ReprocessingsController : ControllerBase
    {
        private readonly IReprocessingService _reprocessingService;
        private readonly ILogger<ReprocessingsController> _logger;
        private readonly IHistoryService _historyService;

        public ReprocessingsController(IReprocessingService reprocessingService, ILogger<ReprocessingsController> logger, IHistoryService historyService)
        {
            _reprocessingService = reprocessingService;
            _logger = logger;
            _historyService = historyService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReprocessingCreateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid model state", errors = ModelState });
            }

            if (!Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) ||
                !int.TryParse(userIdHeader.ToString(), out var userId) || userId <= 0)
            {
                return BadRequest(new { message = "X-User-Id header is required" });
            }

            try
            {
                var reprocessing = await _reprocessingService.CreateReprocessingAsync(request, userId);
                await TryLogAsync(userId, new HistoryEvent
                {
                    Action = "Reprocessing.Created",
                    EntityType = "Reprocessing",
                    EntityId = reprocessing.Id,
                    WarehouseId = request.WarehouseId,
                    MaterialId = request.SourceMaterialId,
                    Description = $"Переработка материала ID {request.SourceMaterialId} (кол-во {request.SourceQuantity})"
                });
                return Ok(new { message = "Reprocessing created successfully", reprocessing });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Key not found while creating reprocessing");
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error while creating reprocessing");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating reprocessing");
                return StatusCode(500, new { message = "An unexpected error occurred while creating reprocessing" });
            }
        }

        private async Task TryLogAsync(int userId, HistoryEvent historyEvent)
        {
            if (userId <= 0)
            {
                return;
            }

            historyEvent.UserId = userId;
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
