using server.Models;

namespace server.Services
{
    public interface IHistoryService
    {
        Task AddEventAsync(HistoryEvent historyEvent);
        Task<IEnumerable<HistoryEvent>> GetHistoryAsync(
            int? userId = null,
            int? relatedUserId = null,
            string? action = null,
            string? entityType = null,
            int? warehouseId = null,
            int? materialId = null,
            int? productId = null,
            DateTime? startDate = null,
            DateTime? endDate = null);
    }
}
