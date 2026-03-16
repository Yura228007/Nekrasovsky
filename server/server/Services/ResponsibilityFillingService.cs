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

        var freeQuantity = filling.Quantity - totalResponsible;
        if (freeQuantity <= 0)
            throw new InvalidOperationException(
                $"Нельзя назначить ответственность: на складе {warehouseId} по материалу {materialId} нет свободного остатка (все {filling.Quantity} уже под ответственностью).");
        if (quantity > freeQuantity)
        {
            _logger.LogWarning("AssignMaterialAtWarehouse: requested {Requested}, capping to free quantity {Free} (warehouse {WarehouseId}, material {MaterialId})", quantity, freeQuantity, warehouseId, materialId);
            quantity = freeQuantity;
        }

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

        var freeQuantity = filling.Quantity - totalResponsible;
        if (freeQuantity <= 0)
            throw new InvalidOperationException(
                $"Нельзя назначить ответственность: на складе {warehouseId} по продукту {productId} нет свободного остатка (все {filling.Quantity} уже под ответственностью).");
        if (quantity > freeQuantity)
        {
            _logger.LogWarning("AssignProductAtWarehouse: requested {Requested}, capping to free quantity {Free} (warehouse {WarehouseId}, product {ProductId})", quantity, freeQuantity, warehouseId, productId);
            quantity = freeQuantity;
        }

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

    /// <summary>
    /// Уменьшить ответственность за материал на складе на quantity (по записям AssignedAt desc, снимая/уменьшая по пользователям).
    /// </summary>
    public async Task<bool> DecreaseMaterialResponsibilityAtWarehouseByQuantityAsync(int warehouseId, int materialId, int quantity)
    {
        if (quantity <= 0)
            return false;

        var fillings = await _context.ResponsibilityFillings
            .Where(rf => rf.IsActive && rf.WarehouseId == warehouseId && rf.MaterialId == materialId && rf.Quantity > 0)
            .OrderByDescending(rf => rf.AssignedAt)
            .ToListAsync();

        if (fillings.Count == 0)
            return true; // нет назначений — не ошибка

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
        _logger.LogInformation("Decreased ResponsibilityFilling for Material {MaterialId} at Warehouse {WarehouseId} by {Quantity}",
            materialId, warehouseId, quantity);
        return true;
    }

    /// <summary>
    /// Уменьшить ответственность за продукт на складе на quantity.
    /// </summary>
    public async Task<bool> DecreaseProductResponsibilityAtWarehouseByQuantityAsync(int warehouseId, int productId, int quantity)
    {
        if (quantity <= 0)
            return false;

        var fillings = await _context.ResponsibilityFillings
            .Where(rf => rf.IsActive && rf.WarehouseId == warehouseId && rf.ProductId == productId && rf.Quantity > 0)
            .OrderByDescending(rf => rf.AssignedAt)
            .ToListAsync();

        if (fillings.Count == 0)
            return true;

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
        _logger.LogInformation("Decreased ResponsibilityFilling for Product {ProductId} at Warehouse {WarehouseId} by {Quantity}",
            productId, warehouseId, quantity);
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
            .Include(rf => rf.Warehouse)
            .Include(rf => rf.Material)
            .Include(rf => rf.Product)
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
            .Include(rf => rf.Warehouse)
            .Where(rf => rf.IsActive && rf.MaterialId != null && rf.Quantity > 0)
            .ToListAsync();

        if (fillings.Count == 0)
            return new List<ResponsibilityAssignment>();

        var userIds = fillings.Select(rf => rf.UserId).Distinct().ToList();
        var userMap = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => $"{u.Surname} {u.Name}");

        return fillings.Select(rf => new ResponsibilityAssignment
        {
            ItemId = rf.MaterialId!.Value,
            UserId = rf.UserId,
            UserName = userMap.TryGetValue(rf.UserId, out var name) ? name : $"Пользователь #{rf.UserId}",
            Quantity = rf.Quantity,
            MeasuringUnit = rf.MeasuringUnit,
            WarehouseId = rf.WarehouseId,
            WarehouseName = rf.Warehouse?.Name
        }).ToList();
    }

    public async Task<List<ResponsibilityAssignment>> GetActiveProductAssignmentsFromFillingAsync()
    {
        var fillings = await _context.ResponsibilityFillings
            .Include(rf => rf.Warehouse)
            .Where(rf => rf.IsActive && rf.ProductId != null && rf.Quantity > 0)
            .ToListAsync();

        if (fillings.Count == 0)
            return new List<ResponsibilityAssignment>();

        var userIds = fillings.Select(rf => rf.UserId).Distinct().ToList();
        var userMap = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => $"{u.Surname} {u.Name}");

        return fillings.Select(rf => new ResponsibilityAssignment
        {
            ItemId = rf.ProductId!.Value,
            UserId = rf.UserId,
            UserName = userMap.TryGetValue(rf.UserId, out var name) ? name : $"Пользователь #{rf.UserId}",
            Quantity = rf.Quantity,
            MeasuringUnit = rf.MeasuringUnit,
            WarehouseId = rf.WarehouseId,
            WarehouseName = rf.Warehouse?.Name
        }).ToList();
    }

    public async Task TransferMaterialResponsibilityAsync(int warehouseId, int materialId, int fromUserId, int toUserId, int? quantityToTransfer = null)
    {
        if (fromUserId == toUserId)
            throw new InvalidOperationException("FromUserId and ToUserId must be different.");

        var totalFrom = await GetUserResponsibleQuantityAtWarehouseAsync(fromUserId, warehouseId, materialId);
        if (totalFrom <= 0)
            throw new InvalidOperationException($"User {fromUserId} has no responsibility for material {materialId} at warehouse {warehouseId}.");

        int transferAmount = quantityToTransfer.HasValue
            ? Math.Clamp(quantityToTransfer.Value, 1, totalFrom)
            : totalFrom;

        var decreased = await DecreaseMaterialResponsibilityAtWarehouseAsync(warehouseId, materialId, transferAmount, fromUserId);
        if (!decreased)
            throw new InvalidOperationException("Failed to decrease responsibility.");

        await AssignMaterialAtWarehouseAsync(toUserId, warehouseId, materialId, transferAmount);
        _logger.LogInformation("Transferred material responsibility: Warehouse {WarehouseId}, Material {MaterialId}, from User {FromUserId} to User {ToUserId}, Quantity {Quantity}",
            warehouseId, materialId, fromUserId, toUserId, transferAmount);
    }

    public async Task TransferProductResponsibilityAsync(int warehouseId, int productId, int fromUserId, int toUserId, int? quantityToTransfer = null)
    {
        if (fromUserId == toUserId)
            throw new InvalidOperationException("FromUserId and ToUserId must be different.");

        var totalFrom = await GetUserResponsibleProductQuantityAtWarehouseAsync(fromUserId, warehouseId, productId);
        if (totalFrom <= 0)
            throw new InvalidOperationException($"User {fromUserId} has no responsibility for product {productId} at warehouse {warehouseId}.");

        int transferAmount = quantityToTransfer.HasValue
            ? Math.Clamp(quantityToTransfer.Value, 1, totalFrom)
            : totalFrom;

        var decreased = await DecreaseProductResponsibilityAtWarehouseAsync(warehouseId, productId, transferAmount, fromUserId);
        if (!decreased)
            throw new InvalidOperationException("Failed to decrease responsibility.");

        await AssignProductAtWarehouseAsync(toUserId, warehouseId, productId, transferAmount);
        _logger.LogInformation("Transferred product responsibility: Warehouse {WarehouseId}, Product {ProductId}, from User {FromUserId} to User {ToUserId}, Quantity {Quantity}",
            warehouseId, productId, fromUserId, toUserId, transferAmount);
    }

    public async Task AssignBatchResponsibilityAsync(int userId, int batchId, int quantity, string? measuringUnit)
    {
        var batch = await _context.ProductBatches
            .Include(pb => pb.Product)
            .FirstOrDefaultAsync(pb => pb.Id == batchId);

        if (batch == null)
            throw new KeyNotFoundException($"ProductBatch with ID {batchId} not found");

        var responsibility = new ResponsibilityFilling
        {
            UserId = userId,
            WarehouseId = batch.WarehouseId,
            ProductId = batch.ProductId,
            ProductBatchId = batchId,
            Quantity = quantity,
            MeasuringUnit = measuringUnit ?? batch.MeasuringUnit ?? batch.Product?.MeasuringUnit,
            AssignedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.ResponsibilityFillings.Add(responsibility);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Batch responsibility assigned: User {UserId}, Batch {BatchId}, Quantity {Quantity}",
            userId, batchId, quantity);
    }

    public async Task DecreaseBatchResponsibilityAsync(int batchId, int quantity, int userId)
    {
        var responsibilities = await _context.ResponsibilityFillings
            .Where(rf => rf.ProductBatchId == batchId && rf.UserId == userId && rf.IsActive)
            .OrderByDescending(rf => rf.AssignedAt)
            .ToListAsync();

        var remaining = quantity;
        foreach (var rf in responsibilities)
        {
            if (remaining <= 0) break;

            if (rf.Quantity <= remaining)
            {
                remaining -= rf.Quantity;
                rf.Quantity = 0;
                rf.IsActive = false;
                rf.ReleasedAt = DateTime.UtcNow;
            }
            else
            {
                rf.Quantity -= remaining;
                remaining = 0;
            }
        }

        if (remaining > 0)
        {
            _logger.LogWarning("Could not fully decrease batch responsibility. Remaining: {Remaining}", remaining);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<int> GetUserResponsibleQuantityForBatchAsync(int userId, int batchId)
    {
        return await _context.ResponsibilityFillings
            .Where(rf => rf.ProductBatchId == batchId && rf.UserId == userId && rf.IsActive)
            .SumAsync(rf => rf.Quantity);
    }

    public async Task TransferBatchResponsibilityAsync(int batchId, int fromUserId, int toUserId, int? quantityToTransfer = null)
    {
        if (fromUserId == toUserId)
            throw new InvalidOperationException("FromUserId and ToUserId must be different.");

        var totalFrom = await GetUserResponsibleQuantityForBatchAsync(fromUserId, batchId);
        if (totalFrom <= 0)
            throw new InvalidOperationException($"User {fromUserId} has no responsibility for batch {batchId}.");

        int transferAmount = quantityToTransfer.HasValue
            ? Math.Clamp(quantityToTransfer.Value, 1, totalFrom)
            : totalFrom;

        await DecreaseBatchResponsibilityAsync(batchId, transferAmount, fromUserId);
        await AssignBatchResponsibilityAsync(toUserId, batchId, transferAmount, null);

        if (transferAmount >= totalFrom)
        {
            var batch = await _context.ProductBatches.FindAsync(batchId);
            if (batch != null && batch.CreatedByUserId == fromUserId)
            {
                batch.CreatedByUserId = toUserId;
                await _context.SaveChangesAsync();
            }
        }

        _logger.LogInformation("Transferred batch responsibility: Batch {BatchId}, from User {FromUserId} to User {ToUserId}, Quantity {Quantity}",
            batchId, fromUserId, toUserId, transferAmount);
    }

    public async Task ReleaseBatchResponsibilityAsync(int batchId, int userId)
    {
        var total = await GetUserResponsibleQuantityForBatchAsync(userId, batchId);
        if (total <= 0)
            return;

        await DecreaseBatchResponsibilityAsync(batchId, total, userId);

        var batch = await _context.ProductBatches.FindAsync(batchId);
        if (batch != null && batch.CreatedByUserId == userId)
        {
            batch.CreatedByUserId = null;
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("Released batch responsibility: Batch {BatchId}, User {UserId}, Quantity {Quantity}", batchId, userId, total);
    }

    public async Task<int> TransferAllResponsibilityFillingAsync(int fromUserId, int toUserId)
    {
        if (fromUserId == toUserId)
            return 0;

        var fillings = await _context.ResponsibilityFillings
            .Where(rf => rf.UserId == fromUserId && rf.IsActive && rf.Quantity > 0)
            .ToListAsync();

        if (fillings.Count == 0)
            return 0;

        int transferred = 0;

        // Группируем по (WarehouseId, MaterialId) и передаём материалы
        var materialGroups = fillings
            .Where(rf => rf.MaterialId.HasValue)
            .GroupBy(rf => (WarehouseId: rf.WarehouseId, MaterialId: rf.MaterialId!.Value))
            .Select(g => new { g.Key.WarehouseId, g.Key.MaterialId, Quantity = g.Sum(rf => rf.Quantity) })
            .ToList();
        foreach (var g in materialGroups)
        {
            try
            {
                await TransferMaterialResponsibilityAsync(g.WarehouseId, g.MaterialId, fromUserId, toUserId, g.Quantity);
                transferred++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "TransferAllResponsibilityFilling: skip material Warehouse {WarehouseId}, Material {MaterialId}", g.WarehouseId, g.MaterialId);
            }
        }

        // Группируем по (WarehouseId, ProductId) для продукта без партии
        var productGroups = fillings
            .Where(rf => rf.ProductId.HasValue && !rf.ProductBatchId.HasValue)
            .GroupBy(rf => (WarehouseId: rf.WarehouseId, ProductId: rf.ProductId!.Value))
            .Select(g => new { g.Key.WarehouseId, g.Key.ProductId, Quantity = g.Sum(rf => rf.Quantity) })
            .ToList();
        foreach (var g in productGroups)
        {
            try
            {
                await TransferProductResponsibilityAsync(g.WarehouseId, g.ProductId, fromUserId, toUserId, g.Quantity);
                transferred++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "TransferAllResponsibilityFilling: skip product Warehouse {WarehouseId}, Product {ProductId}", g.WarehouseId, g.ProductId);
            }
        }

        // Группируем по ProductBatchId и передаём партии
        var batchGroups = fillings
            .Where(rf => rf.ProductBatchId.HasValue)
            .GroupBy(rf => rf.ProductBatchId!.Value)
            .Select(g => new { BatchId = g.Key, Quantity = g.Sum(rf => rf.Quantity) })
            .ToList();
        foreach (var g in batchGroups)
        {
            try
            {
                await TransferBatchResponsibilityAsync(g.BatchId, fromUserId, toUserId, g.Quantity);
                transferred++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "TransferAllResponsibilityFilling: skip batch {BatchId}", g.BatchId);
            }
        }

        _logger.LogInformation("TransferAllResponsibilityFilling: from User {FromUserId} to User {ToUserId}, {Count} groups transferred", fromUserId, toUserId, transferred);
        return transferred;
    }

    /// <inheritdoc />
    public async Task<List<ResponsibilityStockItem>> GetResponsibilityStockForUserAsync(int userId)
    {
        var fillings = await _context.ResponsibilityFillings
            .Include(rf => rf.Warehouse)
            .Include(rf => rf.Material)
            .Include(rf => rf.Product)
            .Include(rf => rf.ProductBatch!)
            .ThenInclude(pb => pb!.Product)
            .Where(rf => rf.UserId == userId && rf.IsActive && rf.Quantity > 0)
            .ToListAsync();

        var result = new List<ResponsibilityStockItem>();
        var warehouses = await _context.Warehouses.ToDictionaryAsync(w => w.Id, w => w.Name);

        // Материалы: группируем по MaterialId
        var materialGroups = fillings
            .Where(rf => rf.MaterialId.HasValue)
            .GroupBy(rf => rf.MaterialId!.Value);
        foreach (var g in materialGroups)
        {
            var first = g.First();
            var material = first.Material ?? await _context.Materials.FindAsync(g.Key);
            if (material == null) continue;
            var warehouseStocks = g.GroupBy(rf => rf.WarehouseId).Select(wg => new ResponsibilityWarehouseStock
            {
                WarehouseId = wg.Key,
                WarehouseName = warehouses.TryGetValue(wg.Key, out var wn) ? wn : $"Склад #{wg.Key}",
                Quantity = wg.Sum(rf => rf.Quantity),
                MeasuringType = first.MeasuringUnit ?? material.MeasuringUnit ?? ""
            }).ToList();
            result.Add(new ResponsibilityStockItem
            {
                ItemType = "Material",
                ItemId = material.Id,
                Name = material.Name,
                MeasuringUnit = material.MeasuringUnit ?? "",
                TotalQuantity = warehouseStocks.Sum(ws => ws.Quantity),
                Warehouses = warehouseStocks
            });
        }

        // Продукты без партии: группируем по ProductId где ProductBatchId == null
        var productNoBatch = fillings
            .Where(rf => rf.ProductId.HasValue && !rf.ProductBatchId.HasValue)
            .GroupBy(rf => rf.ProductId!.Value);
        foreach (var g in productNoBatch)
        {
            var first = g.First();
            var product = first.Product ?? await _context.Products.FindAsync(g.Key);
            if (product == null) continue;
            var warehouseStocks = g.GroupBy(rf => rf.WarehouseId).Select(wg => new ResponsibilityWarehouseStock
            {
                WarehouseId = wg.Key,
                WarehouseName = warehouses.TryGetValue(wg.Key, out var wn) ? wn : $"Склад #{wg.Key}",
                Quantity = wg.Sum(rf => rf.Quantity),
                MeasuringType = first.MeasuringUnit ?? product.MeasuringUnit ?? ""
            }).ToList();
            result.Add(new ResponsibilityStockItem
            {
                ItemType = "Product",
                ItemId = product.Id,
                Name = product.Name,
                MeasuringUnit = product.MeasuringUnit ?? "",
                TotalQuantity = warehouseStocks.Sum(ws => ws.Quantity),
                Warehouses = warehouseStocks
            });
        }

        // Партии: группируем по ProductBatchId
        var batchGroups = fillings
            .Where(rf => rf.ProductBatchId.HasValue)
            .GroupBy(rf => rf.ProductBatchId!.Value);
        foreach (var g in batchGroups)
        {
            var first = g.First();
            var batch = first.ProductBatch ?? await _context.ProductBatches.Include(pb => pb.Product).FirstOrDefaultAsync(pb => pb.Id == g.Key);
            var product = batch?.Product;
            if (batch == null || product == null) continue;
            var warehouseStocks = g.GroupBy(rf => rf.WarehouseId).Select(wg => new ResponsibilityWarehouseStock
            {
                WarehouseId = wg.Key,
                WarehouseName = warehouses.TryGetValue(wg.Key, out var wn) ? wn : $"Склад #{wg.Key}",
                Quantity = wg.Sum(rf => rf.Quantity),
                MeasuringType = first.MeasuringUnit ?? product.MeasuringUnit ?? ""
            }).ToList();
            result.Add(new ResponsibilityStockItem
            {
                ItemType = "Product",
                ItemId = product.Id,
                Name = $"{product.Name} (партия #{batch.Id})",
                MeasuringUnit = product.MeasuringUnit ?? "",
                TotalQuantity = warehouseStocks.Sum(ws => ws.Quantity),
                Warehouses = warehouseStocks
            });
        }

        return result;
    }
}
