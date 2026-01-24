using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Data;

namespace server.Services
{
    public class RequestLogService : IRequestLogService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RequestLogService> _logger;

        public RequestLogService(AppDbContext context, ILogger<RequestLogService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<RequestLog>> GetAllRequestLogsAsync()
        {
            return await _context.RequestLogs
                .OrderByDescending(rl => rl.RequestTime)
                .ToListAsync();
        }

        public async Task<RequestLog?> GetRequestLogByIdAsync(int id)
        {
            return await _context.RequestLogs.FindAsync(id);
        }

        public async Task<IEnumerable<RequestLog>> GetRequestLogsByUserAsync(int userId)
        {
            return await _context.RequestLogs
                .Where(rl => rl.UserId == userId)
                .OrderByDescending(rl => rl.RequestTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<RequestLog>> GetRequestLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.RequestLogs
                .Where(rl => rl.RequestTime >= startDate && rl.RequestTime <= endDate)
                .OrderByDescending(rl => rl.RequestTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<RequestLog>> GetRequestLogsByStatusCodeAsync(int statusCode)
        {
            return await _context.RequestLogs
                .Where(rl => rl.StatusCode == statusCode)
                .OrderByDescending(rl => rl.RequestTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<RequestLog>> GetRequestLogsByControllerAsync(string controller)
        {
            return await _context.RequestLogs
                .Where(rl => rl.Controller != null && EF.Functions.ILike(rl.Controller, $"%{controller}%"))
                .OrderByDescending(rl => rl.RequestTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<RequestLog>> GetRequestLogsByActionAsync(string controller, string action)
        {
            return await _context.RequestLogs
                .Where(rl => rl.Controller != null && rl.Action != null 
                    && EF.Functions.ILike(rl.Controller, $"%{controller}%")
                    && EF.Functions.ILike(rl.Action, $"%{action}%"))
                .OrderByDescending(rl => rl.RequestTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<RequestLog>> SearchRequestLogsAsync(string? url, string? httpMethod, int? statusCode, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.RequestLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(url))
                query = query.Where(rl => EF.Functions.ILike(rl.Url, $"%{url}%"));

            if (!string.IsNullOrWhiteSpace(httpMethod))
                query = query.Where(rl => rl.HttpMethod == httpMethod);

            if (statusCode.HasValue)
                query = query.Where(rl => rl.StatusCode == statusCode.Value);

            if (startDate.HasValue)
                query = query.Where(rl => rl.RequestTime >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(rl => rl.RequestTime <= endDate.Value);

            return await query
                .OrderByDescending(rl => rl.RequestTime)
                .ToListAsync();
        }

        public async Task<int> DeleteOldRequestLogsAsync(DateTime beforeDate)
        {
            var logsToDelete = await _context.RequestLogs
                .Where(rl => rl.RequestTime < beforeDate)
                .ToListAsync();

            var count = logsToDelete.Count;
            _context.RequestLogs.RemoveRange(logsToDelete);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted {Count} old RequestLogs before {Date}", count, beforeDate);
            return count;
        }

        public async Task<IEnumerable<RequestLog>> AdvancedSearchRequestLogsAsync(
            int? userId, string? controller, string? action, string? httpMethod,
            int? statusCode, string? url, DateTime? startDate, DateTime? endDate,
            int? warehouseId, int? materialId, int? productId,
            long? minDurationMs, long? maxDurationMs,
            int pageNumber = 1, int pageSize = 100)
        {
            var query = _context.RequestLogs
                .Include(rl => rl.User)
                .Include(rl => rl.Warehouse)
                .Include(rl => rl.Material)
                .Include(rl => rl.Product)
                .AsQueryable();

            if (userId.HasValue)
                query = query.Where(rl => rl.UserId == userId.Value);

            if (!string.IsNullOrWhiteSpace(controller))
                query = query.Where(rl => rl.Controller != null && EF.Functions.ILike(rl.Controller, $"%{controller}%"));

            if (!string.IsNullOrWhiteSpace(action))
                query = query.Where(rl => rl.Action != null && EF.Functions.ILike(rl.Action, $"%{action}%"));

            if (!string.IsNullOrWhiteSpace(httpMethod))
                query = query.Where(rl => rl.HttpMethod == httpMethod);

            if (statusCode.HasValue)
                query = query.Where(rl => rl.StatusCode == statusCode.Value);

            if (!string.IsNullOrWhiteSpace(url))
                query = query.Where(rl => EF.Functions.ILike(rl.Url, $"%{url}%"));

            if (startDate.HasValue)
                query = query.Where(rl => rl.RequestTime >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(rl => rl.RequestTime <= endDate.Value);

            if (warehouseId.HasValue)
                query = query.Where(rl => rl.WarehouseId == warehouseId.Value);

            if (materialId.HasValue)
                query = query.Where(rl => rl.MaterialId == materialId.Value);

            if (productId.HasValue)
                query = query.Where(rl => rl.ProductId == productId.Value);

            if (minDurationMs.HasValue)
                query = query.Where(rl => rl.DurationMs >= minDurationMs.Value);

            if (maxDurationMs.HasValue)
                query = query.Where(rl => rl.DurationMs <= maxDurationMs.Value);

            return await query
                .OrderByDescending(rl => rl.RequestTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetLogsCountAsync(
            int? userId, string? controller, string? action, string? httpMethod,
            int? statusCode, string? url, DateTime? startDate, DateTime? endDate,
            int? warehouseId, int? materialId, int? productId,
            long? minDurationMs, long? maxDurationMs)
        {
            var query = _context.RequestLogs.AsQueryable();

            if (userId.HasValue)
                query = query.Where(rl => rl.UserId == userId.Value);

            if (!string.IsNullOrWhiteSpace(controller))
                query = query.Where(rl => rl.Controller != null && EF.Functions.ILike(rl.Controller, $"%{controller}%"));

            if (!string.IsNullOrWhiteSpace(action))
                query = query.Where(rl => rl.Action != null && EF.Functions.ILike(rl.Action, $"%{action}%"));

            if (!string.IsNullOrWhiteSpace(httpMethod))
                query = query.Where(rl => rl.HttpMethod == httpMethod);

            if (statusCode.HasValue)
                query = query.Where(rl => rl.StatusCode == statusCode.Value);

            if (!string.IsNullOrWhiteSpace(url))
                query = query.Where(rl => EF.Functions.ILike(rl.Url, $"%{url}%"));

            if (startDate.HasValue)
                query = query.Where(rl => rl.RequestTime >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(rl => rl.RequestTime <= endDate.Value);

            if (warehouseId.HasValue)
                query = query.Where(rl => rl.WarehouseId == warehouseId.Value);

            if (materialId.HasValue)
                query = query.Where(rl => rl.MaterialId == materialId.Value);

            if (productId.HasValue)
                query = query.Where(rl => rl.ProductId == productId.Value);

            if (minDurationMs.HasValue)
                query = query.Where(rl => rl.DurationMs >= minDurationMs.Value);

            if (maxDurationMs.HasValue)
                query = query.Where(rl => rl.DurationMs <= maxDurationMs.Value);

            return await query.CountAsync();
        }
    }
}

