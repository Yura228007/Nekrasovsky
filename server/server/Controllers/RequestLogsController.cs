using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestLogsController : ControllerBase
    {
        private readonly IRequestLogService _requestLogService;

        public RequestLogsController(IRequestLogService requestLogService)
        {
            _requestLogService = requestLogService;
        }

        // GET: api/request-logs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RequestLog>>> GetAll()
        {
            var logs = await _requestLogService.GetAllRequestLogsAsync();
            return Ok(logs);
        }

        // GET: api/request-logs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RequestLog>> GetById(int id)
        {
            var log = await _requestLogService.GetRequestLogByIdAsync(id);
            if (log == null)
                return NotFound($"RequestLog with ID {id} not found");

            return Ok(log);
        }

        // GET: api/request-logs/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<RequestLog>>> GetByUser(int userId)
        {
            var logs = await _requestLogService.GetRequestLogsByUserAsync(userId);
            return Ok(logs);
        }

        // GET: api/request-logs/date-range?startDate=&endDate=
        [HttpGet("date-range")]
        public async Task<ActionResult<IEnumerable<RequestLog>>> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var logs = await _requestLogService.GetRequestLogsByDateRangeAsync(startDate, endDate);
            return Ok(logs);
        }

        // GET: api/request-logs/status/{statusCode}
        [HttpGet("status/{statusCode}")]
        public async Task<ActionResult<IEnumerable<RequestLog>>> GetByStatusCode(int statusCode)
        {
            var logs = await _requestLogService.GetRequestLogsByStatusCodeAsync(statusCode);
            return Ok(logs);
        }

        // GET: api/request-logs/controller/{controller}
        [HttpGet("controller/{controller}")]
        public async Task<ActionResult<IEnumerable<RequestLog>>> GetByController(string controller)
        {
            var logs = await _requestLogService.GetRequestLogsByControllerAsync(controller);
            return Ok(logs);
        }

        // GET: api/request-logs/action/{controller}/{action}
        [HttpGet("action/{controller}/{action}")]
        public async Task<ActionResult<IEnumerable<RequestLog>>> GetByAction(string controller, string action)
        {
            var logs = await _requestLogService.GetRequestLogsByActionAsync(controller, action);
            return Ok(logs);
        }

        // GET: api/request-logs/search?url=&httpMethod=&statusCode=&startDate=&endDate=
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<RequestLog>>> Search(
            [FromQuery] string? url,
            [FromQuery] string? httpMethod,
            [FromQuery] int? statusCode,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var logs = await _requestLogService.SearchRequestLogsAsync(url, httpMethod, statusCode, startDate, endDate);
            return Ok(logs);
        }

        // POST: api/request-logs/cleanup
        [HttpPost("cleanup")]
        public async Task<IActionResult> DeleteOldLogs([FromBody] DateTime beforeDate)
        {
            var deletedCount = await _requestLogService.DeleteOldRequestLogsAsync(beforeDate);
            return Ok(new { message = $"Deleted {deletedCount} old RequestLogs", deletedCount });
        }
    }
}

