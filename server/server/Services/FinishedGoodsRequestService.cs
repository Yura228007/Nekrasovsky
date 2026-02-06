using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;

namespace server.Services;

public class FinishedGoodsRequestService : IFinishedGoodsRequestService
{
    private readonly AppDbContext _context;
    private readonly ILogger<FinishedGoodsRequestService> _logger;
    private readonly IResponsibilityFillingService _responsibilityFillingService;
    private readonly IFillingWarehouseService _fillingWarehouseService;

    public FinishedGoodsRequestService(
        AppDbContext context,
        ILogger<FinishedGoodsRequestService> logger,
        IResponsibilityFillingService responsibilityFillingService,
        IFillingWarehouseService fillingWarehouseService)
    {
        _context = context;
        _logger = logger;
        _responsibilityFillingService = responsibilityFillingService;
        _fillingWarehouseService = fillingWarehouseService;
    }

    public async Task<FinishedGoodsRequest> CreateFinishedGoodsRequestAsync(
        int fromUserId, int fromWarehouseId, int toWarehouseId,
        int productId, double quantity, string? measuringUnit, FinishedGoodsRequestType requestType, int? productOutputId = null)
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

        // Перемещаем продукцию на склад готовой продукции физически
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

        // Списываем ответственность с исходного склада
        await _responsibilityFillingService.DecreaseProductResponsibilityAtWarehouseAsync(
            fromWarehouseId, productId, quantity, fromUserId);

        // Добавляем ответственность на склад готовой продукции (отдельной строкой)
        // Ответственность остается у того же пользователя (fromUserId), но теперь на складе готовой продукции
        await _responsibilityFillingService.AssignProductAtWarehouseAsync(
            fromUserId, toWarehouseId, productId, quantity, measuringUnit);

        // Создаем запрос (ответственность уже перемещена на склад готовой продукции, но остается у создателя до одобрения)
        var request = new FinishedGoodsRequest
        {
            FromUserId = fromUserId,
            FromWarehouseId = fromWarehouseId,
            ToWarehouseId = toWarehouseId,
            ProductId = productId,
            Quantity = quantity,
            MeasuringUnit = measuringUnit,
            RequestType = requestType,
            Status = FinishedGoodsRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ProductOutputId = productOutputId
        };

        _context.FinishedGoodsRequests.Add(request);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "FinishedGoodsRequest created: FromUser {FromUserId}, FromWarehouse {FromWarehouseId}, ToWarehouse {ToWarehouseId}, Product {ProductId}, Quantity {Quantity}, Type {RequestType}",
            fromUserId, fromWarehouseId, toWarehouseId, productId, quantity, requestType);

        return request;
    }

    public async Task<List<FinishedGoodsRequest>> GetPendingFinishedGoodsRequestsAsync(int finishedGoodsWarehouseId)
    {
        return await _context.FinishedGoodsRequests
            .Include(fgr => fgr.FromUser)
            .Include(fgr => fgr.Product)
            .Include(fgr => fgr.FromWarehouse)
            .Where(fgr => fgr.ToWarehouseId == finishedGoodsWarehouseId && fgr.Status == FinishedGoodsRequestStatus.Pending)
            .OrderByDescending(fgr => fgr.CreatedAt)
            .ToListAsync();
    }

    public async Task<FinishedGoodsRequest> ApproveFinishedGoodsRequestAsync(int requestId, int approvedByUserId)
    {
        var request = await _context.FinishedGoodsRequests
            .Include(fgr => fgr.Product)
            .FirstOrDefaultAsync(fgr => fgr.Id == requestId);

        if (request == null)
            throw new KeyNotFoundException($"FinishedGoodsRequest with ID {requestId} not found");

        if (request.Status != FinishedGoodsRequestStatus.Pending)
            throw new InvalidOperationException($"FinishedGoodsRequest {requestId} is not pending (current status: {request.Status})");

        // Передаем ответственность от создателя запроса менеджеру склада готовой продукции
        // Продукция уже физически перемещена при создании запроса, ответственность уже на складе готовой продукции у создателя
        // Списываем ответственность у создателя на складе готовой продукции
        await _responsibilityFillingService.DecreaseProductResponsibilityAtWarehouseAsync(
            request.ToWarehouseId, request.ProductId, request.Quantity, request.FromUserId);
        
        // Назначаем ответственность тому, кто принял
        await _responsibilityFillingService.AssignProductAtWarehouseAsync(
            approvedByUserId, request.ToWarehouseId, request.ProductId, request.Quantity, request.MeasuringUnit);

        request.Status = FinishedGoodsRequestStatus.Approved;
        request.ApprovedByUserId = approvedByUserId;
        request.ProcessedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "FinishedGoodsRequest {RequestId} approved by user {ApprovedByUserId}. Responsibility transferred from {FromUserId} to {ApprovedByUserId}",
            requestId, approvedByUserId, request.FromUserId, approvedByUserId);

        return request;
    }

    public async Task<FinishedGoodsRequest> RejectFinishedGoodsRequestAsync(int requestId, int rejectedByUserId)
    {
        var request = await _context.FinishedGoodsRequests
            .FirstOrDefaultAsync(fgr => fgr.Id == requestId);

        if (request == null)
            throw new KeyNotFoundException($"FinishedGoodsRequest with ID {requestId} not found");

        if (request.Status != FinishedGoodsRequestStatus.Pending)
            throw new InvalidOperationException($"FinishedGoodsRequest {requestId} is not pending (current status: {request.Status})");

        // При отклонении возвращаем продукцию обратно на исходный склад
        // и ответственность остается у создателя запроса
        var fromFilling = await _context.FillingWarehouses
            .FirstOrDefaultAsync(fw => fw.WarehouseId == request.FromWarehouseId && fw.ProductId == request.ProductId);

        if (fromFilling == null)
        {
            var product = await _context.Products.FindAsync(request.ProductId);
            fromFilling = new FillingWarehouse
            {
                WarehouseId = request.FromWarehouseId,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                MeasuringType = request.MeasuringUnit ?? product?.MeasuringUnit ?? "шт"
            };
            _context.FillingWarehouses.Add(fromFilling);
        }
        else
        {
            fromFilling.Quantity += request.Quantity;
        }

        var toFilling = await _context.FillingWarehouses
            .FirstOrDefaultAsync(fw => fw.WarehouseId == request.ToWarehouseId && fw.ProductId == request.ProductId);

        if (toFilling != null)
        {
            toFilling.Quantity -= request.Quantity;
            if (toFilling.Quantity < 0)
            {
                throw new InvalidOperationException($"Cannot reject: insufficient quantity on target warehouse {request.ToWarehouseId}.");
            }
        }

        request.Status = FinishedGoodsRequestStatus.Rejected;
        request.ApprovedByUserId = rejectedByUserId; // Используем это поле для хранения ID отклонившего
        request.ProcessedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "FinishedGoodsRequest {RequestId} rejected by user {RejectedByUserId}. Product returned to source warehouse, responsibility remains with {FromUserId}",
            requestId, rejectedByUserId, request.FromUserId);

        return request;
    }
}
