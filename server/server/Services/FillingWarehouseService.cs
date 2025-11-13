using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Data;

namespace server.Services
{
    public class FillingWarehouseService : IFillingWarehouseService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FillingWarehouseService> _logger;

        public FillingWarehouseService(AppDbContext context, ILogger<FillingWarehouseService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<FillingWarehouse>> GetAllFillingWarehousesAsync()
        {
            return await _context.FillingWarehouses.ToListAsync();
        }

        public async Task<FillingWarehouse?> GetFillingWarehouseAsync(int warehouseId, int materialId)
        {
            return await _context.FillingWarehouses
                .FirstOrDefaultAsync(fw => fw.WarehouseId == warehouseId && fw.MaterialId == materialId);
        }

        public async Task<IEnumerable<FillingWarehouse>> GetFillingByWarehouseAsync(int warehouseId)
        {
            return await _context.FillingWarehouses
                .Where(fw => fw.WarehouseId == warehouseId)
                .ToListAsync();
        }

        public async Task<IEnumerable<FillingWarehouse>> GetFillingByMaterialAsync(int materialId)
        {
            return await _context.FillingWarehouses
                .Where(fw => fw.MaterialId == materialId)
                .ToListAsync();
        }

        public async Task<FillingWarehouse> CreateFillingWarehouseAsync(FillingWarehouse filling)
        {
            // Check if warehouse exists
            if (!await _context.Warehouses.AnyAsync(w => w.Id == filling.WarehouseId))
            {
                throw new KeyNotFoundException($"Warehouse with ID {filling.WarehouseId} not found");
            }

            // Check if material exists
            if (!await _context.Materials.AnyAsync(m => m.Id == filling.MaterialId))
            {
                throw new KeyNotFoundException($"Material with ID {filling.MaterialId} not found");
            }

            // Check if filling already exists
            if (await _context.FillingWarehouses.AnyAsync(fw => 
                fw.WarehouseId == filling.WarehouseId && fw.MaterialId == filling.MaterialId))
            {
                throw new InvalidOperationException($"Filling for this warehouse and material already exists");
            }

            _context.FillingWarehouses.Add(filling);
            await _context.SaveChangesAsync();

            _logger.LogInformation("FillingWarehouse created: Warehouse {WarehouseId}, Material {MaterialId}, Quantity {Quantity}", 
                filling.WarehouseId, filling.MaterialId, filling.Quantity);
            return filling;
        }

        public async Task<FillingWarehouse> UpdateFillingWarehouseAsync(int warehouseId, int materialId, FillingWarehouse updatedFilling)
        {
            var filling = await GetFillingWarehouseAsync(warehouseId, materialId);
            if (filling == null)
            {
                throw new KeyNotFoundException($"FillingWarehouse not found");
            }

            // If keys are being changed, we need to delete old and create new
            if (filling.WarehouseId != updatedFilling.WarehouseId || filling.MaterialId != updatedFilling.MaterialId)
            {
                _context.FillingWarehouses.Remove(filling);
                await _context.SaveChangesAsync();
                return await CreateFillingWarehouseAsync(updatedFilling);
            }

            filling.Quantity = updatedFilling.Quantity;
            filling.MeasuringType = updatedFilling.MeasuringType;

            await _context.SaveChangesAsync();

            _logger.LogInformation("FillingWarehouse updated: Warehouse {WarehouseId}, Material {MaterialId}", 
                warehouseId, materialId);
            return filling;
        }

        public async Task<bool> DeleteFillingWarehouseAsync(int warehouseId, int materialId)
        {
            var filling = await GetFillingWarehouseAsync(warehouseId, materialId);
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

        public async Task<FillingWarehouse> UpdateQuantityAsync(int warehouseId, int materialId, int quantity)
        {
            var filling = await GetFillingWarehouseAsync(warehouseId, materialId);
            if (filling == null)
            {
                throw new KeyNotFoundException($"FillingWarehouse not found");
            }

            filling.Quantity = quantity;
            await _context.SaveChangesAsync();

            _logger.LogInformation("FillingWarehouse quantity updated: Warehouse {WarehouseId}, Material {MaterialId}, New Quantity {Quantity}", 
                warehouseId, materialId, quantity);
            return filling;
        }

        public async Task<IEnumerable<FillingWarehouse>> GetWarehouseStockAsync(int warehouseId)
        {
            return await _context.FillingWarehouses
                .Where(fw => fw.WarehouseId == warehouseId)
                .ToListAsync();
        }
    }
}

