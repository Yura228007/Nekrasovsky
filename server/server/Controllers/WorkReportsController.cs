using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Services;

namespace server.Controllers
{
    public class StartWorkRequest
    {
        public int UserId { get; set; }
        public string? StartTime { get; set; } // Changed to string for validation
    }

    public class FinishWorkRequest
    {
        public string? FinishTime { get; set; } // Changed to string for validation
    }

    [ApiController]
    [Route("api/[controller]")]
    public class WorkReportsController : ControllerBase
    {
        private readonly IWorkReportService _workReportService;
        private readonly ILogger<WorkReportsController> _logger;

        public WorkReportsController(IWorkReportService workReportService, ILogger<WorkReportsController> logger)
        {
            _workReportService = workReportService;
            _logger = logger;
        }

        // GET: api/work-reports
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkReport>>> GetAll()
        {
            try
            {
                var reports = await _workReportService.GetAllWorkReportsAsync();
                return Ok(reports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all work reports");
                return StatusCode(500, new { message = "An error occurred while retrieving work reports" });
            }
        }

        // GET: api/work-reports/5
        [HttpGet("{id}")]
        public async Task<ActionResult<WorkReport>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                var report = await _workReportService.GetWorkReportByIdAsync(id);
                if (report == null)
                {
                    _logger.LogWarning("WorkReport with ID {WorkReportId} not found", id);
                    return NotFound(new { message = $"WorkReport with ID {id} not found" });
                }

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting work report with ID {WorkReportId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the work report" });
            }
        }

        // GET: api/work-reports/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<WorkReport>>> GetByUser(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new { message = "UserId must be greater than 0" });
                }

