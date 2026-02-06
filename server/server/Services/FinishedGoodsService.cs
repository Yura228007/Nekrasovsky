using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;

namespace server.Services;

public class FinishedGoodsService : IFinishedGoodsService
{
    private readonly AppDbContext _context;
    private readonly ILogger<FinishedGoodsService> _logger;
    private readonly IResponsibilityFillingService _responsibilityFillingService;
    private readonly IDisposalRequestService _disposalRequestService;

    public FinishedGoodsService(
        AppDbContext context,
        ILogger<FinishedGoodsService> logger,
        IResponsibilityFillingService responsibilityFillingService,
        IDisposalRequestService disposalRequestService)
    {
        _context = context;
        _logger = logger;
        _responsibilityFillingService = responsibilityFillingService;
        _disposalRequestService = disposalRequestService;
    }

    public async Task ProcessSaleAsync(int userId, int warehouseId, int productId, double quantity, string? measuringUnit)
    {
        if (quantity <= 0)
            throw new InvalidOperationException("Количество должно быть больше нуля");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Проверяем наличие продукции на складе
            var filling = await _context.FillingWarehouses
                .FirstOrDefaultAsync(fw => fw.WarehouseId == warehouseId && fw.ProductId == productId);

            if (filling == null || filling.Quantity < quantity)
            {
                throw new InvalidOperationException(
                    $"Недостаточно продукции на складе: требуется {quantity}, доступно {filling?.Quantity ?? 0}");
            }

            // Проверяем ответственность пользователя
            var userResponsibleQty = await _responsibilityFillingService.GetUserResponsibleProductQuantityAtWarehouseAsync(
                userId, warehouseId, productId);

            if (userResponsibleQty < quantity)
            {
                throw new InvalidOperationException(
                    $"Недостаточно ответственности за продукт: требуется {quantity}, под ответственностью {userResponsibleQty}");
            }

            // Списываем продукцию со склада
            filling.Quantity -= quantity;

            // Списываем ответственность
            await _responsibilityFillingService.DecreaseProductResponsibilityAtWarehouseAsync(
                warehouseId, productId, quantity, userId);

            // Создаем запись о продаже
            var sale = new ProductSale
            {
                UserId = userId,
                WarehouseId = warehouseId,
                ProductId = productId,
                Quantity = quantity,
                MeasuringUnit = measuringUnit,
                SoldAt = DateTime.UtcNow
            };
            _context.ProductSales.Add(sale);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Product sale processed: User {UserId}, Warehouse {WarehouseId}, Product {ProductId}, Quantity {Quantity}",
                userId, warehouseId, productId, quantity);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task ProcessDisposalAsync(int userId, int warehouseId, int productId, double quantity, string? measuringUnit)
    {
        if (quantity <= 0)
            throw new InvalidOperationException("Количество должно быть больше нуля");

        // Находим склад утиля
        var disposalWarehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Type != null && w.Type.Contains("Утиль", StringComparison.OrdinalIgnoreCase) ||
                                      w.Name.Contains("Утиль", StringComparison.OrdinalIgnoreCase));

        if (disposalWarehouse == null)
        {
            throw new InvalidOperationException("Склад утиля не найден");
        }

        // Создаем запрос на утиль через сервис (он сам обработает перемещение и ответственность)
        await _disposalRequestService.CreateDisposalRequestForProductAsync(
            userId,
            warehouseId,
            disposalWarehouse.Id,
            productId,
            quantity,
            measuringUnit,
            DisposalRequestType.Defect);

        _logger.LogInformation(
            "Finished goods disposal request created: User {UserId}, FromWarehouse {FromWarehouseId}, ToWarehouse {ToWarehouseId}, Product {ProductId}, Quantity {Quantity}",
            userId, warehouseId, disposalWarehouse.Id, productId, quantity);
    }
}
