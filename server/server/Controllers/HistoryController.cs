using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("api/history")]
    public class HistoryController : ControllerBase
    {
        private readonly IHistoryService _historyService;
        private readonly ILogger<HistoryController> _logger;

        public HistoryController(IHistoryService historyService, ILogger<HistoryController> logger)
        {
            _historyService = historyService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<HistoryEvent>>> GetHistory(
            [FromQuery] int? userId,
            [FromQuery] int? relatedUserId,
            [FromQuery] string? action,
            [FromQuery] string? entityType,
            [FromQuery] int? warehouseId,
            [FromQuery] int? materialId,
            [FromQuery] int? productId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var history = await _historyService.GetHistoryAsync(
                    userId, relatedUserId, action, entityType,
                    warehouseId, materialId, productId, startDate, endDate);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting history");
                return StatusCode(500, new { message = "An error occurred while retrieving history" });
            }
        }
    }
}
