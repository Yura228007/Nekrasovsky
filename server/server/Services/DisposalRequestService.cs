using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;

namespace server.Services;

public class DisposalRequestService : IDisposalRequestService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DisposalRequestService> _logger;
    private readonly IResponsibilityFillingService _responsibilityFillingService;
    private readonly IFillingWarehouseService _fillingWarehouseService;

    public DisposalRequestService(
        AppDbContext context,
        ILogger<DisposalRequestService> logger,
        IResponsibilityFillingService responsibilityFillingService,
        IFillingWarehouseService fillingWarehouseService)
    {
        _context = context;
        _logger = logger;
        _responsibilityFillingService = responsibilityFillingService;
        _fillingWarehouseService = fillingWarehouseService;
    }

    public async Task<DisposalRequest> CreateDisposalRequestAsync(int fromUserId, int fromWarehouseId, int toWarehouseId,
        int materialId, double quantity, string? measuringUnit, DisposalRequestType requestType)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");

        // Проверяем, что материал есть на исходном складе под ответственностью пользователя
        var userResponsibleQty = await _responsibilityFillingService.GetUserResponsibleQuantityAtWarehouseAsync(
            fromUserId, fromWarehouseId, materialId);
        
        if (userResponsibleQty < quantity)
        {
            throw new InvalidOperationException(
                $"Недостаточно ответственности за материал (ID {materialId}): требуется {quantity}, под ответственностью {userResponsibleQty}.");
        }

        var request = new DisposalRequest
        {
            FromUserId = fromUserId,
            FromWarehouseId = fromWarehouseId,
            ToWarehouseId = toWarehouseId,
            MaterialId = materialId,
            Quantity = quantity,
            MeasuringUnit = measuringUnit,
            RequestType = requestType,
            Status = DisposalRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.DisposalRequests.Add(request);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "DisposalRequest created: FromUser {FromUserId}, FromWarehouse {FromWarehouseId}, ToWarehouse {ToWarehouseId}, Material {MaterialId}, Quantity {Quantity}, Type {RequestType}",
            fromUserId, fromWarehouseId, toWarehouseId, materialId, quantity, requestType);

        return request;
    }

    public async Task<DisposalRequest> CreateDisposalRequestForProductAsync(int fromUserId, int fromWarehouseId, int toWarehouseId,
        int productId, double quantity, string? measuringUnit, DisposalRequestType requestType)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");

        // Проверяем, что продукт есть на исходном складе под ответственностью пользователя
        var userResponsibleQty = await _responsibilityFillingService.GetUserResponsibleProductQuantityAtWarehouseAsync(
            fromUserId, fromWarehouseId, productId);
        
        if (userResponsibleQty < quantity)
        {
            throw new InvalidOperationException(
                $"Недостаточно ответственности за продукт (ID {productId}): требуется {quantity}, под ответственностью {userResponsibleQty}.");
        }

        // Проверяем, что продукт физически есть на исходном складе
        var fromFilling = await _context.FillingWarehouses
            .FirstOrDefaultAsync(fw => fw.WarehouseId == fromWarehouseId && fw.ProductId == productId);

        if (fromFilling == null || fromFilling.Quantity < quantity)
        {
            throw new InvalidOperationException(
                $"Недостаточно продукции на складе {fromWarehouseId}: требуется {quantity}, доступно {fromFilling?.Quantity ?? 0}.");
        }

        // Перемещаем продукцию на склад утиля физически
        fromFilling.Quantity -= quantity;

        var toFilling = await _context.FillingWarehouses
            .FirstOrDefaultAsync(fw => fw.WarehouseId == toWarehouseId && fw.ProductId == productId);

        if (toFilling == null)
        {
            var product = await _context.Products.FindAsync(productId);
            toFilling = new FillingWarehouse
            {
                WarehouseId = toWarehouseId,
                ProductId = productId,
                Quantity = quantity,
                MeasuringType = measuringUnit ?? product?.MeasuringUnit ?? "шт"
            };
            _context.FillingWarehouses.Add(toFilling);
        }
        else
        {
            toFilling.Quantity += quantity;
        }

        // Списываем ответственность с текущего склада
        await _responsibilityFillingService.DecreaseProductResponsibilityAtWarehouseAsync(
            fromWarehouseId, productId, quantity, fromUserId);

        // Добавляем ответственность на склад утиля (отдельной строкой)
        // Ответственность остается у того же пользователя (fromUserId), но теперь на складе утиля
        await _responsibilityFillingService.AssignProductAtWarehouseAsync(
            fromUserId, toWarehouseId, productId, quantity, measuringUnit);

        // Создаем запрос (ответственность уже перемещена на склад утиля, но остается у создателя до одобрения)
        var request = new DisposalRequest
        {
            FromUserId = fromUserId,
            FromWarehouseId = fromWarehouseId,
            ToWarehouseId = toWarehouseId,
            ProductId = productId,
            Quantity = quantity,
            MeasuringUnit = measuringUnit,
            RequestType = requestType,
            Status = DisposalRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.DisposalRequests.Add(request);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "DisposalRequest created for product: FromUser {FromUserId}, FromWarehouse {FromWarehouseId}, ToWarehouse {ToWarehouseId}, Product {ProductId}, Quantity {Quantity}, Type {RequestType}",
            fromUserId, fromWarehouseId, toWarehouseId, productId, quantity, requestType);

        return request;
    }

    public async Task<List<DisposalRequest>> GetPendingDisposalRequestsAsync(int disposalWarehouseId)
    {
        return await _context.DisposalRequests
            .Include(dr => dr.FromUser)
            .Include(dr => dr.Material)
            .Include(dr => dr.Product)
            .Include(dr => dr.FromWarehouse)
            .Where(dr => dr.ToWarehouseId == disposalWarehouseId && dr.Status == DisposalRequestStatus.Pending)
            .OrderByDescending(dr => dr.CreatedAt)
            .ToListAsync();
    }

        public async Task<DisposalRequest> ApproveDisposalRequestAsync(int requestId, int approvedByUserId)
        {
            var request = await _context.DisposalRequests
                .Include(dr => dr.Material)
                .Include(dr => dr.Product)
                .FirstOrDefaultAsync(dr => dr.Id == requestId);

            if (request == null)
                throw new KeyNotFoundException($"DisposalRequest with ID {requestId} not found");

            if (request.Status != DisposalRequestStatus.Pending)
                throw new InvalidOperationException($"DisposalRequest {requestId} is not pending (current status: {request.Status})");

            // Материал/продукт уже перемещен на склад утиля при создании запроса
            // Теперь передаем ответственность от создателя запроса к тому, кто принял
            if (request.MaterialId.HasValue)
            {
                // Списываем ответственность у создателя на складе утиля
                await _responsibilityFillingService.DecreaseMaterialResponsibilityAtWarehouseAsync(
                    request.ToWarehouseId, request.MaterialId.Value, request.Quantity, request.FromUserId);
                
                // Назначаем ответственность тому, кто принял
                await _responsibilityFillingService.AssignMaterialAtWarehouseAsync(
                    approvedByUserId, request.ToWarehouseId, request.MaterialId.Value, request.Quantity, request.MeasuringUnit);
            }
            else if (request.ProductId.HasValue)
            {
                // Списываем ответственность у создателя на складе утиля
                await _responsibilityFillingService.DecreaseProductResponsibilityAtWarehouseAsync(
                    request.ToWarehouseId, request.ProductId.Value, request.Quantity, request.FromUserId);
                
                // Назначаем ответственность тому, кто принял
                await _responsibilityFillingService.AssignProductAtWarehouseAsync(
                    approvedByUserId, request.ToWarehouseId, request.ProductId.Value, request.Quantity, request.MeasuringUnit);
            }

            request.Status = DisposalRequestStatus.Approved;
            request.ApprovedByUserId = approvedByUserId;
            request.ProcessedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var itemType = request.MaterialId.HasValue ? "Material" : "Product";
            var itemId = request.MaterialId ?? request.ProductId ?? 0;

            _logger.LogInformation(
                "DisposalRequest {RequestId} approved by user {ApprovedByUserId}. {ItemType} {ItemId} already moved, responsibility remains with creator {FromUserId}",
                requestId, approvedByUserId, itemType, itemId, request.FromUserId);

            return request;
        }

    public async Task<DisposalRequest> RejectDisposalRequestAsync(int requestId)
    {
        var request = await _context.DisposalRequests.FindAsync(requestId);

        if (request == null)
            throw new KeyNotFoundException($"DisposalRequest with ID {requestId} not found");

        if (request.Status != DisposalRequestStatus.Pending)
            throw new InvalidOperationException($"DisposalRequest {requestId} is not pending (current status: {request.Status})");

        request.Status = DisposalRequestStatus.Rejected;
        request.ProcessedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("DisposalRequest {RequestId} rejected", requestId);

        return request;
    }
}
