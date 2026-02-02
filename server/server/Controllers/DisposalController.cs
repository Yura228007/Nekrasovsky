using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("api/disposal")]
    public class DisposalController : ControllerBase
    {
        private readonly IDisposalService _disposalService;
        private readonly ILogger<DisposalController> _logger;
        private readonly IHistoryService _historyService;

        public DisposalController(IDisposalService disposalService, ILogger<DisposalController> logger, IHistoryService historyService)
        {
            _disposalService = disposalService;
            _logger = logger;
            _historyService = historyService;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessDisposal([FromBody] DisposalProcessRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Request body is required" });
            }

            if (!Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) ||
                !int.TryParse(userIdHeader.ToString(), out var userId) || userId <= 0)
            {
                return BadRequest(new { message = "X-User-Id header is required" });
            }

            try
            {
                await _disposalService.ProcessDisposalAsync(
                    request.DisposalWarehouseId,
                    request.MaterialId,
                    request.ProductId,
                    request.ReturnableQuantity,
                    request.NonReturnableQuantity);

                var itemType = request.MaterialId.HasValue ? "Материал" : "Продукт";
                var itemId = request.MaterialId ?? request.ProductId ?? 0;
                await TryLogAsync(userId, new HistoryEvent
                {
                    Action = "Disposal.Processed",
                    EntityType = "Disposal",
                    EntityId = itemId,
                    WarehouseId = request.DisposalWarehouseId,
                    MaterialId = request.MaterialId,
                    ProductId = request.ProductId,
                    Description = $"Утиль: {itemType} ID {itemId}, возвратный {request.ReturnableQuantity}, невозвратный {request.NonReturnableQuantity}"
                });

                return Ok(new { message = "Disposal processed successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Key not found while processing disposal");
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error while processing disposal");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while processing disposal");
                return StatusCode(500, new { message = "An unexpected error occurred while processing disposal" });
            }
        }

        private async Task TryLogAsync(int userId, HistoryEvent historyEvent)
        {
            if (userId <= 0) return;
            historyEvent.UserId = userId;
            try
            {
                await _historyService.AddEventAsync(historyEvent);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to log disposal history event");
            }
        }
    }
}
