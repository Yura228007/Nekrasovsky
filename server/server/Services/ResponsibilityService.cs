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
                // Treat legacy records with missing ReleasedAt as active.
                query = query.Where(r => r.IsActive || r.ReleasedAt == null);
            }

            return await query.OrderByDescending(r => r.AssignedAt).ToListAsync();
        }

        public async Task<Responsibility?> GetResponsibilityByMaterialAsync(int materialId, bool activeOnly = true)
        {
            var query = _context.Responsibilities.Where(r => r.MaterialId == materialId);
            if (activeOnly)
            {
                // Treat legacy records with missing ReleasedAt as active.
                query = query.Where(r => r.IsActive || r.ReleasedAt == null);
            }

            return await query.OrderByDescending(r => r.AssignedAt).FirstOrDefaultAsync();
        }

        public async Task<Responsibility?> GetResponsibilityByProductAsync(int productId, bool activeOnly = true)
        {
            var query = _context.Responsibilities.Where(r => r.ProductId == productId);
            if (activeOnly)
            {
                // Treat legacy records with missing ReleasedAt as active.
                query = query.Where(r => r.IsActive || r.ReleasedAt == null);
            }

            return await query.OrderByDescending(r => r.AssignedAt).FirstOrDefaultAsync();
        }

        public async Task<Responsibility> AssignMaterialAsync(int materialId, int userId, int? quantity = null, string? measuringUnit = null)
        {
            if (!await _context.Materials.AnyAsync(m => m.Id == materialId))
            {
                throw new KeyNotFoundException($"Material with ID {materialId} not found");
            }

            // Если единица измерения не указана, берем из материала
            if (string.IsNullOrWhiteSpace(measuringUnit))
            {
                var material = await _context.Materials.FirstOrDefaultAsync(m => m.Id == materialId);
                measuringUnit = material?.MeasuringUnit;
            }

            return await AssignAsync(userId, materialId, null, quantity, measuringUnit);
        }

        public async Task<Responsibility> AssignProductAsync(int productId, int userId, int? quantity = null, string? measuringUnit = null)
        {
            if (!await _context.Products.AnyAsync(p => p.Id == productId))
            {
                throw new KeyNotFoundException($"Product with ID {productId} not found");
            }

            // Если единица измерения не указана, берем из продукта
            if (string.IsNullOrWhiteSpace(measuringUnit))
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
                measuringUnit = product?.MeasuringUnit;
            }

            return await AssignAsync(userId, null, productId, quantity, measuringUnit);
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

        public async Task<bool> ReleaseProductAsync(int productId)
        {
            var responsibility = await _context.Responsibilities
                .Where(r => r.IsActive && r.ProductId == productId)
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

        public async Task<List<ResponsibilityAssignment>> GetActiveMaterialAssignmentsAsync()
        {
            var responsibilities = await _context.Responsibilities
                .Where(r => r.MaterialId.HasValue && r.IsActive)
                .OrderByDescending(r => r.AssignedAt)
                .ToListAsync();

            if (responsibilities.Count == 0)
            {
                return new List<ResponsibilityAssignment>();
            }

            var userMap = await _context.Users
                .ToDictionaryAsync(u => u.Id, u => $"{u.Surname} {u.Name}");

            // Возвращаем все активные назначения (не только первое), так как теперь поддерживаем несколько пользователей
            return responsibilities
                .Select(r => new ResponsibilityAssignment
                {
                    ItemId = r.MaterialId!.Value,
                    UserId = r.UserId,
                    UserName = userMap.TryGetValue(r.UserId, out var name) ? name : $"Пользователь #{r.UserId}",
                    Quantity = r.Quantity,
                    MeasuringUnit = r.MeasuringUnit
                })
                .ToList();
        }

        public async Task<List<ResponsibilityAssignment>> GetActiveProductAssignmentsAsync()
        {
            var responsibilities = await _context.Responsibilities
                .Where(r => r.ProductId.HasValue && r.IsActive)
                .OrderByDescending(r => r.AssignedAt)
                .ToListAsync();

            if (responsibilities.Count == 0)
            {
                return new List<ResponsibilityAssignment>();
            }

            var userMap = await _context.Users
                .ToDictionaryAsync(u => u.Id, u => $"{u.Surname} {u.Name}");

            // Возвращаем все активные назначения (не только первое), так как теперь поддерживаем несколько пользователей
            return responsibilities
                .Select(r => new ResponsibilityAssignment
                {
                    ItemId = r.ProductId!.Value,
                    UserId = r.UserId,
                    UserName = userMap.TryGetValue(r.UserId, out var name) ? name : $"Пользователь #{r.UserId}",
                    Quantity = r.Quantity,
                    MeasuringUnit = r.MeasuringUnit
                })
                .ToList();
        }

        private async Task<Responsibility> AssignAsync(int userId, int? materialId, int? productId, int? quantity = null, string? measuringUnit = null)
        {
            if ((materialId.HasValue && productId.HasValue) || (!materialId.HasValue && !productId.HasValue))
            {
                throw new InvalidOperationException("Either MaterialId or ProductId must be set, but not both.");
            }

            if (!await _context.Users.AnyAsync(u => u.Id == userId))
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            // Проверяем существующую ответственность для этого пользователя и материала/продукта
            var existingForUser = await _context.Responsibilities
                .Where(r => r.IsActive && r.MaterialId == materialId && r.ProductId == productId && r.UserId == userId)
                .OrderByDescending(r => r.AssignedAt)
                .FirstOrDefaultAsync();

            // Если у пользователя уже есть активная ответственность, обновляем количество или возвращаем существующую
            if (existingForUser != null)
            {
                if (quantity.HasValue)
                {
                    existingForUser.Quantity = quantity;
                    existingForUser.MeasuringUnit = measuringUnit;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Responsibility quantity updated: User {UserId}, Material {MaterialId}, Product {ProductId}, Quantity {Quantity}", 
                        userId, materialId, productId, quantity);
                }
                return existingForUser;
            }

            // Создаем новую ответственность (не закрываем существующие, так как теперь поддерживаем несколько пользователей)
            var responsibility = new Responsibility
            {
                UserId = userId,
                MaterialId = materialId,
                ProductId = productId,
                AssignedAt = DateTime.UtcNow,
                IsActive = true,
                Quantity = quantity,
                MeasuringUnit = measuringUnit
            };

            _context.Responsibilities.Add(responsibility);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Responsibility assigned: User {UserId}, Material {MaterialId}, Product {ProductId}, Quantity {Quantity}", 
                userId, materialId, productId, quantity);
            return responsibility;
        }

        public async Task<bool> DecreaseResponsibilityQuantityAsync(int materialId, int quantity, int? userId = null)
        {
            if (quantity <= 0)
            {
                return false;
            }

            // Получаем активные ответственности для материала, отсортированные по дате назначения
            var responsibilities = await _context.Responsibilities
                .Where(r => r.IsActive && 
                           r.MaterialId == materialId && 
                           (!userId.HasValue || r.UserId == userId.Value) &&
                           (r.Quantity == null || r.Quantity > 0))
                .OrderByDescending(r => r.AssignedAt)
                .ToListAsync();

            if (responsibilities.Count == 0)
            {
                return false;
            }

            int remainingQuantity = quantity;

            foreach (var responsibility in responsibilities)
            {
                if (remainingQuantity <= 0)
                {
                    break;
                }

                if (responsibility.Quantity == null)
                {
                    // Если количество не указано, значит ответственность за весь материал
                    // В этом случае не уменьшаем, так как это означает "ответственность за все"
                    continue;
                }

                int decreaseAmount = Math.Min(remainingQuantity, responsibility.Quantity.Value);
                responsibility.Quantity -= decreaseAmount;
                remainingQuantity -= decreaseAmount;

                // Если количество стало 0 или меньше, освобождаем ответственность
                if (responsibility.Quantity <= 0)
                {
                    responsibility.IsActive = false;
                    responsibility.ReleasedAt = DateTime.UtcNow;
                    responsibility.Quantity = 0;
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Decreased responsibility quantity for Material {MaterialId} by {Quantity}", materialId, quantity);
            return true;
        }

        public async Task<bool> DecreaseProductResponsibilityQuantityAsync(int productId, int quantity, int? userId = null)
        {
            if (quantity <= 0)
            {
                return false;
            }

            // Получаем активные ответственности для продукта
            var responsibilities = await _context.Responsibilities
                .Where(r => r.IsActive && 
                           r.ProductId == productId && 
                           (!userId.HasValue || r.UserId == userId.Value) &&
                           (r.Quantity == null || r.Quantity > 0))
                .OrderByDescending(r => r.AssignedAt)
                .ToListAsync();

            if (responsibilities.Count == 0)
            {
                return false;
            }

            int remainingQuantity = quantity;

            foreach (var responsibility in responsibilities)
            {
                if (remainingQuantity <= 0)
                {
                    break;
                }

                if (responsibility.Quantity == null)
                {
                    // Если количество не указано, значит ответственность за весь продукт
                    continue;
                }

                int decreaseAmount = Math.Min(remainingQuantity, responsibility.Quantity.Value);
                responsibility.Quantity -= decreaseAmount;
                remainingQuantity -= decreaseAmount;

                // Если количество стало 0 или меньше, освобождаем ответственность
                if (responsibility.Quantity <= 0)
                {
                    responsibility.IsActive = false;
                    responsibility.ReleasedAt = DateTime.UtcNow;
                    responsibility.Quantity = 0;
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Decreased responsibility quantity for Product {ProductId} by {Quantity}", productId, quantity);
            return true;
        }
    }
}
