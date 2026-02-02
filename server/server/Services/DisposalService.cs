using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;

namespace server.Services
{
    public class DisposalService : IDisposalService
    {
        private readonly AppDbContext _context;
        private readonly IFillingWarehouseService _fillingService;
        private readonly IResponsibilityFillingService _responsibilityFillingService;
        private readonly ILogger<DisposalService> _logger;

        public DisposalService(
            AppDbContext context,
            IFillingWarehouseService fillingService,
            IResponsibilityFillingService responsibilityFillingService,
            ILogger<DisposalService> logger)
        {
            _context = context;
            _fillingService = fillingService;
            _responsibilityFillingService = responsibilityFillingService;
            _logger = logger;
        }

        public async Task ProcessDisposalAsync(int disposalWarehouseId, int? materialId, int? productId,
            int returnableQuantity, int nonReturnableQuantity)
        {
            var totalQty = returnableQuantity + nonReturnableQuantity;
            if (totalQty <= 0)
            {
                throw new InvalidOperationException("Укажите количество возвратного или невозвратного брака.");
            }

            var hasMaterial = materialId.HasValue;
            var hasProduct = productId.HasValue;
            if (hasMaterial == hasProduct)
            {
                throw new InvalidOperationException("Укажите либо MaterialId, либо ProductId.");
            }

            var disposalWarehouse = await _context.Warehouses.FindAsync(disposalWarehouseId);
            if (disposalWarehouse == null || !disposalWarehouse.IsActive)
            {
                throw new KeyNotFoundException("Склад утиля не найден или неактивен.");
            }

            var ecoWarehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w => EF.Functions.ILike(w.Type, "ЭКО") && w.IsActive);
            if (ecoWarehouse == null && returnableQuantity > 0)
            {
                throw new InvalidOperationException("Склад ЭКО не найден. Создайте склад типа ЭКО для возвратного брака.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (hasMaterial)
                {
                    await ProcessMaterialDisposalAsync(disposalWarehouseId, materialId!.Value,
                        returnableQuantity, nonReturnableQuantity, ecoWarehouse?.Id);
                }
                else
                {
                    await ProcessProductDisposalAsync(disposalWarehouseId, productId!.Value,
                        returnableQuantity, nonReturnableQuantity, ecoWarehouse?.Id);
                }

                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Disposal processed: Warehouse {WarehouseId}, Material {MaterialId}, Product {ProductId}, Returnable {Returnable}, NonReturnable {NonReturnable}",
                    disposalWarehouseId, materialId, productId, returnableQuantity, nonReturnableQuantity);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task ProcessMaterialDisposalAsync(int disposalWarehouseId, int materialId,
            int returnableQty, int nonReturnableQty, int? ecoWarehouseId)
        {
            var filling = await _fillingService.GetFillingByMaterialAsync(disposalWarehouseId, materialId);
            if (filling == null)
            {
                throw new InvalidOperationException("Материал не найден на складе утиля.");
            }

            var totalQty = returnableQty + nonReturnableQty;
            if (filling.Quantity < totalQty)
            {
                throw new InvalidOperationException($"Недостаточно количества. На складе: {filling.Quantity}, запрошено: {totalQty}.");
            }

            // Возвратный брак: переместить в ЭКО
            if (returnableQty > 0 && ecoWarehouseId.HasValue)
            {
                var ecoFilling = await _fillingService.GetFillingByMaterialAsync(ecoWarehouseId.Value, materialId);
                if (ecoFilling == null)
                {
                    var material = await _context.Materials.FindAsync(materialId);
                    var newFilling = new FillingWarehouse
                    {
                        WarehouseId = ecoWarehouseId.Value,
                        MaterialId = materialId,
                        Quantity = returnableQty,
                        MeasuringType = material?.MeasuringUnit ?? "шт"
                    };
                    await _fillingService.CreateFillingWarehouseAsync(newFilling);
                }
                else
                {
                    await _fillingService.UpdateQuantityByMaterialAsync(ecoWarehouseId.Value, materialId, ecoFilling.Quantity + returnableQty);
                }
            }

            // Уменьшить на складе утиля
            var newQuantity = filling.Quantity - totalQty;
            if (newQuantity <= 0)
            {
                await _fillingService.DeleteFillingWarehouseByMaterialAsync(disposalWarehouseId, materialId);
            }
            else
            {
                await _fillingService.UpdateQuantityByMaterialAsync(disposalWarehouseId, materialId, newQuantity);
            }

            // Снять ответственность по ResponsibilityFilling с склада утиля (в т.ч. часть, ушедшая на ЭКО)
            await _responsibilityFillingService.DecreaseMaterialResponsibilityAtWarehouseByQuantityAsync(disposalWarehouseId, materialId, totalQty);

            await _context.SaveChangesAsync();
        }

        private async Task ProcessProductDisposalAsync(int disposalWarehouseId, int productId,
            int returnableQty, int nonReturnableQty, int? ecoWarehouseId)
        {
            var filling = await _fillingService.GetFillingByProductAsync(disposalWarehouseId, productId);
            if (filling == null)
            {
                throw new InvalidOperationException("Продукт не найден на складе утиля.");
            }

            var totalQty = returnableQty + nonReturnableQty;
            if (filling.Quantity < totalQty)
            {
                throw new InvalidOperationException($"Недостаточно количества. На складе: {filling.Quantity}, запрошено: {totalQty}.");
            }

            // Возвратный брак: переместить в ЭКО
            if (returnableQty > 0 && ecoWarehouseId.HasValue)
            {
                var ecoFilling = await _fillingService.GetFillingByProductAsync(ecoWarehouseId.Value, productId);
                if (ecoFilling == null)
                {
                    var product = await _context.Products.FindAsync(productId);
                    var newFilling = new FillingWarehouse
                    {
                        WarehouseId = ecoWarehouseId.Value,
                        ProductId = productId,
                        Quantity = returnableQty,
                        MeasuringType = product?.MeasuringUnit ?? "шт"
                    };
                    await _fillingService.CreateFillingWarehouseAsync(newFilling);
                }
                else
                {
                    await _fillingService.UpdateQuantityByProductAsync(ecoWarehouseId.Value, productId, ecoFilling.Quantity + returnableQty);
                }
            }

            // Уменьшить на складе утиля
            var newQuantity = filling.Quantity - totalQty;
            if (newQuantity <= 0)
            {
                await _fillingService.DeleteFillingWarehouseByProductAsync(disposalWarehouseId, productId);
            }
            else
            {
                await _fillingService.UpdateQuantityByProductAsync(disposalWarehouseId, productId, newQuantity);
            }

            // Снять ответственность по ResponsibilityFilling с склада утиля (в т.ч. часть, ушедшая на ЭКО)
            await _responsibilityFillingService.DecreaseProductResponsibilityAtWarehouseByQuantityAsync(disposalWarehouseId, productId, totalQty);

            await _context.SaveChangesAsync();
        }
    }
}
