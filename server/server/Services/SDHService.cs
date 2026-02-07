using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;

namespace server.Services;

public class SDHService : ISDHService
{
    private readonly AppDbContext _context;
    private readonly ILogger<SDHService> _logger;
    private readonly IResponsibilityFillingService _responsibilityFillingService;
    private readonly IFillingWarehouseService _fillingWarehouseService;

    public SDHService(
        AppDbContext context,
        ILogger<SDHService> logger,
        IResponsibilityFillingService responsibilityFillingService,
        IFillingWarehouseService fillingWarehouseService)
    {
        _context = context;
        _logger = logger;
        _responsibilityFillingService = responsibilityFillingService;
        _fillingWarehouseService = fillingWarehouseService;
    }

    public async Task ProcessNonReturnableDefectAsync(int userId, int warehouseId, int? materialId, int? productId, double quantity, string? measuringUnit)
    {
        if (quantity <= 0)
            throw new InvalidOperationException("Количество должно быть больше нуля");

        if (materialId.HasValue == productId.HasValue)
            throw new InvalidOperationException("Укажите либо MaterialId, либо ProductId");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Проверяем наличие на складе
            FillingWarehouse? filling = null;
            if (materialId.HasValue)
            {
                filling = await _context.FillingWarehouses
                    .FirstOrDefaultAsync(fw => fw.WarehouseId == warehouseId && fw.MaterialId == materialId);
            }
            else if (productId.HasValue)
            {
                filling = await _context.FillingWarehouses
                    .FirstOrDefaultAsync(fw => fw.WarehouseId == warehouseId && fw.ProductId == productId);
            }

            if (filling == null || filling.Quantity < quantity)
            {
                throw new InvalidOperationException(
                    $"Недостаточно на складе: требуется {quantity}, доступно {filling?.Quantity ?? 0}");
            }

            // Проверяем ответственность пользователя
            double userResponsibleQty = 0;
            if (materialId.HasValue)
            {
                userResponsibleQty = await _responsibilityFillingService.GetUserResponsibleQuantityAtWarehouseAsync(
                    userId, warehouseId, materialId.Value);
            }
            else if (productId.HasValue)
            {
                userResponsibleQty = await _responsibilityFillingService.GetUserResponsibleProductQuantityAtWarehouseAsync(
                    userId, warehouseId, productId.Value);
            }

            if (userResponsibleQty < quantity)
            {
                throw new InvalidOperationException(
                    $"Недостаточно ответственности: требуется {quantity}, под ответственностью {userResponsibleQty}");
            }

            // Списываем со склада
            filling.Quantity -= quantity;

            // Списываем ответственность напрямую через контекст (в рамках транзакции)
            var responsibilityFillings = await _context.ResponsibilityFillings
                .Where(rf => rf.IsActive && rf.WarehouseId == warehouseId && 
                            ((materialId.HasValue && rf.MaterialId == materialId) || 
                             (productId.HasValue && rf.ProductId == productId)) &&
                            rf.UserId == userId && rf.Quantity > 0)
                .OrderByDescending(rf => rf.AssignedAt)
                .ToListAsync();

            if (responsibilityFillings.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Не удалось найти записи ответственности: требуется {quantity}, под ответственностью {userResponsibleQty}");
            }

            var totalAvailable = responsibilityFillings.Sum(rf => rf.Quantity);
            if (totalAvailable < quantity)
            {
                throw new InvalidOperationException(
                    $"Недостаточно ответственности для списания: требуется {quantity}, доступно {totalAvailable}");
            }

            double remaining = quantity;
            foreach (var rf in responsibilityFillings)
            {
                if (remaining <= 0)
                    break;
                double decrease = Math.Min(remaining, rf.Quantity);
                rf.Quantity -= decrease;
                remaining -= decrease;
                if (rf.Quantity <= 0)
                {
                    rf.IsActive = false;
                    rf.ReleasedAt = DateTime.UtcNow;
                    rf.Quantity = 0;
                }
            }

            if (remaining > 0)
            {
                throw new InvalidOperationException(
                    $"Не удалось полностью списать ответственность: осталось {remaining} из {quantity}");
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "SDH non-returnable defect processed: User {UserId}, Warehouse {WarehouseId}, Material {MaterialId}, Product {ProductId}, Quantity {Quantity}",
                userId, warehouseId, materialId, productId, quantity);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error in ProcessNonReturnableDefectAsync: User {UserId}, Warehouse {WarehouseId}, Material {MaterialId}, Product {ProductId}, Quantity {Quantity}",
                userId, warehouseId, materialId, productId, quantity);
            throw;
        }
    }

    public async Task ProcessSaleAsync(int userId, int warehouseId, int? materialId, int? productId, double quantity, string? measuringUnit)
    {
        if (quantity <= 0)
            throw new InvalidOperationException("Количество должно быть больше нуля");

        if (materialId.HasValue == productId.HasValue)
            throw new InvalidOperationException("Укажите либо MaterialId, либо ProductId");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Проверяем наличие на складе
            FillingWarehouse? filling = null;
            if (materialId.HasValue)
            {
                filling = await _context.FillingWarehouses
                    .FirstOrDefaultAsync(fw => fw.WarehouseId == warehouseId && fw.MaterialId == materialId);
            }
            else if (productId.HasValue)
            {
                filling = await _context.FillingWarehouses
                    .FirstOrDefaultAsync(fw => fw.WarehouseId == warehouseId && fw.ProductId == productId);
            }

            if (filling == null || filling.Quantity < quantity)
            {
                throw new InvalidOperationException(
                    $"Недостаточно на складе: требуется {quantity}, доступно {filling?.Quantity ?? 0}");
            }

            // Проверяем ответственность пользователя
            double userResponsibleQty = 0;
            if (materialId.HasValue)
            {
                userResponsibleQty = await _responsibilityFillingService.GetUserResponsibleQuantityAtWarehouseAsync(
                    userId, warehouseId, materialId.Value);
            }
            else if (productId.HasValue)
            {
                userResponsibleQty = await _responsibilityFillingService.GetUserResponsibleProductQuantityAtWarehouseAsync(
                    userId, warehouseId, productId.Value);
            }

            if (userResponsibleQty < quantity)
            {
                throw new InvalidOperationException(
                    $"Недостаточно ответственности: требуется {quantity}, под ответственностью {userResponsibleQty}");
            }

            // Списываем со склада
            filling.Quantity -= quantity;

            // Списываем ответственность напрямую через контекст (в рамках транзакции)
            var responsibilityFillings = await _context.ResponsibilityFillings
                .Where(rf => rf.IsActive && rf.WarehouseId == warehouseId && 
                            ((materialId.HasValue && rf.MaterialId == materialId) || 
                             (productId.HasValue && rf.ProductId == productId)) &&
                            rf.UserId == userId && rf.Quantity > 0)
                .OrderByDescending(rf => rf.AssignedAt)
                .ToListAsync();

            if (responsibilityFillings.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Не удалось найти записи ответственности: требуется {quantity}, под ответственностью {userResponsibleQty}");
            }

            var totalAvailable = responsibilityFillings.Sum(rf => rf.Quantity);
            if (totalAvailable < quantity)
            {
                throw new InvalidOperationException(
                    $"Недостаточно ответственности для списания: требуется {quantity}, доступно {totalAvailable}");
            }

            double remaining = quantity;
            foreach (var rf in responsibilityFillings)
            {
                if (remaining <= 0)
                    break;
                double decrease = Math.Min(remaining, rf.Quantity);
                rf.Quantity -= decrease;
                remaining -= decrease;
                if (rf.Quantity <= 0)
                {
                    rf.IsActive = false;
                    rf.ReleasedAt = DateTime.UtcNow;
                    rf.Quantity = 0;
                }
            }

            if (remaining > 0)
            {
                throw new InvalidOperationException(
                    $"Не удалось полностью списать ответственность: осталось {remaining} из {quantity}");
            }

            // Создаем запись о продаже
            var sale = new MaterialSDHSale
            {
                UserId = userId,
                WarehouseId = warehouseId,
                MaterialId = materialId,
                ProductId = productId,
                Quantity = quantity,
                MeasuringUnit = measuringUnit,
                SoldAt = DateTime.UtcNow
            };
            _context.MaterialSDHSales.Add(sale);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "SDH sale processed: User {UserId}, Warehouse {WarehouseId}, Material {MaterialId}, Product {ProductId}, Quantity {Quantity}",
                userId, warehouseId, materialId, productId, quantity);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error in ProcessSaleAsync: User {UserId}, Warehouse {WarehouseId}, Material {MaterialId}, Product {ProductId}, Quantity {Quantity}",
                userId, warehouseId, materialId, productId, quantity);
            throw;
        }
    }

    public async Task CreateSDHRequestAsync(int userId, int fromWarehouseId, int toWarehouseId, int? materialId, int? productId, double quantity, string? measuringUnit)
    {
        if (quantity <= 0)
            throw new InvalidOperationException("Количество должно быть больше нуля");

        if (materialId.HasValue == productId.HasValue)
            throw new InvalidOperationException("Укажите либо MaterialId, либо ProductId");

        // Проверяем наличие на складе
        FillingWarehouse? filling = null;
        if (materialId.HasValue)
        {
            filling = await _context.FillingWarehouses
                .FirstOrDefaultAsync(fw => fw.WarehouseId == fromWarehouseId && fw.MaterialId == materialId);
        }
        else if (productId.HasValue)
        {
            filling = await _context.FillingWarehouses
                .FirstOrDefaultAsync(fw => fw.WarehouseId == fromWarehouseId && fw.ProductId == productId);
        }

        if (filling == null || filling.Quantity < quantity)
        {
            throw new InvalidOperationException(
                $"Недостаточно на складе: требуется {quantity}, доступно {filling?.Quantity ?? 0}");
        }

        // Проверяем ответственность пользователя
        double userResponsibleQty = 0;
        if (materialId.HasValue)
        {
            userResponsibleQty = await _responsibilityFillingService.GetUserResponsibleQuantityAtWarehouseAsync(
                userId, fromWarehouseId, materialId.Value);
        }
        else if (productId.HasValue)
        {
            userResponsibleQty = await _responsibilityFillingService.GetUserResponsibleProductQuantityAtWarehouseAsync(
                userId, fromWarehouseId, productId.Value);
        }

        if (userResponsibleQty < quantity)
        {
            throw new InvalidOperationException(
                $"Недостаточно ответственности: требуется {quantity}, под ответственностью {userResponsibleQty}");
        }

        // Перемещаем на склад СДХ (наполнение меняется сразу, ответственность перемещается на склад СДХ, но остается у создателя)
        if (materialId.HasValue)
        {
            // Уменьшаем на исходном складе
            var fromFilling = await _context.FillingWarehouses
                .FirstOrDefaultAsync(fw => fw.WarehouseId == fromWarehouseId && fw.MaterialId == materialId);
            if (fromFilling != null)
            {
                fromFilling.Quantity -= quantity;
                if (fromFilling.Quantity < 0) fromFilling.Quantity = 0;
            }

            // Увеличиваем на складе СДХ
            var toFilling = await _context.FillingWarehouses
                .FirstOrDefaultAsync(fw => fw.WarehouseId == toWarehouseId && fw.MaterialId == materialId);
            if (toFilling != null)
            {
                toFilling.Quantity += quantity;
            }
            else
            {
                var material = await _context.Materials.FindAsync(materialId.Value);
                toFilling = new FillingWarehouse
                {
                    WarehouseId = toWarehouseId,
                    MaterialId = materialId.Value,
                    Quantity = quantity,
                    MeasuringType = measuringUnit ?? filling?.MeasuringType ?? material?.MeasuringUnit ?? "шт"
                };
                _context.FillingWarehouses.Add(toFilling);
            }

            // Списываем ответственность с исходного склада
            await _responsibilityFillingService.DecreaseMaterialResponsibilityAtWarehouseAsync(
                fromWarehouseId, materialId.Value, quantity, userId);

            // Добавляем ответственность на склад СДХ (отдельной строкой)
            // Ответственность остается у того же пользователя (userId), но теперь на складе СДХ
            await _responsibilityFillingService.AssignMaterialAtWarehouseAsync(
                userId, toWarehouseId, materialId.Value, quantity, measuringUnit ?? filling?.MeasuringType);
        }
        else if (productId.HasValue)
        {
            // Уменьшаем на исходном складе
            var fromFilling = await _context.FillingWarehouses
                .FirstOrDefaultAsync(fw => fw.WarehouseId == fromWarehouseId && fw.ProductId == productId);
            if (fromFilling != null)
            {
                fromFilling.Quantity -= quantity;
                if (fromFilling.Quantity < 0) fromFilling.Quantity = 0;
            }

            // Увеличиваем на складе СДХ
            var toFilling = await _context.FillingWarehouses
                .FirstOrDefaultAsync(fw => fw.WarehouseId == toWarehouseId && fw.ProductId == productId);
            if (toFilling != null)
            {
                toFilling.Quantity += quantity;
            }
            else
            {
                var product = await _context.Products.FindAsync(productId.Value);
                toFilling = new FillingWarehouse
                {
                    WarehouseId = toWarehouseId,
                    ProductId = productId.Value,
                    Quantity = quantity,
                    MeasuringType = measuringUnit ?? filling?.MeasuringType ?? product?.MeasuringUnit ?? "шт"
                };
                _context.FillingWarehouses.Add(toFilling);
            }

            // Списываем ответственность с исходного склада
            await _responsibilityFillingService.DecreaseProductResponsibilityAtWarehouseAsync(
                fromWarehouseId, productId.Value, quantity, userId);

            // Добавляем ответственность на склад СДХ (отдельной строкой)
            // Ответственность остается у того же пользователя (userId), но теперь на складе СДХ
            await _responsibilityFillingService.AssignProductAtWarehouseAsync(
                userId, toWarehouseId, productId.Value, quantity, measuringUnit ?? filling?.MeasuringType);
        }
        await _context.SaveChangesAsync();

        // Создаем запрос
        var request = new SDHRequest
        {
            FromUserId = userId,
            FromWarehouseId = fromWarehouseId,
            ToWarehouseId = toWarehouseId,
            MaterialId = materialId,
            ProductId = productId,
            Quantity = quantity,
            MeasuringUnit = measuringUnit ?? filling?.MeasuringType,
            Status = SDHRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        _context.SDHRequests.Add(request);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "SDH request created: User {UserId}, FromWarehouse {FromWarehouseId}, ToWarehouse {ToWarehouseId}, Material {MaterialId}, Product {ProductId}, Quantity {Quantity}",
            userId, fromWarehouseId, toWarehouseId, materialId, productId, quantity);
    }

    public async Task ApproveSDHRequestAsync(int requestId, int approvedByUserId)
    {
        var request = await _context.SDHRequests
            .FirstOrDefaultAsync(r => r.Id == requestId && r.Status == SDHRequestStatus.Pending);

        if (request == null)
        {
            throw new KeyNotFoundException($"Запрос с ID {requestId} не найден или уже обработан");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Передаем ответственность получателю (ответственность уже на складе СДХ, передаем ее другому пользователю)
            if (request.MaterialId.HasValue)
            {
                await _responsibilityFillingService.TransferMaterialResponsibilityAsync(
                    request.ToWarehouseId,
                    request.MaterialId.Value,
                    request.FromUserId,
                    approvedByUserId,
                    request.Quantity);
            }
            else if (request.ProductId.HasValue)
            {
                await _responsibilityFillingService.TransferProductResponsibilityAsync(
                    request.ToWarehouseId,
                    request.ProductId.Value,
                    request.FromUserId,
                    approvedByUserId,
                    request.Quantity);
            }

            // Обновляем запрос
            request.Status = SDHRequestStatus.Approved;
            request.ApprovedByUserId = approvedByUserId;
            request.ProcessedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "SDH request approved: Request {RequestId}, ApprovedBy {ApprovedByUserId}",
                requestId, approvedByUserId);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task RejectSDHRequestAsync(int requestId)
    {
        var request = await _context.SDHRequests
            .FirstOrDefaultAsync(r => r.Id == requestId && r.Status == SDHRequestStatus.Pending);

        if (request == null)
        {
            throw new KeyNotFoundException($"Запрос с ID {requestId} не найден или уже обработан");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Возвращаем на исходный склад
            if (request.MaterialId.HasValue)
            {
                // Уменьшаем на складе СДХ
                var toFilling = await _context.FillingWarehouses
                    .FirstOrDefaultAsync(fw => fw.WarehouseId == request.ToWarehouseId && fw.MaterialId == request.MaterialId);
                if (toFilling != null)
                {
                    toFilling.Quantity -= request.Quantity;
                    if (toFilling.Quantity < 0) toFilling.Quantity = 0;
                }

                // Увеличиваем на исходном складе
                var fromFilling = await _context.FillingWarehouses
                    .FirstOrDefaultAsync(fw => fw.WarehouseId == request.FromWarehouseId && fw.MaterialId == request.MaterialId);
                if (fromFilling != null)
                {
                    fromFilling.Quantity += request.Quantity;
                }
                else
                {
                    var material = await _context.Materials.FindAsync(request.MaterialId.Value);
                    fromFilling = new FillingWarehouse
                    {
                        WarehouseId = request.FromWarehouseId,
                        MaterialId = request.MaterialId.Value,
                        Quantity = request.Quantity,
                        MeasuringType = request.MeasuringUnit ?? material?.MeasuringUnit ?? "шт"
                    };
                    _context.FillingWarehouses.Add(fromFilling);
                }

                // Возвращаем ответственность на исходный склад
                await _responsibilityFillingService.DecreaseMaterialResponsibilityAtWarehouseAsync(
                    request.ToWarehouseId, request.MaterialId.Value, request.Quantity, request.FromUserId);
                await _responsibilityFillingService.AssignMaterialAtWarehouseAsync(
                    request.FromUserId, request.FromWarehouseId, request.MaterialId.Value, request.Quantity, request.MeasuringUnit);
            }
            else if (request.ProductId.HasValue)
            {
                // Уменьшаем на складе СДХ
                var toFilling = await _context.FillingWarehouses
                    .FirstOrDefaultAsync(fw => fw.WarehouseId == request.ToWarehouseId && fw.ProductId == request.ProductId);
                if (toFilling != null)
                {
                    toFilling.Quantity -= request.Quantity;
                    if (toFilling.Quantity < 0) toFilling.Quantity = 0;
                }

                // Увеличиваем на исходном складе
                var fromFilling = await _context.FillingWarehouses
                    .FirstOrDefaultAsync(fw => fw.WarehouseId == request.FromWarehouseId && fw.ProductId == request.ProductId);
                if (fromFilling != null)
                {
                    fromFilling.Quantity += request.Quantity;
                }
                else
                {
                    var product = await _context.Products.FindAsync(request.ProductId.Value);
                    fromFilling = new FillingWarehouse
                    {
                        WarehouseId = request.FromWarehouseId,
                        ProductId = request.ProductId.Value,
                        Quantity = request.Quantity,
                        MeasuringType = request.MeasuringUnit ?? product?.MeasuringUnit ?? "шт"
                    };
                    _context.FillingWarehouses.Add(fromFilling);
                }

                // Возвращаем ответственность на исходный склад
                await _responsibilityFillingService.DecreaseProductResponsibilityAtWarehouseAsync(
                    request.ToWarehouseId, request.ProductId.Value, request.Quantity, request.FromUserId);
                await _responsibilityFillingService.AssignProductAtWarehouseAsync(
                    request.FromUserId, request.FromWarehouseId, request.ProductId.Value, request.Quantity, request.MeasuringUnit);
            }

            // Обновляем запрос
            request.Status = SDHRequestStatus.Rejected;
            request.ProcessedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "SDH request rejected: Request {RequestId}",
                requestId);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
