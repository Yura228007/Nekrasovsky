using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;

namespace server.Services;

public class ResponsibilityShiftSnapshotService : IResponsibilityShiftSnapshotService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ResponsibilityShiftSnapshotService> _logger;

    public ResponsibilityShiftSnapshotService(AppDbContext context, ILogger<ResponsibilityShiftSnapshotService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ResponsibilityShiftSnapshot> CreateSnapshotAsync(int workReportId, int userId)
    {
        var fillings = await _context.ResponsibilityFillings
            .Where(rf => rf.UserId == userId && rf.IsActive)
            .ToListAsync();

        var aggregated = fillings
            .GroupBy(rf => new { rf.WarehouseId, rf.MaterialId, rf.ProductId })
            .Select(g => new
            {
                g.Key.WarehouseId,
                g.Key.MaterialId,
                g.Key.ProductId,
                Quantity = g.Sum(rf => rf.Quantity),
                MeasuringUnit = g.First().MeasuringUnit
            })
            .Where(x => x.Quantity > 0)
            .ToList();

        var snapshot = new ResponsibilityShiftSnapshot
        {
            WorkReportId = workReportId,
            UserId = userId,
            SnapshotAt = DateTime.UtcNow
        };
        _context.ResponsibilityShiftSnapshots.Add(snapshot);
        await _context.SaveChangesAsync();

        foreach (var a in aggregated)
        {
            var item = new ResponsibilityShiftSnapshotItem
            {
                ResponsibilityShiftSnapshotId = snapshot.Id,
                WarehouseId = a.WarehouseId,
                MaterialId = a.MaterialId,
                ProductId = a.ProductId,
                Quantity = a.Quantity,
                MeasuringUnit = a.MeasuringUnit
            };
            _context.ResponsibilityShiftSnapshotItems.Add(item);
        }
        await _context.SaveChangesAsync();

        _logger.LogInformation("Responsibility snapshot created for WorkReport {WorkReportId}, User {UserId}, items {Count}",
            workReportId, userId, aggregated.Count);
        return snapshot;
    }

    public async Task<ResponsibilityShiftSnapshot?> GetByWorkReportIdAsync(int workReportId)
    {
        return await _context.ResponsibilityShiftSnapshots
            .Include(s => s.Items)
                .ThenInclude(i => i.Warehouse)
            .Include(s => s.Items)
                .ThenInclude(i => i.Material)
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(s => s.WorkReportId == workReportId);
    }
}
