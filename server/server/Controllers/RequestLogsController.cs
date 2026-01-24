using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Services;
using System.IO;
using System.Text;
using CsvHelper;
using ClosedXML.Excel;

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

        // GET: api/request-logs/advanced-search
        [HttpGet("advanced-search")]
        public async Task<ActionResult<object>> AdvancedSearch(
            [FromQuery] int? userId,
            [FromQuery] string? controller,
            [FromQuery] string? action,
            [FromQuery] string? httpMethod,
            [FromQuery] int? statusCode,
            [FromQuery] string? url,
            [FromQuery] string? startDate,
            [FromQuery] string? endDate,
            [FromQuery] int? warehouseId,
            [FromQuery] int? materialId,
            [FromQuery] int? productId,
            [FromQuery] long? minDurationMs,
            [FromQuery] long? maxDurationMs,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 100)
        {
            try
            {
                DateTime? parsedStartDate = null;
                if (!string.IsNullOrWhiteSpace(startDate))
                {
                    if (!DateTime.TryParse(startDate, out DateTime parsed))
                    {
                        return BadRequest(new { message = "StartDate must be a valid DateTime format" });
                    }
                    parsedStartDate = parsed;
                }

                DateTime? parsedEndDate = null;
                if (!string.IsNullOrWhiteSpace(endDate))
                {
                    if (!DateTime.TryParse(endDate, out DateTime parsed))
                    {
                        return BadRequest(new { message = "EndDate must be a valid DateTime format" });
                    }
                    parsedEndDate = parsed;
                }

                if (parsedStartDate.HasValue && parsedEndDate.HasValue && parsedStartDate > parsedEndDate)
                {
                    return BadRequest(new { message = "StartDate must be less than or equal to EndDate" });
                }

                if (pageNumber < 1)
                {
                    return BadRequest(new { message = "PageNumber must be greater than 0" });
                }

                if (pageSize < 1 || pageSize > 1000)
                {
                    return BadRequest(new { message = "PageSize must be between 1 and 1000" });
                }

                var logs = await _requestLogService.AdvancedSearchRequestLogsAsync(
                    userId, controller, action, httpMethod, statusCode, url,
                    parsedStartDate, parsedEndDate, warehouseId, materialId, productId,
                    minDurationMs, maxDurationMs, pageNumber, pageSize);

                var totalCount = await _requestLogService.GetLogsCountAsync(
                    userId, controller, action, httpMethod, statusCode, url,
                    parsedStartDate, parsedEndDate, warehouseId, materialId, productId,
                    minDurationMs, maxDurationMs);

                return Ok(new
                {
                    logs,
                    totalCount,
                    pageNumber,
                    pageSize,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while performing advanced search on request logs");
                return StatusCode(500, new { message = "An error occurred while searching request logs" });
            }
        }

        // GET: api/request-logs/export
        [HttpGet("export")]
        public async Task<IActionResult> ExportLogs(
            [FromQuery] string format,
            [FromQuery] int? userId,
            [FromQuery] string? controller,
            [FromQuery] string? action,
            [FromQuery] string? httpMethod,
            [FromQuery] int? statusCode,
            [FromQuery] string? url,
            [FromQuery] string? startDate,
            [FromQuery] string? endDate,
            [FromQuery] int? warehouseId,
            [FromQuery] int? materialId,
            [FromQuery] int? productId,
            [FromQuery] long? minDurationMs,
            [FromQuery] long? maxDurationMs)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(format) || (format != "csv" && format != "xlsx"))
                {
                    return BadRequest(new { message = "Format must be either 'csv' or 'xlsx'" });
                }

                DateTime? parsedStartDate = null;
                if (!string.IsNullOrWhiteSpace(startDate))
                {
                    if (!DateTime.TryParse(startDate, out DateTime parsed))
                    {
                        return BadRequest(new { message = "StartDate must be a valid DateTime format" });
                    }
                    parsedStartDate = parsed;
                }

                DateTime? parsedEndDate = null;
                if (!string.IsNullOrWhiteSpace(endDate))
                {
                    if (!DateTime.TryParse(endDate, out DateTime parsed))
                    {
                        return BadRequest(new { message = "EndDate must be a valid DateTime format" });
                    }
                    parsedEndDate = parsed;
                }

                var logs = await _requestLogService.AdvancedSearchRequestLogsAsync(
                    userId, controller, action, httpMethod, statusCode, url,
                    parsedStartDate, parsedEndDate, warehouseId, materialId, productId,
                    minDurationMs, maxDurationMs, 1, int.MaxValue);

                if (format == "csv")
                {
                    return ExportToCsv(logs);
                }
                else
                {
                    return ExportToExcel(logs);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while exporting request logs");
                return StatusCode(500, new { message = "An error occurred while exporting request logs" });
            }
        }

        private IActionResult ExportToCsv(IEnumerable<RequestLog> logs)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
            using var csv = new CsvHelper.CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture);

            csv.WriteField("ID");
            csv.WriteField("Request Time");
            csv.WriteField("HTTP Method");
            csv.WriteField("URL");
            csv.WriteField("Controller");
            csv.WriteField("Action");
            csv.WriteField("Status Code");
            csv.WriteField("Duration (ms)");
            csv.WriteField("User ID");
            csv.WriteField("User Name");
            csv.WriteField("Warehouse ID");
            csv.WriteField("Warehouse Name");
            csv.WriteField("Material ID");
            csv.WriteField("Material Name");
            csv.WriteField("Product ID");
            csv.WriteField("Product Name");
            csv.WriteField("Client IP");
            csv.WriteField("User Agent");
            csv.NextRecord();

            foreach (var log in logs)
            {
                csv.WriteField(log.Id);
                csv.WriteField(log.RequestTime.ToString("yyyy-MM-dd HH:mm:ss"));
                csv.WriteField(log.HttpMethod);
                csv.WriteField(log.Url);
                csv.WriteField(log.Controller ?? "");
                csv.WriteField(log.Action ?? "");
                csv.WriteField(log.StatusCode);
                csv.WriteField(log.DurationMs);
                csv.WriteField(log.UserId?.ToString() ?? "");
                csv.WriteField(log.User != null ? $"{log.User.Name} {log.User.Surname}" : "");
                csv.WriteField(log.WarehouseId?.ToString() ?? "");
                csv.WriteField(log.Warehouse?.Name ?? "");
                csv.WriteField(log.MaterialId?.ToString() ?? "");
                csv.WriteField(log.Material?.Name ?? "");
                csv.WriteField(log.ProductId?.ToString() ?? "");
                csv.WriteField(log.Product?.Name ?? "");
                csv.WriteField(log.ClientIp ?? "");
                csv.WriteField(log.UserAgent ?? "");
                csv.NextRecord();
            }

            writer.Flush();
            var bytes = memoryStream.ToArray();
            var fileName = $"request_logs_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";

            return File(bytes, "text/csv", fileName);
        }

        private IActionResult ExportToExcel(IEnumerable<RequestLog> logs)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Request Logs");

            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Request Time";
            worksheet.Cell(1, 3).Value = "HTTP Method";
            worksheet.Cell(1, 4).Value = "URL";
            worksheet.Cell(1, 5).Value = "Controller";
            worksheet.Cell(1, 6).Value = "Action";
            worksheet.Cell(1, 7).Value = "Status Code";
            worksheet.Cell(1, 8).Value = "Duration (ms)";
            worksheet.Cell(1, 9).Value = "User ID";
            worksheet.Cell(1, 10).Value = "User Name";
            worksheet.Cell(1, 11).Value = "Warehouse ID";
            worksheet.Cell(1, 12).Value = "Warehouse Name";
            worksheet.Cell(1, 13).Value = "Material ID";
            worksheet.Cell(1, 14).Value = "Material Name";
            worksheet.Cell(1, 15).Value = "Product ID";
            worksheet.Cell(1, 16).Value = "Product Name";
            worksheet.Cell(1, 17).Value = "Client IP";
            worksheet.Cell(1, 18).Value = "User Agent";

            var headerRange = worksheet.Range(1, 1, 1, 18);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            int row = 2;
            foreach (var log in logs)
            {
                worksheet.Cell(row, 1).Value = log.Id;
                worksheet.Cell(row, 2).Value = log.RequestTime.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cell(row, 3).Value = log.HttpMethod;
                worksheet.Cell(row, 4).Value = log.Url;
                worksheet.Cell(row, 5).Value = log.Controller ?? "";
                worksheet.Cell(row, 6).Value = log.Action ?? "";
                worksheet.Cell(row, 7).Value = log.StatusCode;
                worksheet.Cell(row, 8).Value = log.DurationMs;
                worksheet.Cell(row, 9).Value = log.UserId?.ToString() ?? "";
                worksheet.Cell(row, 10).Value = log.User != null ? $"{log.User.Name} {log.User.Surname}" : "";
                worksheet.Cell(row, 11).Value = log.WarehouseId?.ToString() ?? "";
                worksheet.Cell(row, 12).Value = log.Warehouse?.Name ?? "";
                worksheet.Cell(row, 13).Value = log.MaterialId?.ToString() ?? "";
                worksheet.Cell(row, 14).Value = log.Material?.Name ?? "";
                worksheet.Cell(row, 15).Value = log.ProductId?.ToString() ?? "";
                worksheet.Cell(row, 16).Value = log.Product?.Name ?? "";
                worksheet.Cell(row, 17).Value = log.ClientIp ?? "";
                worksheet.Cell(row, 18).Value = log.UserAgent ?? "";
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);
            var bytes = memoryStream.ToArray();
            var fileName = $"request_logs_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";

            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
