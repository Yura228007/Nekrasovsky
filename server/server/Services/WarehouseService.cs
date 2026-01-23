using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Data;

namespace server.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<WarehouseService> _logger;

        public WarehouseService(AppDbContext context, ILogger<WarehouseService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Warehouse>> GetAllWarehousesAsync()
        {
            return await _context.Warehouses.ToListAsync();
        }

        public async Task<Warehouse?> GetWarehouseByIdAsync(int id)
        {
            return await _context.Warehouses.FindAsync(id);
        }

        public async Task<IEnumerable<Warehouse>> GetWarehousesByTypeAsync(string type)
        {
            return await _context.Warehouses
                .Where(w => w.Type == type)
                .ToListAsync();
        }

        public async Task<IEnumerable<Warehouse>> SearchWarehousesAsync(string? name, string? type, bool? isActive = null, string? sortBy = null)
        {
            var query = _context.Warehouses.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(w => EF.Functions.ILike(w.Name, $"%{name}%"));

            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(w => w.Type == type);

            if (isActive.HasValue)
                query = query.Where(w => w.IsActive == isActive.Value);

            // Сортировка
            query = sortBy?.ToLower() switch
            {
                "name" => query.OrderBy(w => w.Name),
                "name_desc" => query.OrderByDescending(w => w.Name),
                "type" => query.OrderBy(w => w.Type),
                "type_desc" => query.OrderByDescending(w => w.Type),
                _ => query.OrderBy(w => w.Name) // По умолчанию
            };

            return await query.ToListAsync();
        }

        public async Task<Warehouse> CreateWarehouseAsync(Warehouse warehouse)
        {
            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Warehouse created with ID: {WarehouseId}, Name: {Name}", warehouse.Id, warehouse.Name);
            return warehouse;
        }

        public async Task<Warehouse> UpdateWarehouseAsync(int id, Warehouse updatedWarehouse)
        {
            var warehouse = await _context.Warehouses.FindAsync(id);
            if (warehouse == null)
            {
                throw new KeyNotFoundException($"Warehouse with ID {id} not found");
            }

            warehouse.Name = updatedWarehouse.Name;
            warehouse.Type = updatedWarehouse.Type;
            warehouse.IsActive = updatedWarehouse.IsActive;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Warehouse updated with ID: {WarehouseId}", warehouse.Id);
            return warehouse;
        }

        public async Task<bool> DeleteWarehouseAsync(int id)
        {
            var warehouse = await _context.Warehouses.FindAsync(id);
            if (warehouse == null)
            {
                return false;
            }

            _context.Warehouses.Remove(warehouse);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Warehouse deleted with ID: {WarehouseId}", id);
            return true;
        }

        public async Task<IEnumerable<AccessibleMovement>> GetWarehouseMovementsAsync(int warehouseId)
        {
            return await _context.AccessibleMovements
                .Where(am => am.FromWarehouseId == warehouseId || am.ToWarehouseId == warehouseId)
                .ToListAsync();
        }

        public async Task<IEnumerable<PartRequest>> GetWarehouseRequestsAsync(int warehouseId)
        {
            return await _context.PartRequests
                .Where(pr => pr.FromWarehouseId == warehouseId || pr.ToWarehouseId == warehouseId)
                .ToListAsync();
        }

        public async Task<Warehouse> StopWarehouseAsync(int id)
        {
            var warehouse = await _context.Warehouses.FindAsync(id);
            if (warehouse == null)
            {
                throw new KeyNotFoundException($"Warehouse with ID {id} not found");
            }

            if (!warehouse.IsActive)
            {
                throw new InvalidOperationException($"Warehouse with ID {id} is already stopped");
            }

            warehouse.IsActive = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Warehouse stopped with ID: {WarehouseId}, Name: {Name}", warehouse.Id, warehouse.Name);
            return warehouse;
        }

        public async Task<Warehouse> StartWarehouseAsync(int id)
        {
            var warehouse = await _context.Warehouses.FindAsync(id);
            if (warehouse == null)
            {
                throw new KeyNotFoundException($"Warehouse with ID {id} not found");
            }

            if (warehouse.IsActive)
            {
                throw new InvalidOperationException($"Warehouse with ID {id} is already active");
            }

            warehouse.IsActive = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Warehouse started with ID: {WarehouseId}, Name: {Name}", warehouse.Id, warehouse.Name);
            return warehouse;
        }
    }
}

