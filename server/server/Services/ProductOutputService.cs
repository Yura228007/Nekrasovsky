using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;

namespace server.Services
{
    public interface IProductOutputService
    {
        Task<IEnumerable<ProductOutput>> GetAllAsync();
        Task<IEnumerable<ProductOutput>> GetByUserAsync(int userId);
        Task<IEnumerable<ProductOutput>> GetByWorkReportAsync(int workReportId);
        Task<ProductOutput?> GetByIdAsync(int id);
        /// <summary>Варианты выпуска для пользователя: только (продукт, склад) под ответственностью и макс. количество.</summary>
        Task<ProductOutputOptionsResponse> GetOutputOptionsForUserAsync(int userId, bool hasSendToSale);
        Task<ProductOutput> CreateAsync(ProductOutput productOutput, bool canBypassResponsibility);
        Task<ProductOutput> UpdateAsync(int id, ProductOutput updated, bool canBypassResponsibility);
        Task DeleteAsync(int id);
    }

    public class ProductOutputService : IProductOutputService
    {
        private const string FinishedGoodsWarehouseType = "Готовая продукция";
        private const string DisposalWarehouseType = "Утиль";
        private const string EcoWarehouseType = "ЭКО";
        private const string RewindWarehouseType = "Перемотка";
        private readonly AppDbContext _context;
        private readonly IProductBatchService _batchService;
        private readonly IResponsibilityFillingService _responsibilityFillingService;
        private readonly IWarehouseService _warehouseService;
        private readonly IFillingWarehouseService _fillingWarehouseService;
        private readonly IFinishedGoodsRequestService _finishedGoodsRequestService;
        private readonly IDisposalRequestService _disposalRequestService;
        private readonly IPartRequestService _partRequestService;
        private readonly ILogger<ProductOutputService> _logger;

