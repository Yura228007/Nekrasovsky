using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShiftReportsController : ControllerBase
    {
        private readonly IShiftReportService _shiftReportService;
        private readonly AppDbContext _context;
        private readonly ILogger<ShiftReportsController> _logger;

        public ShiftReportsController(
            IShiftReportService shiftReportService,
            AppDbContext context,
            ILogger<ShiftReportsController> logger)
        {
            _shiftReportService = shiftReportService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all shift reports (Admin/Owner only)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShiftReport>>> GetAll([FromQuery] int? requestingUserId)
        {
            if (!requestingUserId.HasValue)
            {
                return BadRequest(new { message = "requestingUserId is required" });
            }

            var canAccess = await CanAccessAllReports(requestingUserId.Value);
            if (!canAccess)
            {
                return Forbid();
            }

            var reports = await _shiftReportService.GetAllReportsAsync();
            return Ok(reports);
        }

        /// <summary>
        /// Get shift report by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ShiftReport>> GetById(int id, [FromQuery] int? requestingUserId)
        {
            if (!requestingUserId.HasValue)
            {
                return BadRequest(new { message = "requestingUserId is required" });
            }

            var report = await _shiftReportService.GetReportByIdAsync(id);
            if (report == null)
            {
                return NotFound(new { message = $"ShiftReport with ID {id} not found" });
            }

            var canAccess = await CanAccessReport(requestingUserId.Value, report);
            if (!canAccess)
            {
                return Forbid();
            }

            return Ok(report);
        }

        /// <summary>
        /// Get shift report by WorkReport ID
        /// </summary>
        [HttpGet("by-work-report/{workReportId}")]
        public async Task<ActionResult<ShiftReport>> GetByWorkReportId(int workReportId, [FromQuery] int? requestingUserId)
        {
            if (!requestingUserId.HasValue)
            {
                return BadRequest(new { message = "requestingUserId is required" });
            }

            var report = await _shiftReportService.GetReportByWorkReportIdAsync(workReportId);
            if (report == null)
            {
                return NotFound(new { message = $"ShiftReport for WorkReport {workReportId} not found" });
            }

            var canAccess = await CanAccessReport(requestingUserId.Value, report);
            if (!canAccess)
            {
                return Forbid();
            }

            return Ok(report);
        }

        /// <summary>
        /// Get shift reports for a specific user
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ShiftReport>>> GetByUser(int userId, [FromQuery] int? requestingUserId)
        {
            if (!requestingUserId.HasValue)
            {
                return BadRequest(new { message = "requestingUserId is required" });
            }

            // User can view their own reports, admin/owner can view any
            if (requestingUserId.Value != userId)
            {
                var canAccess = await CanAccessAllReports(requestingUserId.Value);
                if (!canAccess)
                {
                    return Forbid();
                }
            }

            var reports = await _shiftReportService.GetReportsByUserAsync(userId);
            return Ok(reports);
        }

        /// <summary>
        /// Get shift reports by date range (Admin/Owner only)
        /// </summary>
        [HttpGet("date-range")]
        public async Task<ActionResult<IEnumerable<ShiftReport>>> GetByDateRange(
            [FromQuery] string startDate,
            [FromQuery] string endDate,
            [FromQuery] int? requestingUserId)
        {
            if (!requestingUserId.HasValue)
            {
                return BadRequest(new { message = "requestingUserId is required" });
            }

            var canAccess = await CanAccessAllReports(requestingUserId.Value);
            if (!canAccess)
            {
                return Forbid();
            }

            if (!DateTime.TryParse(startDate, out var start) || !DateTime.TryParse(endDate, out var end))
            {
                return BadRequest(new { message = "Invalid date format" });
            }

            var reports = await _shiftReportService.GetReportsByDateRangeAsync(start, end);
            return Ok(reports);
        }

        /// <summary>
        /// Download shift report Excel (Admin/Owner or own report)
        /// </summary>
        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadReport(int id, [FromQuery] int? requestingUserId)
        {
            if (!requestingUserId.HasValue)
            {
                return BadRequest(new { message = "requestingUserId is required" });
            }

            try
            {
                var canDownload = await _shiftReportService.CanUserDownloadReportAsync(requestingUserId.Value, id);
                if (!canDownload)
                {
                    _logger.LogWarning("User {UserId} attempted to download report {ReportId} without permission",
                        requestingUserId.Value, id);
                    return Forbid();
                }

                var report = await _shiftReportService.GetReportByIdAsync(id);
                if (report == null)
                {
                    return NotFound(new { message = $"ShiftReport with ID {id} not found" });
                }

                var fileBytes = await _shiftReportService.GetReportFileAsync(id);

                _logger.LogInformation("User {UserId} downloaded report {ReportId}", requestingUserId.Value, id);

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", report.FileName);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex, "Report file not found for report {ReportId}", id);
                return NotFound(new { message = "Report file not found on server" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading report {ReportId}", id);
                return StatusCode(500, new { message = "Error downloading report" });
            }
        }

        /// <summary>
        /// Generate report for a completed shift (Admin/Owner only or own shift)
        /// </summary>
        [HttpPost("generate/{workReportId}")]
        public async Task<ActionResult<ShiftReport>> GenerateReport(int workReportId, [FromQuery] int? requestingUserId)
        {
            if (!requestingUserId.HasValue)
            {
                return BadRequest(new { message = "requestingUserId is required" });
            }

            try
            {
                var workReport = await _context.WorkReports.FindAsync(workReportId);
                if (workReport == null)
                {
                    return NotFound(new { message = $"WorkReport with ID {workReportId} not found" });
                }

                // Check permissions
                if (workReport.UserId != requestingUserId.Value)
                {
                    var canAccess = await CanAccessAllReports(requestingUserId.Value);
                    if (!canAccess)
                    {
                        return Forbid();
                    }
                }

                var report = await _shiftReportService.GenerateReportAsync(workReportId);
                return Ok(new { message = "Report generated successfully", report });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report for WorkReport {WorkReportId}", workReportId);
                return StatusCode(500, new { message = "Error generating report" });
            }
        }

        /// <summary>
        /// Delete a shift report (Admin/Owner only)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReport(int id, [FromQuery] int? requestingUserId)
        {
            if (!requestingUserId.HasValue)
            {
                return BadRequest(new { message = "requestingUserId is required" });
            }

            var canAccess = await CanAccessAllReports(requestingUserId.Value);
            if (!canAccess)
            {
                return Forbid();
            }

            var deleted = await _shiftReportService.DeleteReportAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"ShiftReport with ID {id} not found" });
            }

            _logger.LogInformation("User {UserId} deleted report {ReportId}", requestingUserId.Value, id);
            return Ok(new { message = "Report deleted successfully" });
        }

        private async Task<bool> CanAccessAllReports(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return false;
            }

            var roleCode = user.Role?.Code ?? string.Empty;
            return roleCode == "Owner" || roleCode == "Admin";
        }

        private async Task<bool> CanAccessReport(int requestingUserId, ShiftReport report)
        {
            // User can always access their own report
            if (report.UserId == requestingUserId)
            {
                return true;
            }

            // Otherwise, must be admin or owner
            return await CanAccessAllReports(requestingUserId);
        }
    }
}
