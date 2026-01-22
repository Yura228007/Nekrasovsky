using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Services;

namespace server.Controllers
{
    public class StartWorkRequest
    {
        public int UserId { get; set; }
        public string? StartTime { get; set; }
    }

    public class FinishWorkRequest
    {
        public string? FinishTime { get; set; }
    }

    [ApiController]
    [Route("api/work-reports")]
    public class WorkReportsController : ControllerBase
    {
        private readonly IWorkReportService _workReportService;
        private readonly ILogger<WorkReportsController> _logger;

        public WorkReportsController(
            IWorkReportService workReportService,
            ILogger<WorkReportsController> logger)
        {
            _workReportService = workReportService;
            _logger = logger;
        }

        // GET: api/work-reports
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkReport>>> GetAll()
        {
            var reports = await _workReportService.GetAllWorkReportsAsync();
            return Ok(reports);
        }

        // GET: api/work-reports/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<WorkReport>> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Id must be greater than 0" });

            var report = await _workReportService.GetWorkReportByIdAsync(id);
            if (report == null)
                return NotFound(new { message = $"WorkReport {id} not found" });

            return Ok(report);
        }

        // GET: api/work-reports/user/{userId}
        [HttpGet("user/{userId:int}")]
        public async Task<ActionResult<IEnumerable<WorkReport>>> GetByUser(int userId)
        {
            if (userId <= 0)
                return BadRequest(new { message = "UserId must be greater than 0" });

            var reports = await _workReportService.GetWorkReportsByUserAsync(userId);
            return Ok(reports);
        }

        // POST: api/work-reports/start
        [HttpPost("start")]
        public async Task<IActionResult> StartWork([FromBody] StartWorkRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Request body is required" });

            if (request.UserId <= 0)
                return BadRequest(new { message = "UserId must be greater than 0" });

            DateTime? startTime = null;
            if (!string.IsNullOrWhiteSpace(request.StartTime))
            {
                if (!DateTime.TryParse(request.StartTime, out var parsed))
                    return BadRequest(new { message = "Invalid StartTime format" });

                startTime = parsed;
            }

            var report = await _workReportService.StartWorkAsync(request.UserId, startTime);

            return Ok(new
            {
                message = "Work started successfully",
                report
            });
        }

        // POST: api/work-reports/{id}/finish
        [HttpPost("{id:int}/finish")]
        public async Task<IActionResult> FinishWork(int id, [FromBody] FinishWorkRequest? request)
        {
            if (id <= 0)
                return BadRequest(new { message = "Id must be greater than 0" });

            DateTime? finishTime = null;
            if (request != null && !string.IsNullOrWhiteSpace(request.FinishTime))
            {
                if (!DateTime.TryParse(request.FinishTime, out var parsed))
                    return BadRequest(new { message = "Invalid FinishTime format" });

                finishTime = parsed;
            }

            var report = await _workReportService.FinishWorkAsync(id, finishTime);

            return Ok(new
            {
                message = "Work finished successfully",
                report
            });
        }

        // DELETE: api/work-reports/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Id must be greater than 0" });

            var deleted = await _workReportService.DeleteWorkReportAsync(id);
            if (!deleted)
                return NotFound(new { message = $"WorkReport {id} not found" });

            return Ok(new { message = "WorkReport deleted successfully" });
        }
    }
}
