using Microsoft.AspNetCore.Mvc;
using server.Services;

namespace server.Controllers
{
    [Route("api/reports")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportGeneratorService _reportService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IReportGeneratorService reportService, ILogger<ReportsController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        /// <summary>
        /// Генерирует отчёт по заявкам на перемещение
        /// </summary>
        /// <param name="startDate">Начальная дата</param>
        /// <param name="endDate">Конечная дата</param>
        /// <param name="userId">ID пользователя (опционально)</param>
        [HttpGet("part-requests")]
        public async Task<IActionResult> GetPartRequestReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int? userId = null)
        {
            try
            {
                // Convert to UTC if needed
                if (startDate.Kind == DateTimeKind.Unspecified)
                {
                    startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
                }
                if (endDate.Kind == DateTimeKind.Unspecified)
                {
                    endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
                }

                // Include full end day
                endDate = endDate.Date.AddDays(1).AddTicks(-1);

                var reportBytes = await _reportService.GeneratePartRequestReportAsync(startDate, endDate, userId);

                var fileName = $"PartRequests_{startDate:yyyy-MM-dd}_{endDate:yyyy-MM-dd}.xlsx";
                return File(reportBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating part request report");
                return StatusCode(500, new { message = "Ошибка генерации отчёта", error = ex.Message });
            }
        }

        /// <summary>
        /// Генерирует отчёт по переработке
        /// </summary>
        /// <param name="startDate">Начальная дата</param>
        /// <param name="endDate">Конечная дата</param>
        /// <param name="userId">ID пользователя (опционально)</param>
        [HttpGet("reprocessing")]
        public async Task<IActionResult> GetReprocessingReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int? userId = null)
        {
            try
            {
                // Convert to UTC if needed
                if (startDate.Kind == DateTimeKind.Unspecified)
                {
                    startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
                }
                if (endDate.Kind == DateTimeKind.Unspecified)
                {
                    endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
                }

                // Include full end day
                endDate = endDate.Date.AddDays(1).AddTicks(-1);

                var reportBytes = await _reportService.GenerateReprocessingReportAsync(startDate, endDate, userId);

                var fileName = $"Reprocessing_{startDate:yyyy-MM-dd}_{endDate:yyyy-MM-dd}.xlsx";
                return File(reportBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating reprocessing report");
                return StatusCode(500, new { message = "Ошибка генерации отчёта", error = ex.Message });
            }
        }
    }
}
