using server.Data;
using server.Models;
using System.Security.Claims;
using System.Text;

namespace server.Middleware
{
    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditMiddleware> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private const int MaxBodyLength = 10_000; // ???????????? ?????????? ???????? ???? ???????/??????

        public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _next = next;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip logging for certain paths (Swagger, health checks, static files, etc.)
            if (ShouldSkipLogging(context))
            {
                await _next(context);
                return;
            }

            var startTime = DateTime.UtcNow;
            RequestLog? auditLog = null;

            try
            {
                // Create RequestLog object
                auditLog = new RequestLog
                {
                    HttpMethod = context.Request.Method,
                    Url = context.Request.Path + context.Request.QueryString,
                    RequestTime = startTime,
                    ClientIp = context.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = context.Request.Headers["User-Agent"].ToString(),
                    UserId = GetUserIdFromContext(context)
                };

                // Read request body
                if (context.Request.ContentLength > 0 &&
                    !context.Request.ContentType?.Contains("multipart/form-data") == true)
                {
                    context.Request.EnableBuffering();
                    using var reader = new StreamReader(
                        context.Request.Body,
                        Encoding.UTF8,
                        leaveOpen: true
                    );
                    var body = await reader.ReadToEndAsync();
                    auditLog.RequestBody = TruncateBody(body);
                    context.Request.Body.Position = 0;
                }

                // Create response body stream to intercept response
                var originalBodyStream = context.Response.Body;
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                // Invoke next middleware
                await _next(context);

                // Read response body and calculate duration
                responseBody.Seek(0, SeekOrigin.Begin);
                var responseText = await new StreamReader(responseBody).ReadToEndAsync();
                auditLog.ResponseBody = TruncateBody(responseText);
                auditLog.StatusCode = context.Response.StatusCode;
                auditLog.DurationMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

                // Extract controller and action from route data
                ExtractControllerAction(context, auditLog);

                // Extract entity IDs from route/body
                ExtractEntityIds(context, auditLog);

                // Restore original response stream and send response to client
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);

