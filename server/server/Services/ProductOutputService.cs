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
        private readonly ILogger<ProductOutputService> _logger;

        public ProductOutputService(
            AppDbContext context,
            IProductBatchService batchService,
            IResponsibilityFillingService responsibilityFillingService,
            IWarehouseService warehouseService,
            IFillingWarehouseService fillingWarehouseService,
            ILogger<ProductOutputService> logger)
        {
            _context = context;
            _batchService = batchService;
            _responsibilityFillingService = responsibilityFillingService;
            _warehouseService = warehouseService;
            _fillingWarehouseService = fillingWarehouseService;
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
                .Include(po => po.Machine)
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
                .Include(po => po.Machine)
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
                .Include(po => po.Machine)
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
                .Include(po => po.Machine)
                .FirstOrDefaultAsync(po => po.Id == id);
        }

        public async Task<ProductOutputOptionsResponse> GetOutputOptionsForUserAsync(int userId, bool hasSendToSale)
        {
            var response = new ProductOutputOptionsResponse { HasSendToSale = hasSendToSale };

            if (hasSendToSale)
            {
                var products = await _context.Products.Where(p => p.IsActive).ToListAsync();
                var warehouses = await _context.Warehouses.Where(w => w.IsActive).ToListAsync();
                var finishedGoods = (await _warehouseService.GetWarehousesByTypeAsync(FinishedGoodsWarehouseType)).FirstOrDefault();
                foreach (var p in products)
                {
                    foreach (var w in warehouses)
                    {
                        response.Options.Add(new ProductOutputOption
                        {
                            ProductId = p.Id,
                            ProductName = p.Name,
                            WarehouseId = w.Id,
                            WarehouseName = w.Name,
                            TargetWarehouseId = finishedGoods?.Id,
                            TargetWarehouseName = finishedGoods?.Name,
                            MaxQuantity = null,
                            MeasuringUnit = p.MeasuringUnit
                        });
                    }
                    response.Options.Add(new ProductOutputOption
                    {
                        ProductId = p.Id,
                        ProductName = p.Name,
                        WarehouseId = null,
                        WarehouseName = "Без склада",
                        TargetWarehouseId = finishedGoods?.Id,
                        TargetWarehouseName = finishedGoods?.Name,
                        MaxQuantity = null,
                        MeasuringUnit = p.MeasuringUnit
                    });
                }
                return response;
            }

            // Выпуск только из партий, доступных пользователю (ResponsibilityFilling по партиям)
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

            var batchIds = batchFillings.Select(rf => rf.ProductBatchId!.Value).Distinct().ToList();
            foreach (var batchId in batchIds)
            {
                var batch = await _batchService.GetBatchByIdAsync(batchId);
                if (batch == null || !batch.IsActive || batch.Quantity <= 0) continue;

                var responsibleQty = await _responsibilityFillingService.GetUserResponsibleQuantityForBatchAsync(userId, batchId);
                if (responsibleQty <= 0) continue;

                var maxQty = Math.Min(responsibleQty, batch.Quantity);
                var first = batchFillings.First(rf => rf.ProductBatchId == batchId);
                var product = batch.Product ?? first.ProductBatch?.Product;
                response.Options.Add(new ProductOutputOption
                {
                    ProductId = batch.ProductId,
                    ProductName = product?.Name ?? $"Продукт #{batch.ProductId}",
                    ProductBatchId = batch.Id,
                    BatchNumber = batch.BatchNumber,
                    WarehouseId = batch.WarehouseId,
                    WarehouseName = first.Warehouse?.Name ?? batch.Warehouse?.Name,
                    TargetWarehouseId = finishedGoodsWarehouse?.Id,
                    TargetWarehouseName = finishedGoodsWarehouse?.Name,
                    MaxQuantity = maxQty,
                    MeasuringUnit = batch.MeasuringUnit ?? product?.MeasuringUnit
                });
            }

            return response;
        }

        public async Task<ProductOutput> CreateAsync(ProductOutput productOutput, bool canBypassResponsibility)
        {
            var totalQty = productOutput.ProducedQuantity + productOutput.DefectQuantity + productOutput.EcoQuantity + productOutput.RewindQuantity;
            if (totalQty <= 0)
                throw new InvalidOperationException("Укажите количество (произведено + брак + эко + перемотка).");

            if (!canBypassResponsibility)
            {
                if (!productOutput.ProductBatchId.HasValue)
                    throw new InvalidOperationException("Для выпуска без права «Отправка на реализацию» необходимо указать партию (ProductBatchId).");

                var batch = await _batchService.GetBatchByIdAsync(productOutput.ProductBatchId.Value);
                if (batch == null)
                    throw new KeyNotFoundException($"Партия с ID {productOutput.ProductBatchId} не найдена.");
                if (!batch.IsActive || batch.Quantity <= 0)
                    throw new InvalidOperationException("Партия неактивна или пуста.");

                var responsible = await _responsibilityFillingService.GetUserResponsibleQuantityForBatchAsync(
                    productOutput.UserId, batch.Id);
                if (totalQty > responsible)
                    throw new InvalidOperationException(
                        $"Сумма (произведено + брак + эко + перемотка = {totalQty}) превышает вашу ответственность по партии ({responsible}).");
                if (totalQty > batch.Quantity)
                    throw new InvalidOperationException(
                        $"Сумма ({totalQty}) превышает количество в партии ({batch.Quantity}).");

                var finishedGoods = (await _warehouseService.GetWarehousesByTypeAsync(FinishedGoodsWarehouseType)).FirstOrDefault();
                if (finishedGoods == null)
                    throw new InvalidOperationException($"Склад типа «{FinishedGoodsWarehouseType}» не найден. Создайте склад для выпуска готовой продукции.");

                productOutput.ProductId = batch.ProductId;
                productOutput.WarehouseId = finishedGoods.Id;
                productOutput.MeasuringUnit = productOutput.MeasuringUnit ?? batch.MeasuringUnit ?? batch.Product?.MeasuringUnit;
            }

            productOutput.CreatedAt = DateTime.UtcNow;
            _context.ProductOutputs.Add(productOutput);
            await _context.SaveChangesAsync();

            if (!canBypassResponsibility && productOutput.ProductBatchId.HasValue && totalQty > 0)
            {
                var batch = await _batchService.GetBatchByIdAsync(productOutput.ProductBatchId.Value);
                var finishedGoods = (await _warehouseService.GetWarehousesByTypeAsync(FinishedGoodsWarehouseType)).FirstOrDefault();
                if (batch == null || finishedGoods == null) { /* уже проверено выше */ }
                else
                {
                    // Уменьшаем количество в партии (новая партия не создаётся)
                    var decreasedBatch = await _batchService.DecreaseBatchQuantityAsync(batch.Id, totalQty);

                    var product = await _context.Products.FindAsync(batch.ProductId);
                    var measuringType = batch.MeasuringUnit ?? product?.MeasuringUnit ?? "шт";

                    // Произведено → склад готовой продукции
                    if (productOutput.ProducedQuantity > 0)
                    {
                        await AddProductQuantityToWarehouseAsync(
                            finishedGoods.Id, batch.ProductId, productOutput.ProducedQuantity, measuringType);
                    }

                    // Невозвратный брак → склад утиля
                    if (productOutput.DefectQuantity > 0)
                    {
                        var disposalWarehouse = (await _warehouseService.GetWarehousesByTypeAsync(DisposalWarehouseType)).FirstOrDefault();
                        if (disposalWarehouse != null)
                        {
                            await AddProductQuantityToWarehouseAsync(
                                disposalWarehouse.Id, batch.ProductId, productOutput.DefectQuantity, measuringType);
                        }
                        else
                        {
                            _logger.LogWarning("Склад типа «{Type}» не найден; невозвратный брак ({Qty}) не перемещён на склад утиля.", DisposalWarehouseType, productOutput.DefectQuantity);
                        }
                    }

                    // Возвратный брак (эко) → склад ЭКО
                    if (productOutput.EcoQuantity > 0)
                    {
                        var ecoWarehouses = await _context.Warehouses
                            .Where(w => w.IsActive && EF.Functions.ILike(w.Type, EcoWarehouseType))
                            .ToListAsync();
                        var ecoWarehouse = ecoWarehouses.FirstOrDefault();
                        if (ecoWarehouse != null)
                        {
                            await AddProductQuantityToWarehouseAsync(
                                ecoWarehouse.Id, batch.ProductId, productOutput.EcoQuantity, measuringType);
                        }
                        else
                        {
                            _logger.LogWarning("Склад типа «{Type}» не найден; возвратный брак ({Qty}) не перемещён на склад ЭКО.", EcoWarehouseType, productOutput.EcoQuantity);
                        }
                    }

                    // Перемотка → склад Перемотка
                    if (productOutput.RewindQuantity > 0)
                    {
                        var rewindWarehouses = await _context.Warehouses
                            .Where(w => w.IsActive && EF.Functions.ILike(w.Type, RewindWarehouseType))
                            .ToListAsync();
                        var rewindWarehouse = rewindWarehouses.FirstOrDefault();
                        if (rewindWarehouse != null)
                        {
                            await AddProductQuantityToWarehouseAsync(
                                rewindWarehouse.Id, batch.ProductId, productOutput.RewindQuantity, measuringType);
                        }
                        else
                        {
                            _logger.LogWarning("Склад типа «{Type}» не найден; перемотка ({Qty}) не перемещена на склад перемотки.", RewindWarehouseType, productOutput.RewindQuantity);
                        }
                    }

                    // Партия полностью выпущена: активность = 0, перемещаем на склад готовой продукции
                    if (decreasedBatch.Quantity == 0)
                    {
                        decreasedBatch.IsActive = false;
                        decreasedBatch.WarehouseId = finishedGoods.Id;
                        await _context.SaveChangesAsync();
                        await _batchService.UpdateFillingWarehouseForProductAsync(batch.ProductId, finishedGoods.Id);
                    }

                    // Снимаем ответственность пользователя по партии
                    await _responsibilityFillingService.DecreaseBatchResponsibilityAsync(
                        batch.Id, totalQty, productOutput.UserId);
                }
            }

            return await GetByIdAsync(productOutput.Id) ?? productOutput;
        }

        public async Task<ProductOutput> UpdateAsync(int id, ProductOutput updated, bool canBypassResponsibility)
        {
            var existing = await _context.ProductOutputs.FindAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"ProductOutput with ID {id} not found");

            // Выпуск из партии: изменение количества при обновлении не поддерживается (логика по партии только при создании)
            if (existing.ProductBatchId.HasValue)
            {
                existing.MachineId = updated.MachineId;
                existing.Note = updated.Note;
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
            existing.MachineId = updated.MachineId;
            existing.ProducedQuantity = updated.ProducedQuantity;
            existing.DefectQuantity = updated.DefectQuantity;
            existing.EcoQuantity = updated.EcoQuantity;
            existing.RewindQuantity = updated.RewindQuantity;
            existing.MeasuringUnit = updated.MeasuringUnit;
            existing.Note = updated.Note;

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
        private async Task AddProductQuantityToWarehouseAsync(int warehouseId, int productId, int quantity, string? measuringType)
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
