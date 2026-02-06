using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Data;

namespace server.Services
{
    public class FillingWarehouseService : IFillingWarehouseService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FillingWarehouseService> _logger;
        private readonly IResponsibilityFillingService _responsibilityFillingService;

        public FillingWarehouseService(AppDbContext context, ILogger<FillingWarehouseService> logger,
            IResponsibilityFillingService responsibilityFillingService)
        {
            _context = context;
            _logger = logger;
            _responsibilityFillingService = responsibilityFillingService;
        }

        public async Task<IEnumerable<FillingWarehouse>> GetAllFillingWarehousesAsync()
        {
            return await _context.FillingWarehouses.ToListAsync();
        }

        public async Task<FillingWarehouse?> GetFillingByMaterialAsync(int warehouseId, int materialId)
        {
            return await _context.FillingWarehouses
                .FirstOrDefaultAsync(fw => fw.WarehouseId == warehouseId && fw.MaterialId == materialId);
        }

        public async Task<FillingWarehouse?> GetFillingByProductAsync(int warehouseId, int productId)
        {
            return await _context.FillingWarehouses
                .FirstOrDefaultAsync(fw => fw.WarehouseId == warehouseId && fw.ProductId == productId);
        }

        public async Task<IEnumerable<FillingWarehouse>> GetFillingByWarehouseAsync(int warehouseId)
        {
            return await _context.FillingWarehouses
                .Where(fw => fw.WarehouseId == warehouseId)
                .ToListAsync();
        }

        public async Task<IEnumerable<FillingWarehouse>> GetFillingsByMaterialAsync(int materialId)
        {
            return await _context.FillingWarehouses
                .Where(fw => fw.MaterialId == materialId)
                .ToListAsync();
        }

        public async Task<IEnumerable<FillingWarehouse>> GetFillingsByProductAsync(int productId)
        {
            return await _context.FillingWarehouses
                .Where(fw => fw.ProductId == productId)
                .ToListAsync();
        }

        public async Task<FillingWarehouse> CreateFillingWarehouseAsync(FillingWarehouse filling)
        {
            if (!IsValidXor(filling.MaterialId, filling.ProductId))
            {
                throw new InvalidOperationException("Either MaterialId or ProductId must be set, but not both.");
            }

            // Check if warehouse exists
            if (!await _context.Warehouses.AnyAsync(w => w.Id == filling.WarehouseId))
            {
                throw new KeyNotFoundException($"Warehouse with ID {filling.WarehouseId} not found");
            }

            if (filling.MaterialId.HasValue)
            {
                if (!await _context.Materials.AnyAsync(m => m.Id == filling.MaterialId.Value))
                {
                    throw new KeyNotFoundException($"Material with ID {filling.MaterialId} not found");
                }

                if (await _context.FillingWarehouses.AnyAsync(fw =>
                    fw.WarehouseId == filling.WarehouseId && fw.MaterialId == filling.MaterialId))
                {
                    throw new InvalidOperationException("Filling for this warehouse and material already exists");
                }
            }
            else if (filling.ProductId.HasValue)
            {
                if (!await _context.Products.AnyAsync(p => p.Id == filling.ProductId.Value))
                {
                    throw new KeyNotFoundException($"Product with ID {filling.ProductId} not found");
                }

                if (await _context.FillingWarehouses.AnyAsync(fw =>
                    fw.WarehouseId == filling.WarehouseId && fw.ProductId == filling.ProductId))
                {
                    throw new InvalidOperationException("Filling for this warehouse and product already exists");
                }
            }

            _context.FillingWarehouses.Add(filling);
            await _context.SaveChangesAsync();

            _logger.LogInformation("FillingWarehouse created: Warehouse {WarehouseId}, Material {MaterialId}, Product {ProductId}, Quantity {Quantity}",
                filling.WarehouseId, filling.MaterialId, filling.ProductId, filling.Quantity);
            return filling;
        }

        public async Task<FillingWarehouse> UpdateFillingWarehouseByMaterialAsync(int warehouseId, int materialId, FillingWarehouse updatedFilling)
        {
            if (!IsValidXor(updatedFilling.MaterialId, updatedFilling.ProductId))
            {
                throw new InvalidOperationException("Either MaterialId or ProductId must be set, but not both.");
            }

            if (updatedFilling.WarehouseId != warehouseId)
            {
                throw new InvalidOperationException("WarehouseId cannot be changed in this update.");
            }

            if (updatedFilling.MaterialId != materialId || updatedFilling.ProductId != null)
            {
                throw new InvalidOperationException("Material update must target the same MaterialId and ProductId must be null.");
            }

            var filling = await GetFillingByMaterialAsync(warehouseId, materialId);
            if (filling == null)
            {
                throw new KeyNotFoundException($"FillingWarehouse not found");
            }

            filling.Quantity = updatedFilling.Quantity;
            filling.MeasuringType = updatedFilling.MeasuringType;

            await _context.SaveChangesAsync();

            _logger.LogInformation("FillingWarehouse updated: Warehouse {WarehouseId}, Material {MaterialId}", 
                warehouseId, materialId);
            return filling;
        }

