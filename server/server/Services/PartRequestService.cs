using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Data;

namespace server.Services
{
    public class PartRequestService : IPartRequestService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PartRequestService> _logger;
        private readonly IResponsibilityFillingService _responsibilityFillingService;
        // Хранилище для подсчета отказов: ключ - комбинация fromUserId-toUserId, значение - количество отказов
        private static readonly Dictionary<string, int> _rejectionCounts = new Dictionary<string, int>();

        public PartRequestService(
            AppDbContext context,
            ILogger<PartRequestService> logger,
            IResponsibilityFillingService responsibilityFillingService)
        {
            _context = context;
            _logger = logger;
            _responsibilityFillingService = responsibilityFillingService;
        }

        public async Task<IEnumerable<PartRequest>> GetAllPartRequestsAsync()
        {
            return await _context.PartRequests
                .Include(pr => pr.Material)
                .ToListAsync();
        }

        public async Task<PartRequest?> GetPartRequestByIdAsync(int id)
        {
            return await _context.PartRequests.FindAsync(id);
        }

        public async Task<IEnumerable<PartRequest>> GetPartRequestsByStatusAsync(PartRequestStatus status)
        {
            return await _context.PartRequests
                .Where(pr => pr.Status == status)
                .ToListAsync();
        }

        public async Task<IEnumerable<PartRequest>> GetPartRequestsByUserAsync(int userId, bool sent = true)
        {
            if (sent)
            {
                return await _context.PartRequests
                    .Include(pr => pr.Material)
                    .Where(pr => pr.FromUserId == userId)
                    .ToListAsync();
            }
            else
            {
                return await _context.PartRequests
                    .Include(pr => pr.Material)
                    .Where(pr => pr.ToUserId == userId)
                    .ToListAsync();
            }
        }

        public async Task<IEnumerable<PartRequest>> GetPartRequestsByWarehouseAsync(int warehouseId, bool from = true)
        {
            if (from)
            {
                return await _context.PartRequests
                    .Where(pr => pr.FromWarehouseId == warehouseId)
                    .ToListAsync();
            }
            else
            {
                return await _context.PartRequests
                    .Where(pr => pr.ToWarehouseId == warehouseId)
                    .ToListAsync();
            }
        }

        public async Task<IEnumerable<PartRequest>> GetPartRequestsByMaterialAsync(int materialId)
        {
            return await _context.PartRequests
                .Where(pr => pr.MaterialId == materialId)
                .ToListAsync();
        }

        public async Task<PartRequest> CreatePartRequestAsync(PartRequest request)
        {
            // Validate related entities exist
            if (!await _context.Users.AnyAsync(u => u.Id == request.FromUserId))
                throw new KeyNotFoundException($"FromUser with ID {request.FromUserId} not found");

            if (!await _context.Users.AnyAsync(u => u.Id == request.ToUserId))
                throw new KeyNotFoundException($"ToUser with ID {request.ToUserId} not found");

            if (!await _context.Warehouses.AnyAsync(w => w.Id == request.FromWarehouseId))
                throw new KeyNotFoundException($"FromWarehouse with ID {request.FromWarehouseId} not found");

            if (!await _context.Warehouses.AnyAsync(w => w.Id == request.ToWarehouseId))
                throw new KeyNotFoundException($"ToWarehouse with ID {request.ToWarehouseId} not found");

            if (!await _context.Materials.AnyAsync(m => m.Id == request.MaterialId))
                throw new KeyNotFoundException($"Material with ID {request.MaterialId} not found");

            request.CreatedAt = DateTime.UtcNow;
            request.Status = PartRequestStatus.Pending;

            _context.PartRequests.Add(request);
            await _context.SaveChangesAsync();

            _logger.LogInformation("PartRequest created with ID: {RequestId}", request.Id);
            return request;
        }

        public async Task<PartRequest> UpdatePartRequestAsync(int id, PartRequest updatedRequest)
        {
            var request = await _context.PartRequests.FindAsync(id);
            if (request == null)
            {
                throw new KeyNotFoundException($"PartRequest with ID {id} not found");
            }

            request.Quantity = updatedRequest.Quantity;
            request.MeasuringType = updatedRequest.MeasuringType;
            // Status can be updated via Approve/Reject methods

            await _context.SaveChangesAsync();

            _logger.LogInformation("PartRequest updated with ID: {RequestId}", request.Id);
            return request;
        }

