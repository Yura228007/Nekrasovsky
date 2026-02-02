using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;

namespace server.Services;

public class ProductBatchService : IProductBatchService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProductBatchService> _logger;

    public ProductBatchService(AppDbContext context, ILogger<ProductBatchService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductBatch>> GetAllBatchesAsync()
    {
        return await _context.ProductBatches
            .Include(pb => pb.Product)
            .Include(pb => pb.Warehouse)
            .Include(pb => pb.CreatedByUser)
            .Where(pb => pb.IsActive && pb.Quantity > 0)
            .OrderByDescending(pb => pb.CreatedAt)
            .ToListAsync();
    }

    public async Task<ProductBatch?> GetBatchByIdAsync(int id)
    {
        return await _context.ProductBatches
            .Include(pb => pb.Product)
            .Include(pb => pb.Warehouse)
            .Include(pb => pb.CreatedByUser)
            .FirstOrDefaultAsync(pb => pb.Id == id);
    }

    public async Task<IEnumerable<ProductBatch>> GetBatchesByProductAsync(int productId)
    {
        return await _context.ProductBatches
            .Include(pb => pb.Warehouse)
            .Include(pb => pb.CreatedByUser)
            .Where(pb => pb.ProductId == productId && pb.IsActive && pb.Quantity > 0)
            .OrderByDescending(pb => pb.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductBatch>> GetBatchesByWarehouseAsync(int warehouseId)
    {
        return await _context.ProductBatches
            .Include(pb => pb.Product)
            .Include(pb => pb.CreatedByUser)
            .Where(pb => pb.WarehouseId == warehouseId && pb.IsActive && pb.Quantity > 0)
            .OrderByDescending(pb => pb.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductBatch>> GetBatchesByUserAsync(int userId)
    {
        return await _context.ProductBatches
            .Include(pb => pb.Product)
            .Include(pb => pb.Warehouse)
            .Where(pb => pb.CreatedByUserId == userId && pb.IsActive && pb.Quantity > 0)
            .OrderByDescending(pb => pb.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductBatch>> GetBatchesByProductAndWarehouseAsync(int productId, int warehouseId)
    {
        return await _context.ProductBatches
            .Include(pb => pb.CreatedByUser)
            .Where(pb => pb.ProductId == productId && pb.WarehouseId == warehouseId && pb.IsActive && pb.Quantity > 0)
            .OrderBy(pb => pb.CreatedAt) // FIFO
            .ToListAsync();
    }

    public async Task<ProductBatch> CreateBatchAsync(ProductBatch batch)
    {
        // Validate related entities
        if (!await _context.Products.AnyAsync(p => p.Id == batch.ProductId))
            throw new KeyNotFoundException($"Product with ID {batch.ProductId} not found");

        if (!await _context.Warehouses.AnyAsync(w => w.Id == batch.WarehouseId))
            throw new KeyNotFoundException($"Warehouse with ID {batch.WarehouseId} not found");

        if (batch.CreatedByUserId.HasValue && !await _context.Users.AnyAsync(u => u.Id == batch.CreatedByUserId.Value))
            throw new KeyNotFoundException($"User with ID {batch.CreatedByUserId} not found");

        batch.CreatedAt = DateTime.UtcNow;
        batch.IsActive = true;

        // Generate batch number if not provided
        if (string.IsNullOrWhiteSpace(batch.BatchNumber))
        {
            var product = await _context.Products.FindAsync(batch.ProductId);
            var code = product?.Code ?? "PROD";
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            batch.BatchNumber = $"{code}-{timestamp}";
        }

        _context.ProductBatches.Add(batch);
        await _context.SaveChangesAsync();

        // Update FillingWarehouse for aggregated view
        await UpdateFillingWarehouseAsync(batch.ProductId, batch.WarehouseId);

        _logger.LogInformation("ProductBatch created with ID: {BatchId}, Product: {ProductId}, Warehouse: {WarehouseId}, Quantity: {Quantity}",
            batch.Id, batch.ProductId, batch.WarehouseId, batch.Quantity);

        return batch;
    }

    public async Task<ProductBatch> UpdateBatchAsync(int id, ProductBatch updatedBatch)
    {
        var batch = await _context.ProductBatches.FindAsync(id);
        if (batch == null)
            throw new KeyNotFoundException($"ProductBatch with ID {id} not found");

        var oldQuantity = batch.Quantity;
        var oldWarehouseId = batch.WarehouseId;

        batch.Quantity = updatedBatch.Quantity;
        batch.Note = updatedBatch.Note;
        batch.IsActive = updatedBatch.IsActive;
        if (updatedBatch.CreatedByUserId.HasValue)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == updatedBatch.CreatedByUserId.Value))
                throw new KeyNotFoundException($"User with ID {updatedBatch.CreatedByUserId} not found");
            batch.CreatedByUserId = updatedBatch.CreatedByUserId;
        }
        else
        {
            batch.CreatedByUserId = null;
        }

        await _context.SaveChangesAsync();

        // Update FillingWarehouse if quantity or warehouse changed
        if (oldQuantity != batch.Quantity || oldWarehouseId != batch.WarehouseId)
        {
            await UpdateFillingWarehouseAsync(batch.ProductId, oldWarehouseId);
            if (oldWarehouseId != batch.WarehouseId)
            {
                await UpdateFillingWarehouseAsync(batch.ProductId, batch.WarehouseId);
            }
        }

        _logger.LogInformation("ProductBatch updated: {BatchId}", id);
        return batch;
    }

    public async Task<bool> DeleteBatchAsync(int id)
    {
        var batch = await _context.ProductBatches.FindAsync(id);
        if (batch == null)
            return false;

        // Снимаем ответственность по партии (ResponsibilityFilling с ProductBatchId)
        var responsibilityFillings = await _context.ResponsibilityFillings
            .Where(rf => rf.ProductBatchId == id)
            .ToListAsync();
        _context.ResponsibilityFillings.RemoveRange(responsibilityFillings);

        batch.IsActive = false;
        batch.Quantity = 0;
        await _context.SaveChangesAsync();

        // Пересчитываем остаток продукта на складе (FillingWarehouse)
        await UpdateFillingWarehouseAsync(batch.ProductId, batch.WarehouseId);

        _logger.LogInformation("ProductBatch deleted: {BatchId}. Removed {Rf} responsibility fillings.", id, responsibilityFillings.Count);
        return true;
    }

    public async Task<ProductBatch> DecreaseBatchQuantityAsync(int batchId, int quantity)
    {
        var batch = await _context.ProductBatches.FindAsync(batchId);
        if (batch == null)
            throw new KeyNotFoundException($"ProductBatch with ID {batchId} not found");

        if (batch.Quantity < quantity)
            throw new InvalidOperationException($"Insufficient quantity in batch. Available: {batch.Quantity}, Required: {quantity}");

        batch.Quantity -= quantity;
        if (batch.Quantity == 0)
        {
            batch.IsActive = false;
        }

        await _context.SaveChangesAsync();

        // Update FillingWarehouse
        await UpdateFillingWarehouseAsync(batch.ProductId, batch.WarehouseId);

        _logger.LogInformation("ProductBatch {BatchId} quantity decreased by {Quantity}. New quantity: {NewQuantity}",
            batchId, quantity, batch.Quantity);

        return batch;
    }

    public async Task<int> GetTotalQuantityByProductAndWarehouseAsync(int productId, int warehouseId)
    {
        return await _context.ProductBatches
            .Where(pb => pb.ProductId == productId && pb.WarehouseId == warehouseId && pb.IsActive)
            .SumAsync(pb => pb.Quantity);
    }

    /// <summary>
    /// Обновляет агрегированную запись FillingWarehouse для продукта на складе
    /// </summary>
    private async Task UpdateFillingWarehouseAsync(int productId, int warehouseId)
    {
        var totalQuantity = await GetTotalQuantityByProductAndWarehouseAsync(productId, warehouseId);

        var filling = await _context.FillingWarehouses
            .FirstOrDefaultAsync(fw => fw.ProductId == productId && fw.WarehouseId == warehouseId);

        if (filling == null && totalQuantity > 0)
        {
            // Create new filling record
            var product = await _context.Products.FindAsync(productId);
            filling = new FillingWarehouse
            {
                ProductId = productId,
                WarehouseId = warehouseId,
                Quantity = totalQuantity,
                MeasuringType = product?.MeasuringUnit
            };
            _context.FillingWarehouses.Add(filling);
        }
        else if (filling != null)
        {
            if (totalQuantity > 0)
            {
                filling.Quantity = totalQuantity;
            }
            else
            {
                // Remove filling if no quantity left
                _context.FillingWarehouses.Remove(filling);
            }
        }

        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task UpdateFillingWarehouseForProductAsync(int productId, int warehouseId)
    {
        await UpdateFillingWarehouseAsync(productId, warehouseId);
    }
}