        public async Task<FillingWarehouse> UpdateFillingWarehouseByProductAsync(int warehouseId, int productId, FillingWarehouse updatedFilling)
        {
            if (!IsValidXor(updatedFilling.MaterialId, updatedFilling.ProductId))
            {
                throw new InvalidOperationException("Either MaterialId or ProductId must be set, but not both.");
            }

            if (updatedFilling.WarehouseId != warehouseId)
            {
                throw new InvalidOperationException("WarehouseId cannot be changed in this update.");
            }

            if (updatedFilling.ProductId != productId || updatedFilling.MaterialId != null)
            {
                throw new InvalidOperationException("Product update must target the same ProductId and MaterialId must be null.");
            }

            var filling = await GetFillingByProductAsync(warehouseId, productId);
            if (filling == null)
            {
                throw new KeyNotFoundException("FillingWarehouse not found");
            }

            filling.Quantity = updatedFilling.Quantity;
            filling.MeasuringType = updatedFilling.MeasuringType;

            await _context.SaveChangesAsync();

            _logger.LogInformation("FillingWarehouse updated: Warehouse {WarehouseId}, Product {ProductId}",
                warehouseId, productId);
            return filling;
        }

        public async Task<bool> DeleteFillingWarehouseByMaterialAsync(int warehouseId, int materialId)
        {
            var filling = await GetFillingByMaterialAsync(warehouseId, materialId);
            if (filling == null)
            {
                return false;
            }

            _context.FillingWarehouses.Remove(filling);
            await _context.SaveChangesAsync();

            _logger.LogInformation("FillingWarehouse deleted: Warehouse {WarehouseId}, Material {MaterialId}",
                warehouseId, materialId);
            return true;
        }

        public async Task<bool> DeleteFillingWarehouseByProductAsync(int warehouseId, int productId)
        {
            var filling = await GetFillingByProductAsync(warehouseId, productId);
            if (filling == null)
            {
                return false;
            }

            _context.FillingWarehouses.Remove(filling);
            await _context.SaveChangesAsync();

            _logger.LogInformation("FillingWarehouse deleted: Warehouse {WarehouseId}, Product {ProductId}",
                warehouseId, productId);
            return true;
        }

        public async Task<FillingWarehouse> UpdateQuantityByMaterialAsync(int warehouseId, int materialId, double quantity)
        {
            var filling = await GetFillingByMaterialAsync(warehouseId, materialId);
            if (filling == null)
            {
                throw new KeyNotFoundException($"FillingWarehouse not found");
            }

            filling.Quantity = quantity;
            await _context.SaveChangesAsync();

            var responsibilityFillings = await _responsibilityFillingService.GetByWarehouseAndMaterialAsync(warehouseId, materialId);
            var totalResponsible = responsibilityFillings.Where(rf => rf.IsActive).Sum(rf => rf.Quantity);
            if (totalResponsible > quantity)
            {
                var toDecrease = totalResponsible - quantity;
                await _responsibilityFillingService.DecreaseMaterialResponsibilityAtWarehouseByQuantityAsync(warehouseId, materialId, toDecrease);
                _logger.LogInformation("ResponsibilityFilling decreased for Warehouse {WarehouseId}, Material {MaterialId} by {Quantity} (new stock {Stock})",
                    warehouseId, materialId, toDecrease, quantity);
            }

            _logger.LogInformation("FillingWarehouse quantity updated: Warehouse {WarehouseId}, Material {MaterialId}, New Quantity {Quantity}",
                warehouseId, materialId, quantity);
            return filling;
        }

        public async Task<FillingWarehouse> UpdateQuantityByProductAsync(int warehouseId, int productId, double quantity)
        {
            var filling = await GetFillingByProductAsync(warehouseId, productId);
            if (filling == null)
            {
                throw new KeyNotFoundException("FillingWarehouse not found");
            }

            filling.Quantity = quantity;
            await _context.SaveChangesAsync();

            var responsibilityFillings = await _responsibilityFillingService.GetByWarehouseAndProductAsync(warehouseId, productId);
            var totalResponsible = responsibilityFillings.Where(rf => rf.IsActive).Sum(rf => rf.Quantity);
            if (totalResponsible > quantity)
            {
                var toDecrease = totalResponsible - quantity;
                await _responsibilityFillingService.DecreaseProductResponsibilityAtWarehouseByQuantityAsync(warehouseId, productId, toDecrease);
                _logger.LogInformation("ResponsibilityFilling decreased for Warehouse {WarehouseId}, Product {ProductId} by {Quantity} (new stock {Stock})",
                    warehouseId, productId, toDecrease, quantity);
            }

            _logger.LogInformation("FillingWarehouse quantity updated: Warehouse {WarehouseId}, Product {ProductId}, New Quantity {Quantity}",
                warehouseId, productId, quantity);
            return filling;
        }

        public async Task<IEnumerable<FillingWarehouse>> GetWarehouseStockAsync(int warehouseId)
        {
            return await _context.FillingWarehouses
                .Where(fw => fw.WarehouseId == warehouseId)
                .ToListAsync();
        }

        private static bool IsValidXor(int? materialId, int? productId)
        {
            return (materialId.HasValue && !productId.HasValue) || (!materialId.HasValue && productId.HasValue);
        }
    }
}

