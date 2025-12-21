using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.Services;

namespace server.Controllers
{
    public class StartWorkRequest
    {
        public int UserId { get; set; }
        public DateTime? StartTime { get; set; }
    }

    public class FinishWorkRequest
    {
        public DateTime? FinishTime { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class WorkReportsController : ControllerBase
    {
        private readonly IWorkReportService _workReportService;

        public WorkReportsController(IWorkReportService workReportService)
        {
            _workReportService = workReportService;
        }

        // GET: api/work-reports
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkReport>>> GetAll()
        {
            var reports = await _workReportService.GetAllWorkReportsAsync();
            return Ok(reports);
        }

        // GET: api/work-reports/5
        [HttpGet("{id}")]
        public async Task<ActionResult<WorkReport>> GetById(int id)
        {
            var report = await _workReportService.GetWorkReportByIdAsync(id);
            if (report == null)
                return NotFound($"WorkReport with ID {id} not found");

            return Ok(report);
        }

        // GET: api/work-reports/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<WorkReport>>> GetByUser(int userId)
        {
            var reports = await _workReportService.GetWorkReportsByUserAsync(userId);
            return Ok(reports);
        }

        // GET: api/work-reports/date/{date}
        [HttpGet("date/{date}")]
        public async Task<ActionResult<IEnumerable<WorkReport>>> GetByDate(DateTime date)
        {
            var reports = await _workReportService.GetWorkReportsByDateAsync(date);
            return Ok(reports);
        }

        // GET: api/work-reports/date-range?startDate=&endDate=
        [HttpGet("date-range")]
        public async Task<ActionResult<IEnumerable<WorkReport>>> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var reports = await _workReportService.GetWorkReportsByDateRangeAsync(startDate, endDate);
            return Ok(reports);
        }

        // GET: api/work-reports/user/{userId}/active
        [HttpGet("user/{userId}/active")]
        public async Task<ActionResult<IEnumerable<WorkReport>>> GetActive(int userId)
        {
            var reports = await _workReportService.GetActiveWorkReportsAsync(userId);
            return Ok(reports);
        }

        // POST: api/work-reports/add
        [HttpPost("add")]
        public async Task<IActionResult> AddWorkReport([FromBody] WorkReport report)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdReport = await _workReportService.CreateWorkReportAsync(report);
                return Ok(new { message = "WorkReport created successfully", report = createdReport });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/work-reports/edit/{id}
        [HttpPost("edit/{id}")]
        public async Task<IActionResult> EditWorkReport(int id, [FromBody] WorkReport updated)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var report = await _workReportService.UpdateWorkReportAsync(id, updated);
                return Ok(new { message = "WorkReport updated successfully", report });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/work-reports/start
        [HttpPost("start")]
        public async Task<IActionResult> StartWork([FromBody] StartWorkRequest request)
        {
            try
            {
                var report = await _workReportService.StartWorkAsync(request.UserId, request.StartTime);
                return Ok(new { message = "Work started successfully", report });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/work-reports/{id}/finish
        [HttpPost("{id}/finish")]
        public async Task<IActionResult> FinishWork(int id, [FromBody] FinishWorkRequest? request = null)
        {
            try
            {
                var report = await _workReportService.FinishWorkAsync(id, request?.FinishTime);
                return Ok(new { message = "Work finished successfully", report });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/work-reports/delete/{id}
        [HttpPost("delete/{id}")]
        public async Task<IActionResult> DeleteWorkReport(int id)
        {
            var deleted = await _workReportService.DeleteWorkReportAsync(id);
            if (!deleted)
                return NotFound($"WorkReport with ID {id} not found");

            return Ok(new { message = "WorkReport deleted successfully" });
        }
    }
}

