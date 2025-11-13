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
    }
}