        public async Task<PartRequest> ApprovePartRequestAsync(int id)
        {
            var request = await _context.PartRequests
                .Include(pr => pr.Material)
                .Include(pr => pr.Product)
                .FirstOrDefaultAsync(pr => pr.Id == id);
            
            if (request == null)
            {
                throw new KeyNotFoundException($"PartRequest with ID {id} not found");
            }

            if (request.Status != PartRequestStatus.Pending)
            {
                throw new InvalidOperationException($"PartRequest {id} is already {request.Status}");
            }

            // Проверяем, что заполнен только один из MaterialId или ProductId
            if (request.MaterialId.HasValue == request.ProductId.HasValue)
            {
                throw new InvalidOperationException("PartRequest must have either MaterialId or ProductId, but not both");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (request.MaterialId.HasValue)
                {
                    // Обработка материала
                    var materialId = request.MaterialId.Value;
                    var fromFilling = await _context.FillingWarehouses
                        .FirstOrDefaultAsync(fw => fw.WarehouseId == request.FromWarehouseId && 
                                                   fw.MaterialId == materialId);

                    if (fromFilling == null || fromFilling.Quantity < request.Quantity)
                    {
                        throw new InvalidOperationException($"Insufficient material quantity in source warehouse. Available: {fromFilling?.Quantity ?? 0}, Required: {request.Quantity}");
                    }

                    // Уменьшаем количество на исходном складе
                    fromFilling.Quantity -= request.Quantity;

                    // Увеличиваем количество на целевом складе
                    var toFilling = await _context.FillingWarehouses
                        .FirstOrDefaultAsync(fw => fw.WarehouseId == request.ToWarehouseId && 
                                                  fw.MaterialId == materialId);

                    if (toFilling == null)
                    {
                        toFilling = new FillingWarehouse
                        {
                            WarehouseId = request.ToWarehouseId,
                            MaterialId = materialId,
                            Quantity = 0,
                            MeasuringType = request.MeasuringType ?? request.Material?.MeasuringUnit
                        };
                        _context.FillingWarehouses.Add(toFilling);
                    }

                    toFilling.Quantity += request.Quantity;
                    if (!string.IsNullOrWhiteSpace(request.MeasuringType))
                    {
                        toFilling.MeasuringType = request.MeasuringType;
                    }

                    await _context.SaveChangesAsync();

                    // Ответственность: привязка к складу (ResponsibilityFilling)
                    var senderResponsibleAtWarehouse = await _responsibilityFillingService.GetUserResponsibleQuantityAtWarehouseAsync(
                        request.FromUserId, request.FromWarehouseId, materialId);

                    if (senderResponsibleAtWarehouse > 0)
                    {
                        var decreaseAmount = Math.Min(senderResponsibleAtWarehouse, request.Quantity);
                        await _responsibilityFillingService.DecreaseMaterialResponsibilityAtWarehouseAsync(
                            request.FromWarehouseId, materialId, decreaseAmount, request.FromUserId);
                    }

                    // Назначаем ответственность получателю
                    await _responsibilityFillingService.AssignMaterialAtWarehouseAsync(
                        request.ToUserId, request.ToWarehouseId, materialId, request.Quantity,
                        request.MeasuringType ?? request.Material?.MeasuringUnit);
                }
                else if (request.ProductId.HasValue)
                {
                    // Обработка продукта
                    var productId = request.ProductId.Value;
                    var fromFilling = await _context.FillingWarehouses
                        .FirstOrDefaultAsync(fw => fw.WarehouseId == request.FromWarehouseId && 
                                                   fw.ProductId == productId);

                    if (fromFilling == null || fromFilling.Quantity < request.Quantity)
                    {
                        throw new InvalidOperationException($"Insufficient product quantity in source warehouse. Available: {fromFilling?.Quantity ?? 0}, Required: {request.Quantity}");
                    }

                    // Уменьшаем количество на исходном складе
                    fromFilling.Quantity -= request.Quantity;

                    // Увеличиваем количество на целевом складе
                    var toFilling = await _context.FillingWarehouses
                        .FirstOrDefaultAsync(fw => fw.WarehouseId == request.ToWarehouseId && 
                                                  fw.ProductId == productId);

                    if (toFilling == null)
                    {
                        toFilling = new FillingWarehouse
                        {
                            WarehouseId = request.ToWarehouseId,
                            ProductId = productId,
                            Quantity = 0,
                            MeasuringType = request.MeasuringType ?? request.Product?.MeasuringUnit
                        };
                        _context.FillingWarehouses.Add(toFilling);
                    }

                    toFilling.Quantity += request.Quantity;
                    if (!string.IsNullOrWhiteSpace(request.MeasuringType))
                    {
                        toFilling.MeasuringType = request.MeasuringType;
                    }

                    await _context.SaveChangesAsync();

                    // Ответственность: привязка к складу (ResponsibilityFilling)
                    var senderResponsibleAtWarehouse = await _responsibilityFillingService.GetUserResponsibleProductQuantityAtWarehouseAsync(
                        request.FromUserId, request.FromWarehouseId, productId);

                    if (senderResponsibleAtWarehouse > 0)
                    {
                        var decreaseAmount = Math.Min(senderResponsibleAtWarehouse, request.Quantity);
                        await _responsibilityFillingService.DecreaseProductResponsibilityAtWarehouseAsync(
                            request.FromWarehouseId, productId, decreaseAmount, request.FromUserId);
                    }

                    // Назначаем ответственность получателю
                    await _responsibilityFillingService.AssignProductAtWarehouseAsync(
                        request.ToUserId, request.ToWarehouseId, productId, request.Quantity,
                        request.MeasuringType ?? request.Product?.MeasuringUnit);
                }

                request.Status = PartRequestStatus.Approved;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Сбрасываем счетчик отказов при одобрении запроса
                var rejectionKey = $"{request.FromUserId}-{request.ToUserId}";
                lock (_rejectionCounts)
                {
                    if (_rejectionCounts.ContainsKey(rejectionKey))
                    {
                        _rejectionCounts[rejectionKey] = 0;
                    }
                }

                var itemType = request.MaterialId.HasValue ? "Material" : "Product";
                var itemId = request.MaterialId ?? request.ProductId ?? 0;
                _logger.LogInformation("PartRequest {RequestId} approved. {ItemType} {ItemId} moved from warehouse {FromWarehouseId} to {ToWarehouseId}, quantity {Quantity}", 
                    id, itemType, itemId, request.FromWarehouseId, request.ToWarehouseId, request.Quantity);
                return request;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<PartRequest> RejectPartRequestAsync(int id, string? reason)
        {
            var request = await _context.PartRequests.FindAsync(id);
            if (request == null)
            {
                throw new KeyNotFoundException($"PartRequest with ID {id} not found");
            }

            request.Status = PartRequestStatus.Rejected;
            await _context.SaveChangesAsync();

            // Подсчет отказов: увеличиваем счетчик для комбинации fromUserId-toUserId
            var rejectionKey = $"{request.FromUserId}-{request.ToUserId}";
            lock (_rejectionCounts)
            {
                if (!_rejectionCounts.ContainsKey(rejectionKey))
                {
                    _rejectionCounts[rejectionKey] = 0;
                }
                _rejectionCounts[rejectionKey]++;
            }

            var rejectionCount = await GetRejectionCountAsync(request.FromUserId, request.ToUserId);

            _logger.LogInformation(
                "PartRequest {RequestId} rejected. Reason: {Reason}. Rejection count for user {FromUserId} from user {ToUserId}: {RejectionCount}", 
                id, reason ?? "No reason provided", request.FromUserId, request.ToUserId, rejectionCount);

            return request;
        }

        /// <summary>
        /// Получает количество отказов для комбинации пользователей
        /// </summary>
        public async Task<int> GetRejectionCountAsync(int fromUserId, int toUserId)
        {
            // Сначала проверяем in-memory хранилище
            var rejectionKey = $"{fromUserId}-{toUserId}";
            int count;
            lock (_rejectionCounts)
            {
                _rejectionCounts.TryGetValue(rejectionKey, out count);
            }

            // Также считаем отказы из БД за последние 24 часа для более точного подсчета
            var last24Hours = DateTime.UtcNow.AddHours(-24);
            var dbRejectionCount = await _context.PartRequests
                .CountAsync(pr => pr.FromUserId == fromUserId && 
                                  pr.ToUserId == toUserId && 
                                  pr.Status == PartRequestStatus.Rejected &&
                                  pr.CreatedAt >= last24Hours);

            // Возвращаем максимальное значение (для случая, если in-memory счетчик был сброшен)
            return Math.Max(count, dbRejectionCount);
        }

        public async Task<bool> DeletePartRequestAsync(int id)
        {
            var request = await _context.PartRequests.FindAsync(id);
            if (request == null)
            {
                return false;
            }

            _context.PartRequests.Remove(request);
            await _context.SaveChangesAsync();

            _logger.LogInformation("PartRequest deleted with ID: {RequestId}", id);
            return true;
        }
    }
}

