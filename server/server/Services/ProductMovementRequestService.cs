using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;

namespace server.Services;

public class ProductMovementRequestService : IProductMovementRequestService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProductMovementRequestService> _logger;
    private readonly IProductBatchService _batchService;
    private readonly IResponsibilityFillingService _responsibilityFillingService;

    public ProductMovementRequestService(
        AppDbContext context,
        ILogger<ProductMovementRequestService> logger,
        IProductBatchService batchService,
        IResponsibilityFillingService responsibilityFillingService)
    {
        _context = context;
        _logger = logger;
        _batchService = batchService;
        _responsibilityFillingService = responsibilityFillingService;
    }

    public async Task<IEnumerable<ProductMovementRequest>> GetAllRequestsAsync()
    {
        return await _context.ProductMovementRequests
            .Include(r => r.ProductBatch)
                .ThenInclude(pb => pb!.Product)
            .Include(r => r.FromWarehouse)
            .Include(r => r.ToWarehouse)
            .Include(r => r.FromUser)
            .Include(r => r.ToUser)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<ProductMovementRequest?> GetRequestByIdAsync(int id)
    {
        return await _context.ProductMovementRequests
            .Include(r => r.ProductBatch)
                .ThenInclude(pb => pb!.Product)
            .Include(r => r.FromWarehouse)
            .Include(r => r.ToWarehouse)
            .Include(r => r.FromUser)
            .Include(r => r.ToUser)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<ProductMovementRequest>> GetRequestsByStatusAsync(ProductMovementStatus status)
    {
        return await _context.ProductMovementRequests
            .Include(r => r.ProductBatch)
                .ThenInclude(pb => pb!.Product)
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductMovementRequest>> GetRequestsByUserAsync(int userId, bool sent = true)
    {
        if (sent)
        {
            return await _context.ProductMovementRequests
                .Include(r => r.ProductBatch)
                    .ThenInclude(pb => pb!.Product)
                .Where(r => r.FromUserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
        else
        {
            return await _context.ProductMovementRequests
                .Include(r => r.ProductBatch)
                    .ThenInclude(pb => pb!.Product)
                .Where(r => r.ToUserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
    }

    public async Task<IEnumerable<ProductMovementRequest>> GetRequestsByWarehouseAsync(int warehouseId, bool from = true)
    {
        if (from)
        {
            return await _context.ProductMovementRequests
                .Include(r => r.ProductBatch)
                    .ThenInclude(pb => pb!.Product)
                .Where(r => r.FromWarehouseId == warehouseId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
        else
        {
            return await _context.ProductMovementRequests
                .Include(r => r.ProductBatch)
                    .ThenInclude(pb => pb!.Product)
                .Where(r => r.ToWarehouseId == warehouseId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
    }

    public async Task<IEnumerable<ProductMovementRequest>> GetRequestsByBatchAsync(int batchId)
    {
        return await _context.ProductMovementRequests
            .Where(r => r.ProductBatchId == batchId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<ProductMovementRequest> CreateRequestAsync(ProductMovementRequest request)
    {
        // Validate related entities
        if (!await _context.Users.AnyAsync(u => u.Id == request.FromUserId))
            throw new KeyNotFoundException($"FromUser with ID {request.FromUserId} not found");

        if (!await _context.Users.AnyAsync(u => u.Id == request.ToUserId))
            throw new KeyNotFoundException($"ToUser with ID {request.ToUserId} not found");

        if (!await _context.Warehouses.AnyAsync(w => w.Id == request.FromWarehouseId))
            throw new KeyNotFoundException($"FromWarehouse with ID {request.FromWarehouseId} not found");

        if (!await _context.Warehouses.AnyAsync(w => w.Id == request.ToWarehouseId))
            throw new KeyNotFoundException($"ToWarehouse with ID {request.ToWarehouseId} not found");

        var batch = await _context.ProductBatches.FindAsync(request.ProductBatchId);
        if (batch == null)
            throw new KeyNotFoundException($"ProductBatch with ID {request.ProductBatchId} not found");

        if (batch.WarehouseId != request.FromWarehouseId)
            throw new InvalidOperationException($"Batch is not located at FromWarehouse");

        if (batch.Quantity < request.Quantity)
            throw new InvalidOperationException($"Insufficient quantity in batch. Available: {batch.Quantity}, Required: {request.Quantity}");

        request.CreatedAt = DateTime.UtcNow;
        request.Status = ProductMovementStatus.Pending;

        _context.ProductMovementRequests.Add(request);
        await _context.SaveChangesAsync();

        _logger.LogInformation("ProductMovementRequest created with ID: {RequestId}", request.Id);
        return request;
    }

    public async Task<ProductMovementRequest> UpdateRequestAsync(int id, ProductMovementRequest updatedRequest)
    {
        var request = await _context.ProductMovementRequests.FindAsync(id);
        if (request == null)
            throw new KeyNotFoundException($"ProductMovementRequest with ID {id} not found");

        request.Quantity = updatedRequest.Quantity;
        request.MeasuringType = updatedRequest.MeasuringType;
        request.Note = updatedRequest.Note;

        await _context.SaveChangesAsync();

        _logger.LogInformation("ProductMovementRequest updated with ID: {RequestId}", request.Id);
        return request;
    }

    public async Task<ProductMovementRequest> ApproveRequestAsync(int id)
    {
        var request = await _context.ProductMovementRequests
            .Include(r => r.ProductBatch)
                .ThenInclude(pb => pb!.Product)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
            throw new KeyNotFoundException($"ProductMovementRequest with ID {id} not found");

        if (request.Status != ProductMovementStatus.Pending)
            throw new InvalidOperationException($"ProductMovementRequest {id} is already {request.Status}");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var batch = request.ProductBatch!;

            // Decrease quantity from source batch
            await _batchService.DecreaseBatchQuantityAsync(request.ProductBatchId, request.Quantity);

            // Create new batch at destination warehouse
            var newBatch = new ProductBatch
            {
                ProductId = batch.ProductId,
                WarehouseId = request.ToWarehouseId,
                Quantity = request.Quantity,
                MeasuringUnit = request.MeasuringType ?? batch.MeasuringUnit,
                CreatedByUserId = request.ToUserId,
                CreatedAt = DateTime.UtcNow,
                Note = $"Перемещено из склада {request.FromWarehouseId}, партия {batch.BatchNumber}",
                IsActive = true
            };

            await _batchService.CreateBatchAsync(newBatch);

            // Transfer responsibility
            var senderResponsibleQuantity = await _responsibilityFillingService.GetUserResponsibleQuantityForBatchAsync(
                request.FromUserId, request.ProductBatchId);

            if (senderResponsibleQuantity >= request.Quantity)
            {
                // Decrease sender's responsibility for this batch
                await _responsibilityFillingService.DecreaseBatchResponsibilityAsync(
                    request.ProductBatchId, request.Quantity, request.FromUserId);

                // Assign responsibility to receiver for new batch
                await _responsibilityFillingService.AssignBatchResponsibilityAsync(
                    request.ToUserId, newBatch.Id, request.Quantity,
                    request.MeasuringType ?? batch.MeasuringUnit);
            }

            request.Status = ProductMovementStatus.Approved;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("ProductMovementRequest {RequestId} approved. Batch {BatchId} moved from warehouse {FromWarehouseId} to {ToWarehouseId}, quantity {Quantity}",
                id, request.ProductBatchId, request.FromWarehouseId, request.ToWarehouseId, request.Quantity);

            return request;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ProductMovementRequest> RejectRequestAsync(int id, string? reason)
    {
        var request = await _context.ProductMovementRequests.FindAsync(id);
        if (request == null)
            throw new KeyNotFoundException($"ProductMovementRequest with ID {id} not found");

        request.Status = ProductMovementStatus.Rejected;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            request.Note = $"Отклонено: {reason}";
        }
        await _context.SaveChangesAsync();

        _logger.LogInformation("ProductMovementRequest {RequestId} rejected. Reason: {Reason}",
            id, reason ?? "No reason provided");

        return request;
    }

    public async Task<bool> DeleteRequestAsync(int id)
    {
        var request = await _context.ProductMovementRequests.FindAsync(id);
        if (request == null)
            return false;

        _context.ProductMovementRequests.Remove(request);
        await _context.SaveChangesAsync();

        _logger.LogInformation("ProductMovementRequest deleted with ID: {RequestId}", id);
        return true;
    }
}