                var reports = await _workReportService.GetWorkReportsByUserAsync(userId);
                return Ok(reports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting work reports for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while retrieving work reports" });
            }
        }

        // GET: api/work-reports/date/{date}
        [HttpGet("date/{date}")]
        public async Task<ActionResult<IEnumerable<WorkReport>>> GetByDate(string date)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(date))
                {
                    return BadRequest(new { message = "Date cannot be empty" });
                }

                if (!DateTime.TryParse(date, out DateTime parsedDate))
                {
                    return BadRequest(new { message = "Date must be a valid DateTime format" });
                }

                var reports = await _workReportService.GetWorkReportsByDateAsync(parsedDate);
                return Ok(reports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting work reports by date");
                return StatusCode(500, new { message = "An error occurred while retrieving work reports" });
            }
        }

        // GET: api/work-reports/date-range?startDate=&endDate=
        [HttpGet("date-range")]
        public async Task<ActionResult<IEnumerable<WorkReport>>> GetByDateRange([FromQuery] string? startDate, [FromQuery] string? endDate)
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

                var reports = await _workReportService.GetWorkReportsByDateRangeAsync(parsedStartDate, parsedEndDate);
                return Ok(reports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting work reports by date range");
                return StatusCode(500, new { message = "An error occurred while retrieving work reports" });
            }
        }

        // GET: api/work-reports/user/{userId}/active
        [HttpGet("user/{userId}/active")]
        public async Task<ActionResult<IEnumerable<WorkReport>>> GetActive(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new { message = "UserId must be greater than 0" });
                }

                var reports = await _workReportService.GetActiveWorkReportsAsync(userId);
                return Ok(reports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting active work reports for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while retrieving work reports" });
            }
        }

        // GET: api/work-reports/{id}/part-requests
        [HttpGet("{id}/part-requests")]
        public async Task<ActionResult<IEnumerable<PartRequest>>> GetPartRequestsForReport(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                var partRequests = await _workReportService.GetPartRequestsForWorkReportAsync(id);
                return Ok(partRequests);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "WorkReport with ID {WorkReportId} not found", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting part requests for work report {WorkReportId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving part requests" });
            }
        }

        // POST: api/work-reports
        [HttpPost]
        public async Task<IActionResult> CreateWorkReport([FromBody] WorkReport report)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid model state", errors = ModelState });
            }

            try
            {
                // Validate Date - ensure it's not default
                if (report.Date == default(DateTime))
                {
                    return BadRequest(new { message = "Date cannot be empty or default" });
                }

                // Validate StartWork - ensure it's not default
                if (report.StartWork == default(DateTime))
                {
                    report.StartWork = DateTime.UtcNow;
                }

                var createdReport = await _workReportService.CreateWorkReportAsync(report);
                _logger.LogInformation("WorkReport created successfully with ID: {WorkReportId}", createdReport.Id);
                return CreatedAtAction(nameof(GetById), new { id = createdReport.Id },
                    new { message = "WorkReport created successfully", report = createdReport });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Key not found while creating work report");
                return NotFound(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while creating work report");
                return StatusCode(500, new { message = "An error occurred while saving the work report to the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating work report");
                return StatusCode(500, new { message = "An unexpected error occurred while creating the work report" });
            }
        }

        // PUT: api/work-reports/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWorkReport(int id, [FromBody] WorkReport updated)
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
                // Validate Date - ensure it's not default
                if (updated.Date == default(DateTime))
                {
                    return BadRequest(new { message = "Date cannot be empty or default" });
                }

                // Validate StartWork - ensure it's not default
                if (updated.StartWork == default(DateTime))
                {
                    return BadRequest(new { message = "StartWork cannot be empty or default" });
                }

                var report = await _workReportService.UpdateWorkReportAsync(id, updated);
                _logger.LogInformation("WorkReport updated successfully with ID: {WorkReportId}", id);
                return Ok(new { message = "WorkReport updated successfully", report });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "WorkReport with ID {WorkReportId} not found for update", id);
                return NotFound(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating work report with ID {WorkReportId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the work report in the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating work report with ID {WorkReportId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred while updating the work report" });
            }
        }

        // POST: api/work-reports/start
        [HttpPost("start")]
        public async Task<IActionResult> StartWork([FromBody] StartWorkRequest request)
        {
            try
            {
                if (request.UserId <= 0)
                {
                    return BadRequest(new { message = "UserId must be greater than 0" });
                }

                DateTime? parsedStartTime = null;
                if (!string.IsNullOrWhiteSpace(request.StartTime))
                {
                    if (!DateTime.TryParse(request.StartTime, out DateTime parsed))
                    {
                        return BadRequest(new { message = "StartTime must be a valid DateTime format or empty" });
                    }
                    parsedStartTime = parsed;
                }

                var report = await _workReportService.StartWorkAsync(request.UserId, parsedStartTime);
                _logger.LogInformation("Work started successfully for user {UserId}, WorkReport ID: {WorkReportId}", request.UserId, report.Id);
                return Ok(new { message = "Work started successfully", report });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found while starting work");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while starting work for user {UserId}", request.UserId);
                return StatusCode(500, new { message = "An unexpected error occurred while starting work" });
            }
        }

        // POST: api/work-reports/{id}/finish
        [HttpPost("{id}/finish")]
        public async Task<IActionResult> FinishWork(int id, [FromBody] FinishWorkRequest? request = null)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                DateTime? parsedFinishTime = null;
                if (request != null && !string.IsNullOrWhiteSpace(request.FinishTime))
                {
                    if (!DateTime.TryParse(request.FinishTime, out DateTime parsed))
                    {
                        return BadRequest(new { message = "FinishTime must be a valid DateTime format or empty" });
                    }
                    parsedFinishTime = parsed;
                }

                var report = await _workReportService.FinishWorkAsync(id, parsedFinishTime);
                _logger.LogInformation("Work finished successfully for WorkReport ID: {WorkReportId}", id);
                return Ok(new { message = "Work finished successfully", report });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "WorkReport with ID {WorkReportId} not found for finishing", id);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while finishing work for WorkReport ID {WorkReportId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while finishing work for WorkReport ID {WorkReportId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred while finishing work" });
            }
        }

        // DELETE: api/work-reports/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkReport(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                var deleted = await _workReportService.DeleteWorkReportAsync(id);
                if (!deleted)
                {
                    _logger.LogWarning("WorkReport with ID {WorkReportId} not found for deletion", id);
                    return NotFound(new { message = $"WorkReport with ID {id} not found" });
                }

                _logger.LogInformation("WorkReport deleted successfully with ID: {WorkReportId}", id);
                return Ok(new { message = "WorkReport deleted successfully" });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while deleting work report with ID {WorkReportId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the work report from the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting work report with ID {WorkReportId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred while deleting the work report" });
            }
        }
    }
}

