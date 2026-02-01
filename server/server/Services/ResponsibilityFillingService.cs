using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;

namespace server.Services;

public class ResponsibilityFillingService : IResponsibilityFillingService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ResponsibilityFillingService> _logger;

    public ResponsibilityFillingService(AppDbContext context, ILogger<ResponsibilityFillingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ResponsibilityFilling> AssignMaterialAtWarehouseAsync(int userId, int warehouseId, int materialId, int quantity, string? measuringUnit = null)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");

        var filling = await _context.FillingWarehouses
            .FirstOrDefaultAsync(fw => fw.WarehouseId == warehouseId && fw.MaterialId == materialId);
        if (filling == null)
            throw new InvalidOperationException($"No filling for material {materialId} at warehouse {warehouseId}. Add stock first.");

        var totalResponsible = await _context.ResponsibilityFillings
            .Where(rf => rf.WarehouseId == warehouseId && rf.MaterialId == materialId && rf.IsActive)
            .SumAsync(rf => rf.Quantity);

        if (totalResponsible + quantity > filling.Quantity)
            throw new InvalidOperationException(
                $"Cannot assign {quantity}: total responsible would be {totalResponsible + quantity}, but only {filling.Quantity} at warehouse {warehouseId}.");

        var existing = await _context.ResponsibilityFillings
            .Where(rf => rf.UserId == userId && rf.WarehouseId == warehouseId && rf.MaterialId == materialId && rf.IsActive)
            .OrderByDescending(rf => rf.AssignedAt)
            .FirstOrDefaultAsync();

        if (existing != null)
        {
            existing.Quantity += quantity;
            existing.MeasuringUnit = measuringUnit ?? existing.MeasuringUnit;
            await _context.SaveChangesAsync();
            _logger.LogInformation("ResponsibilityFilling updated: User {UserId}, Warehouse {WarehouseId}, Material {MaterialId}, added {Quantity}, total {Total}",
                userId, warehouseId, materialId, quantity, existing.Quantity);
            return existing;
        }

        var rf = new ResponsibilityFilling
        {
            UserId = userId,
            WarehouseId = warehouseId,
            MaterialId = materialId,
            Quantity = quantity,
            MeasuringUnit = measuringUnit ?? (await _context.Materials.FindAsync(materialId))?.MeasuringUnit,
            AssignedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.ResponsibilityFillings.Add(rf);
        await _context.SaveChangesAsync();
        _logger.LogInformation("ResponsibilityFilling created: User {UserId}, Warehouse {WarehouseId}, Material {MaterialId}, Quantity {Quantity}",
            userId, warehouseId, materialId, quantity);
        return rf;
    }

    public async Task<ResponsibilityFilling> AssignProductAtWarehouseAsync(int userId, int warehouseId, int productId, int quantity, string? measuringUnit = null)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");

        var filling = await _context.FillingWarehouses
            .FirstOrDefaultAsync(fw => fw.WarehouseId == warehouseId && fw.ProductId == productId);
        if (filling == null)
            throw new InvalidOperationException($"No filling for product {productId} at warehouse {warehouseId}. Add stock first.");

        var totalResponsible = await _context.ResponsibilityFillings
            .Where(rf => rf.WarehouseId == warehouseId && rf.ProductId == productId && rf.IsActive)
            .SumAsync(rf => rf.Quantity);

        if (totalResponsible + quantity > filling.Quantity)
            throw new InvalidOperationException(
                $"Cannot assign {quantity}: total responsible would be {totalResponsible + quantity}, but only {filling.Quantity} at warehouse {warehouseId}.");

        var existing = await _context.ResponsibilityFillings
            .Where(rf => rf.UserId == userId && rf.WarehouseId == warehouseId && rf.ProductId == productId && rf.IsActive)
            .OrderByDescending(rf => rf.AssignedAt)
            .FirstOrDefaultAsync();

        if (existing != null)
        {
            existing.Quantity += quantity;
            existing.MeasuringUnit = measuringUnit ?? existing.MeasuringUnit;
            await _context.SaveChangesAsync();
            _logger.LogInformation("ResponsibilityFilling updated: User {UserId}, Warehouse {WarehouseId}, Product {ProductId}, added {Quantity}, total {Total}",
                userId, warehouseId, productId, quantity, existing.Quantity);
            return existing;
        }

        var rf = new ResponsibilityFilling
        {
            UserId = userId,
            WarehouseId = warehouseId,
            ProductId = productId,
            Quantity = quantity,
            MeasuringUnit = measuringUnit ?? (await _context.Products.FindAsync(productId))?.MeasuringUnit,
            AssignedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.ResponsibilityFillings.Add(rf);
        await _context.SaveChangesAsync();
        _logger.LogInformation("ResponsibilityFilling created: User {UserId}, Warehouse {WarehouseId}, Product {ProductId}, Quantity {Quantity}",
            userId, warehouseId, productId, quantity);
        return rf;
    }

    public async Task<bool> DecreaseMaterialResponsibilityAtWarehouseAsync(int warehouseId, int materialId, int quantity, int userId)
    {
        if (quantity <= 0)
            return false;

        var fillings = await _context.ResponsibilityFillings
            .Where(rf => rf.IsActive && rf.WarehouseId == warehouseId && rf.MaterialId == materialId && rf.UserId == userId && rf.Quantity > 0)
            .OrderByDescending(rf => rf.AssignedAt)
            .ToListAsync();

        if (fillings.Count == 0)
            return false;

        int remaining = quantity;
        foreach (var rf in fillings)
        {
            if (remaining <= 0)
                break;
            int decrease = Math.Min(remaining, rf.Quantity);
            rf.Quantity -= decrease;
            remaining -= decrease;
            if (rf.Quantity <= 0)
            {
                rf.IsActive = false;
                rf.ReleasedAt = DateTime.UtcNow;
                rf.Quantity = 0;
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Decreased ResponsibilityFilling for Material {MaterialId} at Warehouse {WarehouseId}, User {UserId}, by {Quantity}",
            materialId, warehouseId, userId, quantity);
        return true;
    }

    public async Task<bool> DecreaseProductResponsibilityAtWarehouseAsync(int warehouseId, int productId, int quantity, int userId)
    {
        if (quantity <= 0)
            return false;

        var fillings = await _context.ResponsibilityFillings
            .Where(rf => rf.IsActive && rf.WarehouseId == warehouseId && rf.ProductId == productId && rf.UserId == userId && rf.Quantity > 0)
            .OrderByDescending(rf => rf.AssignedAt)
            .ToListAsync();

        if (fillings.Count == 0)
            return false;

        int remaining = quantity;
        foreach (var rf in fillings)
        {
            if (remaining <= 0)
                break;
            int decrease = Math.Min(remaining, rf.Quantity);
            rf.Quantity -= decrease;
            remaining -= decrease;
            if (rf.Quantity <= 0)
            {
                rf.IsActive = false;
                rf.ReleasedAt = DateTime.UtcNow;
                rf.Quantity = 0;
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Decreased ResponsibilityFilling for Product {ProductId} at Warehouse {WarehouseId}, User {UserId}, by {Quantity}",
            productId, warehouseId, userId, quantity);
        return true;
    }

    public async Task<int> GetUserResponsibleQuantityAtWarehouseAsync(int userId, int warehouseId, int materialId)
    {
        return await _context.ResponsibilityFillings
            .Where(rf => rf.IsActive && rf.UserId == userId && rf.WarehouseId == warehouseId && rf.MaterialId == materialId)
            .SumAsync(rf => rf.Quantity);
    }

    public async Task<int> GetUserResponsibleProductQuantityAtWarehouseAsync(int userId, int warehouseId, int productId)
    {
        return await _context.ResponsibilityFillings
            .Where(rf => rf.IsActive && rf.UserId == userId && rf.WarehouseId == warehouseId && rf.ProductId == productId)
            .SumAsync(rf => rf.Quantity);
    }

    public async Task<List<ResponsibilityFilling>> GetResponsibilityFillingsByUserAsync(int userId)
    {
        return await _context.ResponsibilityFillings
            .Where(rf => rf.UserId == userId && rf.IsActive)
            .OrderBy(rf => rf.WarehouseId)
            .ThenBy(rf => rf.MaterialId)
            .ThenBy(rf => rf.ProductId)
            .ToListAsync();
    }

    public async Task<List<ResponsibilityFilling>> GetByWarehouseAndMaterialAsync(int warehouseId, int materialId)
    {
        return await _context.ResponsibilityFillings
            .Where(rf => rf.IsActive && rf.WarehouseId == warehouseId && rf.MaterialId == materialId)
            .OrderByDescending(rf => rf.AssignedAt)
            .ToListAsync();
    }

    public async Task<List<ResponsibilityFilling>> GetByWarehouseAndProductAsync(int warehouseId, int productId)
    {
        return await _context.ResponsibilityFillings
            .Where(rf => rf.IsActive && rf.WarehouseId == warehouseId && rf.ProductId == productId)
            .OrderByDescending(rf => rf.AssignedAt)
            .ToListAsync();
    }

    public async Task<List<ResponsibilityAssignment>> GetActiveMaterialAssignmentsFromFillingAsync()
    {
        var fillings = await _context.ResponsibilityFillings
            .Where(rf => rf.IsActive && rf.MaterialId != null && rf.Quantity > 0)
            .ToListAsync();

        if (fillings.Count == 0)
            return new List<ResponsibilityAssignment>();

        var userIds = fillings.Select(rf => rf.UserId).Distinct().ToList();
        var userMap = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => $"{u.Surname} {u.Name}");

        return fillings
            .GroupBy(rf => new { MaterialId = rf.MaterialId!.Value, rf.UserId })
            .Select(g => new ResponsibilityAssignment
            {
                ItemId = g.Key.MaterialId,
                UserId = g.Key.UserId,
                UserName = userMap.TryGetValue(g.Key.UserId, out var name) ? name : $"Пользователь #{g.Key.UserId}",
                Quantity = g.Sum(rf => rf.Quantity),
                MeasuringUnit = g.First().MeasuringUnit
            })
            .ToList();
    }

    public async Task<List<ResponsibilityAssignment>> GetActiveProductAssignmentsFromFillingAsync()
    {
        var fillings = await _context.ResponsibilityFillings
            .Where(rf => rf.IsActive && rf.ProductId != null && rf.Quantity > 0)
            .ToListAsync();

        if (fillings.Count == 0)
            return new List<ResponsibilityAssignment>();

        var userIds = fillings.Select(rf => rf.UserId).Distinct().ToList();
        var userMap = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => $"{u.Surname} {u.Name}");

        return fillings
            .GroupBy(rf => new { ProductId = rf.ProductId!.Value, rf.UserId })
            .Select(g => new ResponsibilityAssignment
            {
                ItemId = g.Key.ProductId,
                UserId = g.Key.UserId,
                UserName = userMap.TryGetValue(g.Key.UserId, out var name) ? name : $"Пользователь #{g.Key.UserId}",
                Quantity = g.Sum(rf => rf.Quantity),
                MeasuringUnit = g.First().MeasuringUnit
            })
            .ToList();
    }
}
