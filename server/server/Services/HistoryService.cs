using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;

namespace server.Services
{
    public class HistoryService : IHistoryService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<HistoryService> _logger;

        public HistoryService(AppDbContext context, ILogger<HistoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddEventAsync(HistoryEvent historyEvent)
        {
            _context.HistoryEvents.Add(historyEvent);
            await _context.SaveChangesAsync();
            _logger.LogInformation("History event recorded: {Action} (User {UserId})", historyEvent.Action, historyEvent.UserId);
        }

        public async Task<IEnumerable<HistoryEvent>> GetHistoryAsync(
            int? userId = null,
            int? relatedUserId = null,
            string? action = null,
            string? entityType = null,
            int? warehouseId = null,
            int? materialId = null,
            int? productId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = _context.HistoryEvents
                .Include(h => h.User)
                .Include(h => h.RelatedUser)
                .Include(h => h.Material)
                .Include(h => h.Product)
                .Include(h => h.Warehouse)
                .AsQueryable();

            if (userId.HasValue)
                query = query.Where(h => h.UserId == userId.Value);

            if (relatedUserId.HasValue)
                query = query.Where(h => h.RelatedUserId == relatedUserId.Value);

            if (!string.IsNullOrWhiteSpace(action))
                query = query.Where(h => h.Action == action);

            if (!string.IsNullOrWhiteSpace(entityType))
                query = query.Where(h => h.EntityType == entityType);

            if (warehouseId.HasValue)
                query = query.Where(h => h.WarehouseId == warehouseId.Value);

            if (materialId.HasValue)
                query = query.Where(h => h.MaterialId == materialId.Value);

            if (productId.HasValue)
                query = query.Where(h => h.ProductId == productId.Value);

            if (startDate.HasValue)
                query = query.Where(h => h.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(h => h.CreatedAt <= endDate.Value);

            return await query
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
        }
    }
}
