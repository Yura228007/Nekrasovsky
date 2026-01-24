using server.Models;

namespace server.Services
{
    public interface IRequestLogService
    {
        Task<IEnumerable<RequestLog>> GetAllRequestLogsAsync();
        Task<RequestLog?> GetRequestLogByIdAsync(int id);
        Task<IEnumerable<RequestLog>> GetRequestLogsByUserAsync(int userId);
        Task<IEnumerable<RequestLog>> GetRequestLogsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<RequestLog>> GetRequestLogsByStatusCodeAsync(int statusCode);
        Task<IEnumerable<RequestLog>> GetRequestLogsByControllerAsync(string controller);
        Task<IEnumerable<RequestLog>> GetRequestLogsByActionAsync(string controller, string action);
        Task<IEnumerable<RequestLog>> SearchRequestLogsAsync(string? url, string? httpMethod, int? statusCode, DateTime? startDate, DateTime? endDate);
        Task<int> DeleteOldRequestLogsAsync(DateTime beforeDate);

        Task<IEnumerable<RequestLog>> AdvancedSearchRequestLogsAsync(
            int? userId, string? controller, string? action, string? httpMethod,
            int? statusCode, string? url, DateTime? startDate, DateTime? endDate,
            int? warehouseId, int? materialId, int? productId,
            long? minDurationMs, long? maxDurationMs,
            int pageNumber = 1, int pageSize = 100);

        Task<int> GetLogsCountAsync(
            int? userId, string? controller, string? action, string? httpMethod,
            int? statusCode, string? url, DateTime? startDate, DateTime? endDate,
            int? warehouseId, int? materialId, int? productId,
            long? minDurationMs, long? maxDurationMs);
    }
}

