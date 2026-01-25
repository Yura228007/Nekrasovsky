using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;

namespace server.Services
{
    public class ResponsibilityService : IResponsibilityService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ResponsibilityService> _logger;

        public ResponsibilityService(AppDbContext context, ILogger<ResponsibilityService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Responsibility>> GetResponsibilitiesByUserAsync(int userId, bool activeOnly = true)
        {
            var query = _context.Responsibilities.Where(r => r.UserId == userId);
            if (activeOnly)
            {
                query = query.Where(r => r.IsActive);
            }

            return await query.OrderByDescending(r => r.AssignedAt).ToListAsync();
        }

        public async Task<Responsibility?> GetResponsibilityByMaterialAsync(int materialId, bool activeOnly = true)
        {
            var query = _context.Responsibilities.Where(r => r.MaterialId == materialId);
            if (activeOnly)
            {
                query = query.Where(r => r.IsActive);
            }

            return await query.OrderByDescending(r => r.AssignedAt).FirstOrDefaultAsync();
        }

        public async Task<Responsibility?> GetResponsibilityByProductAsync(int productId, bool activeOnly = true)
        {
            var query = _context.Responsibilities.Where(r => r.ProductId == productId);
            if (activeOnly)
            {
                query = query.Where(r => r.IsActive);
            }

            return await query.OrderByDescending(r => r.AssignedAt).FirstOrDefaultAsync();
        }

        public async Task<Responsibility> AssignMaterialAsync(int materialId, int userId)
        {
            if (!await _context.Materials.AnyAsync(m => m.Id == materialId))
            {
                throw new KeyNotFoundException($"Material with ID {materialId} not found");
            }

            return await AssignAsync(userId, materialId, null);
        }

        public async Task<Responsibility> AssignProductAsync(int productId, int userId)
        {
            if (!await _context.Products.AnyAsync(p => p.Id == productId))
            {
                throw new KeyNotFoundException($"Product with ID {productId} not found");
            }

            return await AssignAsync(userId, null, productId);
        }

        public async Task<bool> IsResponsibleForMaterialAsync(int materialId, int userId)
        {
            return await _context.Responsibilities.AnyAsync(r =>
                r.IsActive && r.MaterialId == materialId && r.UserId == userId);
        }

        public async Task<bool> ReleaseMaterialAsync(int materialId)
        {
            var responsibility = await _context.Responsibilities
                .Where(r => r.IsActive && r.MaterialId == materialId)
                .OrderByDescending(r => r.AssignedAt)
                .FirstOrDefaultAsync();

            if (responsibility == null)
            {
                return false;
            }

            responsibility.IsActive = false;
            responsibility.ReleasedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> TransferAllAsync(int fromUserId, int toUserId)
        {
            if (fromUserId == toUserId)
            {
                return 0;
            }

            var active = await _context.Responsibilities
                .Where(r => r.UserId == fromUserId && r.IsActive)
                .ToListAsync();

            if (active.Count == 0)
            {
                return 0;
            }

            var now = DateTime.UtcNow;
            foreach (var responsibility in active)
            {
                responsibility.IsActive = false;
                responsibility.ReleasedAt = now;

                var newResponsibility = new Responsibility
                {
                    UserId = toUserId,
                    MaterialId = responsibility.MaterialId,
                    ProductId = responsibility.ProductId,
                    AssignedAt = now,
                    IsActive = true
                };

                _context.Responsibilities.Add(newResponsibility);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Transferred {Count} responsibilities from {FromUserId} to {ToUserId}", active.Count, fromUserId, toUserId);
            return active.Count;
        }

        public async Task<List<ResponsibilityStockItem>> GetResponsibilityStockAsync(int userId)
        {
            var responsibilities = await _context.Responsibilities
                .Where(r => r.UserId == userId && r.IsActive)
                .ToListAsync();

            var materialIds = responsibilities
                .Where(r => r.MaterialId.HasValue)
                .Select(r => r.MaterialId!.Value)
                .Distinct()
                .ToList();

            var productIds = responsibilities
                .Where(r => r.ProductId.HasValue)
                .Select(r => r.ProductId!.Value)
                .Distinct()
                .ToList();

            var materials = await _context.Materials
                .Where(m => materialIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id);

            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            var fillings = await _context.FillingWarehouses
                .Where(fw =>
                    (fw.MaterialId.HasValue && materialIds.Contains(fw.MaterialId.Value)) ||
                    (fw.ProductId.HasValue && productIds.Contains(fw.ProductId.Value)))
                .ToListAsync();

            var warehouses = await _context.Warehouses
                .ToDictionaryAsync(w => w.Id, w => w.Name);

            var result = new List<ResponsibilityStockItem>();

            foreach (var materialId in materialIds)
            {
                if (!materials.TryGetValue(materialId, out var material))
                {
                    continue;
                }

                var materialFillings = fillings.Where(fw => fw.MaterialId == materialId).ToList();
                var warehouseStocks = materialFillings.Select(fw => new ResponsibilityWarehouseStock
                {
                    WarehouseId = fw.WarehouseId,
                    WarehouseName = warehouses.TryGetValue(fw.WarehouseId, out var name) ? name : $"Склад #{fw.WarehouseId}",
                    Quantity = fw.Quantity,
                    MeasuringType = string.IsNullOrWhiteSpace(fw.MeasuringType) ? material.MeasuringUnit : fw.MeasuringType!
                }).ToList();

                result.Add(new ResponsibilityStockItem
                {
                    ItemType = "Material",
                    ItemId = material.Id,
                    Name = material.Name,
                    MeasuringUnit = material.MeasuringUnit,
                    TotalQuantity = materialFillings.Sum(fw => fw.Quantity),
                    Warehouses = warehouseStocks
                });
            }

            foreach (var productId in productIds)
            {
                if (!products.TryGetValue(productId, out var product))
                {
                    continue;
                }

                var productFillings = fillings.Where(fw => fw.ProductId == productId).ToList();
                var warehouseStocks = productFillings.Select(fw => new ResponsibilityWarehouseStock
                {
                    WarehouseId = fw.WarehouseId,
                    WarehouseName = warehouses.TryGetValue(fw.WarehouseId, out var name) ? name : $"Склад #{fw.WarehouseId}",
                    Quantity = fw.Quantity,
                    MeasuringType = string.IsNullOrWhiteSpace(fw.MeasuringType) ? product.MeasuringUnit : fw.MeasuringType!
                }).ToList();

                result.Add(new ResponsibilityStockItem
                {
                    ItemType = "Product",
                    ItemId = product.Id,
                    Name = product.Name,
                    MeasuringUnit = product.MeasuringUnit,
                    TotalQuantity = productFillings.Sum(fw => fw.Quantity),
                    Warehouses = warehouseStocks
                });
            }

            return result;
        }

        private async Task<Responsibility> AssignAsync(int userId, int? materialId, int? productId)
        {
            if ((materialId.HasValue && productId.HasValue) || (!materialId.HasValue && !productId.HasValue))
            {
                throw new InvalidOperationException("Either MaterialId or ProductId must be set, but not both.");
            }

            if (!await _context.Users.AnyAsync(u => u.Id == userId))
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            var existing = await _context.Responsibilities
                .Where(r => r.IsActive && r.MaterialId == materialId && r.ProductId == productId)
                .OrderByDescending(r => r.AssignedAt)
                .FirstOrDefaultAsync();

            if (existing != null && existing.UserId == userId)
            {
                return existing;
            }

            var now = DateTime.UtcNow;
            if (existing != null)
            {
                existing.IsActive = false;
                existing.ReleasedAt = now;
            }

            var responsibility = new Responsibility
            {
                UserId = userId,
                MaterialId = materialId,
                ProductId = productId,
                AssignedAt = now,
                IsActive = true
            };

            _context.Responsibilities.Add(responsibility);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Responsibility assigned: User {UserId}, Material {MaterialId}, Product {ProductId}", userId, materialId, productId);
            return responsibility;
        }
    }
}
