using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestLogsController : ControllerBase
    {
        private readonly IRequestLogService _requestLogService;
        private readonly ILogger<RequestLogsController> _logger;

        public RequestLogsController(IRequestLogService requestLogService, ILogger<RequestLogsController> logger)
        {
            _requestLogService = requestLogService;
            _logger = logger;
        }

        // GET: api/request-logs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RequestLog>>> GetAll()
        {
            try
            {
                var logs = await _requestLogService.GetAllRequestLogsAsync();
                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all request logs");
                return StatusCode(500, new { message = "An error occurred while retrieving request logs" });
            }
        }

        // GET: api/request-logs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RequestLog>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                var log = await _requestLogService.GetRequestLogByIdAsync(id);
                if (log == null)
                {
                    _logger.LogWarning("RequestLog with ID {RequestLogId} not found", id);
                    return NotFound(new { message = $"RequestLog with ID {id} not found" });
                }

                return Ok(log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting request log with ID {RequestLogId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the request log" });
            }
        }

        // GET: api/request-logs/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<RequestLog>>> GetByUser(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new { message = "UserId must be greater than 0" });
                }

                var logs = await _requestLogService.GetRequestLogsByUserAsync(userId);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting request logs for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while retrieving request logs" });
            }
        }

        // GET: api/request-logs/date-range?startDate=&endDate=
        [HttpGet("date-range")]
        public async Task<ActionResult<IEnumerable<RequestLog>>> GetByDateRange([FromQuery] string? startDate, [FromQuery] string? endDate)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(startDate))
                {
                    return BadRequest(new { message = "StartDate cannot be empty" });
                }

                if (!DateTime.TryParse(startDate, out DateTime parsedStartDate))
                {
                    return BadRequest(new { message = "StartDate must be a valid DateTime format" });
                }

                if (string.IsNullOrWhiteSpace(endDate))
                {
                    return BadRequest(new { message = "EndDate cannot be empty" });
                }

                if (!DateTime.TryParse(endDate, out DateTime parsedEndDate))
                {
                    return BadRequest(new { message = "EndDate must be a valid DateTime format" });
                }

                if (parsedStartDate > parsedEndDate)
                {
                    return BadRequest(new { message = "StartDate must be less than or equal to EndDate" });
                }

                var logs = await _requestLogService.GetRequestLogsByDateRangeAsync(parsedStartDate, parsedEndDate);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting request logs by date range");
                return StatusCode(500, new { message = "An error occurred while retrieving request logs" });
            }
        }

        // GET: api/request-logs/status/{statusCode}
        [HttpGet("status/{statusCode}")]
        public async Task<ActionResult<IEnumerable<RequestLog>>> GetByStatusCode(int statusCode)
        {
            try
            {
                if (statusCode < 100 || statusCode > 599)
                {
                    return BadRequest(new { message = "StatusCode must be between 100 and 599" });
                }

                var logs = await _requestLogService.GetRequestLogsByStatusCodeAsync(statusCode);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting request logs by status code {StatusCode}", statusCode);
                return StatusCode(500, new { message = "An error occurred while retrieving request logs" });
            }
        }

        // GET: api/request-logs/controller/{controller}
        [HttpGet("controller/{controller}")]
        public async Task<ActionResult<IEnumerable<RequestLog>>> GetByController(string controller)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(controller))
                {
                    return BadRequest(new { message = "Controller cannot be empty" });
                }

                var logs = await _requestLogService.GetRequestLogsByControllerAsync(controller);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting request logs by controller '{Controller}'", controller);
                return StatusCode(500, new { message = "An error occurred while retrieving request logs" });
            }
        }

        // GET: api/request-logs/action/{controller}/{action}
        [HttpGet("action/{controller}/{action}")]
        public async Task<ActionResult<IEnumerable<RequestLog>>> GetByAction(string controller, string action)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(controller))
                {
                    return BadRequest(new { message = "Controller cannot be empty" });
                }

                if (string.IsNullOrWhiteSpace(action))
                {
                    return BadRequest(new { message = "Action cannot be empty" });
                }

                var logs = await _requestLogService.GetRequestLogsByActionAsync(controller, action);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting request logs by action '{Controller}/{Action}'", controller, action);
                return StatusCode(500, new { message = "An error occurred while retrieving request logs" });
            }
        }

        // GET: api/request-logs/search?url=&httpMethod=&statusCode=&startDate=&endDate=
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<RequestLog>>> Search(
            [FromQuery] string? url,
            [FromQuery] string? httpMethod,
            [FromQuery] int? statusCode,
            [FromQuery] string? startDate,
            [FromQuery] string? endDate)
        {
            try
            {
                DateTime? parsedStartDate = null;
                if (!string.IsNullOrWhiteSpace(startDate))
                {
                    if (!DateTime.TryParse(startDate, out DateTime parsed))
                    {
                        return BadRequest(new { message = "StartDate must be a valid DateTime format or empty" });
                    }
                    parsedStartDate = parsed;
                }

                DateTime? parsedEndDate = null;
                if (!string.IsNullOrWhiteSpace(endDate))
                {
                    if (!DateTime.TryParse(endDate, out DateTime parsed))
                    {
                        return BadRequest(new { message = "EndDate must be a valid DateTime format or empty" });
                    }
                    parsedEndDate = parsed;
                }

                if (parsedStartDate.HasValue && parsedEndDate.HasValue && parsedStartDate > parsedEndDate)
                {
                    return BadRequest(new { message = "StartDate must be less than or equal to EndDate" });
                }

                var logs = await _requestLogService.SearchRequestLogsAsync(url, httpMethod, statusCode, parsedStartDate, parsedEndDate);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching request logs");
                return StatusCode(500, new { message = "An error occurred while searching request logs" });
            }
        }

        // POST: api/request-logs/cleanup
        [HttpPost("cleanup")]
        public async Task<IActionResult> DeleteOldLogs([FromBody] string? beforeDate)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(beforeDate))
                {
                    return BadRequest(new { message = "BeforeDate cannot be empty" });
                }

                if (!DateTime.TryParse(beforeDate, out DateTime parsedBeforeDate))
                {
                    return BadRequest(new { message = "BeforeDate must be a valid DateTime format" });
                }

                var deletedCount = await _requestLogService.DeleteOldRequestLogsAsync(parsedBeforeDate);
                _logger.LogInformation("Deleted {Count} old RequestLogs before {Date}", deletedCount, parsedBeforeDate);
                return Ok(new { message = $"Deleted {deletedCount} old RequestLogs", deletedCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting old request logs");
                return StatusCode(500, new { message = "An error occurred while deleting old request logs" });
            }
        }
    }
}