                // Save log to database and file asynchronously (fire-and-forget)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await SaveLogToDatabaseAsync(auditLog);
                        await SaveLogToFileAsync(auditLog);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to save audit log");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in AuditMiddleware: {ex.Message}");

                // Save error log if possible (asynchronously in background)
                if (auditLog != null)
                {
                    auditLog.StatusCode = 500;
                    auditLog.ResponseBody = $"Error: {ex.Message}";
                    auditLog.DurationMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await SaveLogToDatabaseAsync(auditLog);
                            await SaveLogToFileAsync(auditLog);
                        }
                        catch (Exception dbEx)
                        {
                            _logger.LogError(dbEx, "Failed to save error log");
                        }
                    });
                }

                throw;
            }
        }

        private int? GetUserIdFromContext(HttpContext context)
        {
            try
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                    return userId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not get user ID: {ex.Message}");
            }
            return null;
        }

        private void ExtractControllerAction(HttpContext context, RequestLog auditLog)
        {
            try
            {
                var routeData = context.GetRouteData();
                if (routeData != null)
                {
                    auditLog.Controller = routeData.Values["controller"]?.ToString();
                    auditLog.Action = routeData.Values["action"]?.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not extract controller/action: {ex.Message}");
            }
        }

        private void ExtractEntityIds(HttpContext context, RequestLog auditLog)
        {
            try
            {
                var routeData = context.GetRouteData();
                var controller = auditLog.Controller?.ToLower();

                if (routeData != null)
                {
                    if (controller == "warehouses" && routeData.Values.TryGetValue("id", out var warehouseId))
                    {
                        if (int.TryParse(warehouseId?.ToString(), out int whId))
                            auditLog.WarehouseId = whId;
                    }

                    if (controller == "materials" && routeData.Values.TryGetValue("id", out var materialId))
                    {
                        if (int.TryParse(materialId?.ToString(), out int matId))
                            auditLog.MaterialId = matId;
                    }

                    if (controller == "products" && routeData.Values.TryGetValue("id", out var productId))
                    {
                        if (int.TryParse(productId?.ToString(), out int prodId))
                            auditLog.ProductId = prodId;
                    }
                }

                if (!string.IsNullOrEmpty(auditLog.RequestBody))
                {
                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(auditLog.RequestBody);
                        var root = doc.RootElement;

                        if (controller == "warehouses" && root.TryGetProperty("id", out var whIdProp) && whIdProp.TryGetInt32(out int whId))
                            auditLog.WarehouseId = whId;

                        if (controller == "materials" && root.TryGetProperty("id", out var matIdProp) && matIdProp.TryGetInt32(out int matId))
                            auditLog.MaterialId = matId;

                        if (controller == "products" && root.TryGetProperty("id", out var prodIdProp) && prodIdProp.TryGetInt32(out int prodId))
                            auditLog.ProductId = prodId;
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not extract entity IDs: {ex.Message}");
            }
        }

        private bool ShouldSkipLogging(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
            var method = context.Request.Method.ToUpperInvariant();

            // Skip Swagger UI and API documentation
            if (path.StartsWith("/swagger") || path.StartsWith("/swaggerui"))
                return true;

            // Skip health checks
            if (path.StartsWith("/health") || path == "/healthz" || path == "/ready")
                return true;

            // Skip static files (CSS, JS, images, fonts, etc.)
            var staticFileExtensions = new[] { ".css", ".js", ".png", ".jpg", ".jpeg", ".gif", ".ico", ".svg", ".woff", ".woff2", ".ttf", ".eot" };
            if (staticFileExtensions.Any(ext => path.EndsWith(ext)))
                return true;

            // Skip favicon
            if (path == "/favicon.ico")
                return true;

            // Skip GET requests to RequestLogs (to avoid logging the logging requests)
            if (method == "GET" && path.StartsWith("/api/request-logs"))
                return true;

            return false;
        }

        private async Task SaveLogToDatabaseAsync(RequestLog auditLog)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            try
            {
                dbContext.RequestLogs.Add(auditLog);
                await dbContext.SaveChangesAsync();
                _logger.LogDebug("RequestLog saved to DB with ID: {LogId}", auditLog.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save RequestLog to database");
                throw;
            }
        }

        private async Task SaveLogToFileAsync(RequestLog auditLog)
        {
            try
            {
                // Create folder Logs/requests if not exists
                var logsDir = Path.Combine(AppContext.BaseDirectory, "Logs", "requests");
                Directory.CreateDirectory(logsDir);

                // File per day (UTC)
                var logFilePath = Path.Combine(logsDir, $"{DateTime.UtcNow:yyyy-MM-dd}.log");

                var logEntry = new StringBuilder();
                logEntry.AppendLine("=======================================================");
                logEntry.AppendLine($"Timestamp: {DateTime.UtcNow:O}");
                logEntry.AppendLine($"Method: {auditLog.HttpMethod}");
                logEntry.AppendLine($"URL: {auditLog.Url}");
                logEntry.AppendLine($"Status: {auditLog.StatusCode}");
                logEntry.AppendLine($"Duration: {auditLog.DurationMs} ms");
                logEntry.AppendLine($"UserId: {auditLog.UserId}");
                logEntry.AppendLine($"Client IP: {auditLog.ClientIp}");
                logEntry.AppendLine($"User-Agent: {auditLog.UserAgent}");
                if (!string.IsNullOrEmpty(auditLog.Controller))
                    logEntry.AppendLine($"Controller: {auditLog.Controller}, Action: {auditLog.Action}");
                if (!string.IsNullOrEmpty(auditLog.RequestBody))
                    logEntry.AppendLine($"Request Body: {TruncateBody(auditLog.RequestBody)}");
                if (!string.IsNullOrEmpty(auditLog.ResponseBody))
                    logEntry.AppendLine($"Response Body: {TruncateBody(auditLog.ResponseBody)}");
                logEntry.AppendLine("=======================================================");
                logEntry.AppendLine();

                await File.AppendAllTextAsync(logFilePath, logEntry.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save audit log to file");
            }
        }

        private string TruncateBody(string? body)
        {
            if (string.IsNullOrEmpty(body))
                return string.Empty;

            if (body.Length <= MaxBodyLength)
                return body;

            return body.Substring(0, MaxBodyLength) + "... [TRUNCATED]";
        }
    }
}