        public ProductOutputService(
            AppDbContext context,
            IProductBatchService batchService,
            IResponsibilityFillingService responsibilityFillingService,
            IWarehouseService warehouseService,
            IFillingWarehouseService fillingWarehouseService,
            IFinishedGoodsRequestService finishedGoodsRequestService,
            IDisposalRequestService disposalRequestService,
            IPartRequestService partRequestService,
            ILogger<ProductOutputService> logger)
        {
            _context = context;
            _batchService = batchService;
            _responsibilityFillingService = responsibilityFillingService;
            _warehouseService = warehouseService;
            _fillingWarehouseService = fillingWarehouseService;
            _finishedGoodsRequestService = finishedGoodsRequestService;
            _disposalRequestService = disposalRequestService;
            _partRequestService = partRequestService;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductOutput>> GetAllAsync()
        {
            return await _context.ProductOutputs
                .Include(po => po.User)
                .Include(po => po.Product)
                .Include(po => po.Warehouse)
                .Include(po => po.ProductBatch)
                .Include(po => po.WorkReport)
                .OrderByDescending(po => po.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductOutput>> GetByUserAsync(int userId)
        {
            return await _context.ProductOutputs
                .Include(po => po.User)
                .Include(po => po.Product)
                .Include(po => po.Warehouse)
                .Include(po => po.ProductBatch)
                .Include(po => po.WorkReport)
                .Where(po => po.UserId == userId)
                .OrderByDescending(po => po.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductOutput>> GetByWorkReportAsync(int workReportId)
        {
            return await _context.ProductOutputs
                .Include(po => po.User)
                .Include(po => po.Product)
                .Include(po => po.Warehouse)
                .Include(po => po.ProductBatch)
                .Include(po => po.WorkReport)
                .Where(po => po.WorkReportId == workReportId)
                .OrderByDescending(po => po.CreatedAt)
                .ToListAsync();
        }

        public async Task<ProductOutput?> GetByIdAsync(int id)
        {
            return await _context.ProductOutputs
                .Include(po => po.User)
                .Include(po => po.Product)
                .Include(po => po.Warehouse)
                .Include(po => po.ProductBatch)
                .Include(po => po.WorkReport)
                .FirstOrDefaultAsync(po => po.Id == id);
        }

        public async Task<ProductOutputOptionsResponse> GetOutputOptionsForUserAsync(int userId, bool hasSendToSale)
        {
            var response = new ProductOutputOptionsResponse { HasSendToSale = hasSendToSale };

            // Получаем партии под ответственностью пользователя
            var batchFillings = await _context.ResponsibilityFillings
                .Where(rf => rf.UserId == userId && rf.IsActive && rf.ProductBatchId != null && rf.Quantity > 0)
                .Include(rf => rf.ProductBatch!)
                .ThenInclude(pb => pb!.Product)
                .Include(rf => rf.Warehouse)
                .ToListAsync();

            var finishedGoodsWarehouse = (await _warehouseService.GetWarehousesByTypeAsync(FinishedGoodsWarehouseType)).FirstOrDefault();
            if (finishedGoodsWarehouse == null)
            {
                _logger.LogWarning("Warehouse type '{Type}' not found; product output options may have no target warehouse.", FinishedGoodsWarehouseType);
            }

            // Группируем по партиям
            var batchGroups = batchFillings
                .GroupBy(rf => rf.ProductBatchId!.Value)
                .ToList();

            // Сначала добавляем обычные партии, потом ЭКО
            var normalBatches = new List<(int BatchId, ProductBatch Batch, double ResponsibleQty, Warehouse? Warehouse)>();
            var ecoBatches = new List<(int BatchId, ProductBatch Batch, double ResponsibleQty, Warehouse? Warehouse)>();

            foreach (var group in batchGroups)
            {
                var batchId = group.Key;
                var batch = await _batchService.GetBatchByIdAsync(batchId);
                if (batch == null || !batch.IsActive || batch.Quantity <= 0) continue;

                var responsibleQty = await _responsibilityFillingService.GetUserResponsibleQuantityForBatchAsync(userId, batchId);
                if (responsibleQty <= 0) continue;

                var maxQty = Math.Min(responsibleQty, batch.Quantity);
                var first = group.First();
                var warehouse = first.Warehouse ?? batch.Warehouse;

                // Проверяем, является ли партия ЭКО (BatchNumber начинается с "ECO-")
                var isEco = !string.IsNullOrEmpty(batch.BatchNumber) && batch.BatchNumber.StartsWith("ECO-", StringComparison.OrdinalIgnoreCase);

                if (isEco)
                {
                    ecoBatches.Add((batchId, batch, maxQty, warehouse));
                }
                else
                {
                    normalBatches.Add((batchId, batch, maxQty, warehouse));
                }
            }

            // Добавляем обычные партии
            foreach (var (batchId, batch, maxQty, warehouse) in normalBatches)
            {
                var product = batch.Product;
                if (product == null || !product.IsActive) continue;

                response.Options.Add(new ProductOutputOption
                {
                    ProductId = batch.ProductId,
                    ProductName = product.Name,
                    ProductBatchId = batch.Id,
                    BatchNumber = batch.BatchNumber,
                    WarehouseId = batch.WarehouseId,
                    WarehouseName = warehouse?.Name ?? batch.Warehouse?.Name ?? "Без склада",
                    TargetWarehouseId = finishedGoodsWarehouse?.Id,
                    TargetWarehouseName = finishedGoodsWarehouse?.Name,
                    MaxQuantity = maxQty,
                    MeasuringUnit = batch.MeasuringUnit ?? product.MeasuringUnit
                });
            }

            // Добавляем ЭКО партии с префиксом "ЭКО:"
            foreach (var (batchId, batch, maxQty, warehouse) in ecoBatches)
            {
                var product = batch.Product;
                if (product == null || !product.IsActive) continue;

                response.Options.Add(new ProductOutputOption
                {
                    ProductId = batch.ProductId,
                    ProductName = $"ЭКО: {product.Name}",
                    ProductBatchId = batch.Id,
                    BatchNumber = batch.BatchNumber,
                    WarehouseId = batch.WarehouseId,
                    WarehouseName = warehouse?.Name ?? batch.Warehouse?.Name ?? "Без склада",
                    TargetWarehouseId = finishedGoodsWarehouse?.Id,
                    TargetWarehouseName = finishedGoodsWarehouse?.Name,
                    MaxQuantity = maxQty,
                    MeasuringUnit = batch.MeasuringUnit ?? product.MeasuringUnit
                });
            }

            return response;
        }

        public async Task<ProductOutput> CreateAsync(ProductOutput productOutput, bool canBypassResponsibility)
        {
            var totalQty = productOutput.ProducedQuantity + productOutput.DefectQuantity + productOutput.EcoQuantity + productOutput.RewindQuantity;
            if (totalQty <= 0)
                throw new InvalidOperationException("Укажите количество (произведено + брак + эко + перемотка).");

            // Проверяем ответственность пользователя
            if (!productOutput.WarehouseId.HasValue)
                throw new InvalidOperationException("Необходимо указать склад (WarehouseId).");

            // Получаем продукт для единицы измерения
            var product = await _context.Products.FindAsync(productOutput.ProductId);
            if (product == null)
                throw new KeyNotFoundException($"Продукт с ID {productOutput.ProductId} не найден.");

            var measuringUnit = productOutput.MeasuringUnit ?? product.MeasuringUnit ?? "шт";

            // Если указана партия, работаем с партией, иначе с продуктом напрямую (для обратной совместимости)
            if (productOutput.ProductBatchId.HasValue)
            {
                // Работаем с партией
                var batch = await _batchService.GetBatchByIdAsync(productOutput.ProductBatchId.Value);
                if (batch == null)
                    throw new KeyNotFoundException($"Партия с ID {productOutput.ProductBatchId.Value} не найдена.");

                if (batch.ProductId != productOutput.ProductId)
                    throw new InvalidOperationException($"Партия {productOutput.ProductBatchId.Value} не соответствует продукту {productOutput.ProductId}.");

                // Проверяем ответственность за партию
                var userResponsibleQty = await _responsibilityFillingService.GetUserResponsibleQuantityForBatchAsync(
                    productOutput.UserId, productOutput.ProductBatchId.Value);

                if (userResponsibleQty < totalQty)
                {
                    throw new InvalidOperationException(
                        $"Недостаточно ответственности за партию {batch.BatchNumber ?? $"#{batch.Id}"}: требуется {totalQty}, под ответственностью {userResponsibleQty}.");
                }

                // Проверяем количество в партии
                if (batch.Quantity < totalQty)
                {
                    throw new InvalidOperationException(
                        $"Недостаточно товара в партии {batch.BatchNumber ?? $"#{batch.Id}"}: требуется {totalQty}, доступно {batch.Quantity}.");
                }
            }
            else
            {
                // Работаем с продуктом напрямую (для обратной совместимости)
                var userResponsibleQty = await _responsibilityFillingService.GetUserResponsibleProductQuantityAtWarehouseAsync(
                    productOutput.UserId, productOutput.WarehouseId.Value, productOutput.ProductId);

                if (userResponsibleQty < totalQty)
                {
                    throw new InvalidOperationException(
                        $"Недостаточно ответственности за продукт (ID {productOutput.ProductId}) на складе {productOutput.WarehouseId}: требуется {totalQty}, под ответственностью {userResponsibleQty}.");
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Списываем ответственность: либо с партии, либо с продукта
                if (productOutput.ProductBatchId.HasValue)
                {
                    // Списываем ответственность с партии
                    await _responsibilityFillingService.DecreaseBatchResponsibilityAsync(
                        productOutput.ProductBatchId.Value, totalQty, productOutput.UserId);
                }
                else
                {
                    // Списываем ответственность с продукта (для обратной совместимости)
                    await _responsibilityFillingService.DecreaseProductResponsibilityAtWarehouseAsync(
                        productOutput.WarehouseId.Value, productOutput.ProductId, totalQty, productOutput.UserId);
                }

                // 2. Списывается то же число с наполнения склада (физическое наполнение)
                var fromFilling = await _context.FillingWarehouses
                    .FirstOrDefaultAsync(fw => fw.WarehouseId == productOutput.WarehouseId.Value && fw.ProductId == productOutput.ProductId);

                if (fromFilling == null || fromFilling.Quantity < totalQty)
                {
                    throw new InvalidOperationException(
                        $"Недостаточно продукции на складе {productOutput.WarehouseId}: требуется {totalQty}, доступно {fromFilling?.Quantity ?? 0}.");
                }

                fromFilling.Quantity -= totalQty;

                // 3. Создается ProductOutput (без изменения БД - только сохранение записи)
                productOutput.MeasuringUnit = measuringUnit;
                productOutput.CreatedAt = DateTime.UtcNow;
                _context.ProductOutputs.Add(productOutput);
                await _context.SaveChangesAsync();

                // 3.1. Если указана партия, списываем товар с партии
                if (productOutput.ProductBatchId.HasValue)
                {
                    var batch = await _batchService.GetBatchByIdAsync(productOutput.ProductBatchId.Value);
                    if (batch != null && batch.IsActive)
                    {
                        // Количество уже проверено выше, просто списываем
                        batch.Quantity -= totalQty;
                        if (batch.Quantity <= 0)
                        {
                            batch.IsActive = false;
                        }
                        await _context.SaveChangesAsync();
                    }
                }

                // 4. Создается ответственность с producedQuantity+ecoQuantity на складе normalWarehouseId и его заполнение, создается запрос
                var finishedGoodsQty = productOutput.ProducedQuantity + productOutput.EcoQuantity;
                if (finishedGoodsQty > 0 && productOutput.NormalWarehouseId.HasValue)
                {
                    // Добавляем продукцию на склад готовой продукции
                    var toFilling = await _context.FillingWarehouses
                        .FirstOrDefaultAsync(fw => fw.WarehouseId == productOutput.NormalWarehouseId.Value && fw.ProductId == productOutput.ProductId);

                    if (toFilling == null)
                    {
                        toFilling = new FillingWarehouse
                        {
                            WarehouseId = productOutput.NormalWarehouseId.Value,
                            ProductId = productOutput.ProductId,
                            Quantity = finishedGoodsQty,
                            MeasuringType = measuringUnit
                        };
                        _context.FillingWarehouses.Add(toFilling);
                    }
                    else
                    {
                        toFilling.Quantity += finishedGoodsQty;
                    }

                    // Сохраняем изменения FillingWarehouse перед назначением ответственности
                    await _context.SaveChangesAsync();

                    // Назначаем ответственность на склад готовой продукции
                    await _responsibilityFillingService.AssignProductAtWarehouseAsync(
                        productOutput.UserId, productOutput.NormalWarehouseId.Value, productOutput.ProductId, finishedGoodsQty, measuringUnit);

                    // Создаем запросы отдельно для нормальной и ЭКО продукции
                    if (productOutput.ProducedQuantity > 0)
                    {
                        var request = new FinishedGoodsRequest
                        {
                            FromUserId = productOutput.UserId,
                            FromWarehouseId = productOutput.WarehouseId.Value,
                            ToWarehouseId = productOutput.NormalWarehouseId.Value,
                            ProductId = productOutput.ProductId,
                            Quantity = productOutput.ProducedQuantity,
                            MeasuringUnit = measuringUnit,
                            RequestType = FinishedGoodsRequestType.Normal,
                            Status = FinishedGoodsRequestStatus.Pending,
                            CreatedAt = DateTime.UtcNow,
                            ProductOutputId = productOutput.Id
                        };
                        _context.FinishedGoodsRequests.Add(request);
                    }

                    if (productOutput.EcoQuantity > 0)
                    {
                        var request = new FinishedGoodsRequest
                        {
                            FromUserId = productOutput.UserId,
                            FromWarehouseId = productOutput.WarehouseId.Value,
                            ToWarehouseId = productOutput.NormalWarehouseId.Value,
                            ProductId = productOutput.ProductId,
                            Quantity = productOutput.EcoQuantity,
                            MeasuringUnit = measuringUnit,
                            RequestType = FinishedGoodsRequestType.Eco,
                            Status = FinishedGoodsRequestStatus.Pending,
                            CreatedAt = DateTime.UtcNow,
                            ProductOutputId = productOutput.Id
                        };
                        _context.FinishedGoodsRequests.Add(request);
                    }
                }

                // 5. Создается запрос на отправку брака на утиль
                // Продукт уже списан со склада источника (в totalQty), нужно добавить на склад утиля и создать запрос
                if (productOutput.DefectQuantity > 0 && productOutput.DefectWarehouseId.HasValue)
                {
                    // Добавляем продукцию на склад утиля (физическое наполнение)
                    var defectFilling = await _context.FillingWarehouses
                        .FirstOrDefaultAsync(fw => fw.WarehouseId == productOutput.DefectWarehouseId.Value && fw.ProductId == productOutput.ProductId);

                    if (defectFilling == null)
                    {
                        defectFilling = new FillingWarehouse
                        {
                            WarehouseId = productOutput.DefectWarehouseId.Value,
                            ProductId = productOutput.ProductId,
                            Quantity = productOutput.DefectQuantity,
                            MeasuringType = measuringUnit
                        };
                        _context.FillingWarehouses.Add(defectFilling);
                    }
                    else
                    {
                        defectFilling.Quantity += productOutput.DefectQuantity;
                    }

                    // Сохраняем изменения FillingWarehouse перед назначением ответственности
                    await _context.SaveChangesAsync();

                    // Назначаем ответственность на склад утиля
                    await _responsibilityFillingService.AssignProductAtWarehouseAsync(
                        productOutput.UserId, productOutput.DefectWarehouseId.Value, productOutput.ProductId, productOutput.DefectQuantity, measuringUnit);

                    // Создаем запрос (продукт уже перемещен, ответственность назначена)
                    var disposalRequest = new DisposalRequest
                    {
                        FromUserId = productOutput.UserId,
                        FromWarehouseId = productOutput.WarehouseId.Value,
                        ToWarehouseId = productOutput.DefectWarehouseId.Value,
                        ProductId = productOutput.ProductId,
                        Quantity = productOutput.DefectQuantity,
                        MeasuringUnit = measuringUnit,
                        RequestType = DisposalRequestType.Defect,
                        Status = DisposalRequestStatus.Pending,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.DisposalRequests.Add(disposalRequest);
                }

                // 6. Создается запрос на передачу продукта в количестве rewindQuantity, пользователю rewindToUserId, на склад rewindWarehouseId
                // Ответственность и склад не меняются пока пользователь не подтвердит в запросах
                if (productOutput.RewindQuantity > 0 && productOutput.RewindWarehouseId.HasValue && productOutput.RewindToUserId.HasValue)
                {
                    var partRequest = new PartRequest
                    {
                        FromUserId = productOutput.UserId,
                        ToUserId = productOutput.RewindToUserId.Value,
                        FromWarehouseId = productOutput.WarehouseId.Value,
                        ToWarehouseId = productOutput.RewindWarehouseId.Value,
                        ProductId = productOutput.ProductId,
                        Quantity = productOutput.RewindQuantity,
                        MeasuringType = measuringUnit,
                        Status = PartRequestStatus.Pending,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.PartRequests.Add(partRequest);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetByIdAsync(productOutput.Id) ?? productOutput;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ProductOutput> UpdateAsync(int id, ProductOutput updated, bool canBypassResponsibility)
        {
            var existing = await _context.ProductOutputs.FindAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"ProductOutput with ID {id} not found");

            // Выпуск из партии: изменение количества при обновлении не поддерживается (логика по партии только при создании)
            if (existing.ProductBatchId.HasValue)
            {
                await _context.SaveChangesAsync();
                return await GetByIdAsync(id) ?? existing;
            }

            if (!canBypassResponsibility)
            {
                if (!updated.WarehouseId.HasValue)
                    throw new InvalidOperationException("Для выпуска без права «Отправка на реализацию» необходимо указать склад.");

                var totalQty = updated.ProducedQuantity + updated.DefectQuantity + updated.EcoQuantity + updated.RewindQuantity;
                var responsible = await _responsibilityFillingService.GetUserResponsibleProductQuantityAtWarehouseAsync(
                    updated.UserId, updated.WarehouseId.Value, updated.ProductId);
                var alreadyUsedByOther = await _context.ProductOutputs
                    .Where(po => po.UserId == updated.UserId && po.ProductId == updated.ProductId && po.WarehouseId == updated.WarehouseId && po.Id != id && !po.ProductBatchId.HasValue)
                    .SumAsync(po => po.ProducedQuantity + po.DefectQuantity + po.EcoQuantity + po.RewindQuantity);
                var available = responsible - alreadyUsedByOther + (existing.ProducedQuantity + existing.DefectQuantity + existing.EcoQuantity + existing.RewindQuantity);
                if (totalQty > available)
                    throw new InvalidOperationException(
                        $"Сумма (произведено + брак + эко + перемотка = {totalQty}) превышает доступное количество по ответственности ({available}).");
            }

            var existingTotal = existing.ProducedQuantity + existing.DefectQuantity + existing.EcoQuantity + existing.RewindQuantity;
            var updatedTotal = updated.ProducedQuantity + updated.DefectQuantity + updated.EcoQuantity + updated.RewindQuantity;
            var totalDelta = updatedTotal - existingTotal;

            existing.ProductId = updated.ProductId;
            existing.WarehouseId = updated.WarehouseId;
            existing.ProducedQuantity = updated.ProducedQuantity;
            existing.DefectQuantity = updated.DefectQuantity;
            existing.EcoQuantity = updated.EcoQuantity;
            existing.RewindQuantity = updated.RewindQuantity;
            existing.MeasuringUnit = updated.MeasuringUnit;

            await _context.SaveChangesAsync();

            if (totalDelta > 0 && existing.WarehouseId.HasValue)
            {
                var decreased = await _responsibilityFillingService.DecreaseProductResponsibilityAtWarehouseAsync(
                    existing.WarehouseId.Value, existing.ProductId, totalDelta, existing.UserId);
                if (!decreased)
                    _logger.LogWarning("ProductOutput Update {OutputId}: could not decrease responsibility for User {UserId}, Product {ProductId}, Warehouse {WarehouseId}, Delta {Delta}",
                        id, existing.UserId, existing.ProductId, existing.WarehouseId.Value, totalDelta);
            }

            return await GetByIdAsync(id) ?? existing;
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _context.ProductOutputs.FindAsync(id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"ProductOutput with ID {id} not found");
            }

            _context.ProductOutputs.Remove(existing);
            await _context.SaveChangesAsync();
        }

        /// <summary>Добавляет количество продукта на склад (создаёт или обновляет FillingWarehouse).</summary>
        private async Task AddProductQuantityToWarehouseAsync(int warehouseId, int productId, double quantity, string? measuringType)
        {
            if (quantity <= 0) return;

            var filling = await _fillingWarehouseService.GetFillingByProductAsync(warehouseId, productId);
            if (filling == null)
            {
                await _fillingWarehouseService.CreateFillingWarehouseAsync(new FillingWarehouse
                {
                    WarehouseId = warehouseId,
                    ProductId = productId,
                    Quantity = quantity,
                    MeasuringType = measuringType
                });
            }
            else
            {
                await _fillingWarehouseService.UpdateQuantityByProductAsync(
                    warehouseId, productId, filling.Quantity + quantity);
            }
        }
    }
}
