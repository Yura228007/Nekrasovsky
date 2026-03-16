using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.Services;
using System.Linq;

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
            _logger.LogInformation("Reprocessing request received: WarehouseId={WarehouseId}, Sources={SourcesCount}, Outputs={OutputsCount}", 
                request?.WarehouseId, request?.Sources?.Count, request?.Outputs?.Count);
            
            if (request?.Outputs != null)
            {
                for (int i = 0; i < request.Outputs.Count; i++)
                {
                    var output = request.Outputs[i];
                    _logger.LogInformation("Output[{Index}]: MaterialId={MaterialId}, ProductId={ProductId}, Quantity={Quantity}, NewMaterialCode={NewMaterialCode}", 
                        i, output.MaterialId, output.ProductId, output.Quantity, output.NewMaterialCode);
                }
            }
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid ModelState: {Errors}", string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(new { message = "Invalid model state", errors = ModelState });
            }

            if (request == null)
            {
                return BadRequest(new { message = "Request body is required" });
            }

            if (!Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) ||
                !int.TryParse(userIdHeader.ToString(), out var userId) || userId <= 0)
            {
                _logger.LogWarning("X-User-Id header missing or invalid");
                return BadRequest(new { message = "X-User-Id header is required" });
            }

            try
            {
                var reprocessing = await _reprocessingService.CreateReprocessingAsync(request, userId);
                var sourceDescription = request.Sources.Count == 0
                    ? "не указаны"
                    : string.Join(", ", request.Sources.Select(s => $"ID {s.MaterialId} (кол-во {s.Quantity})"));

                await TryLogAsync(userId, new HistoryEvent
                {
                    Action = "Reprocessing.Created",
                    EntityType = "Reprocessing",
                    EntityId = reprocessing.Id,
                    WarehouseId = request.WarehouseId,
                    MaterialId = request.Sources.FirstOrDefault()?.MaterialId,
                    Description = $"Переработка материалов: {sourceDescription}"
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
