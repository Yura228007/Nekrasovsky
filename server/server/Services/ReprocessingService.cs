using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;
using System.Linq;

namespace server.Services
{
    public class ReprocessingService : IReprocessingService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ReprocessingService> _logger;
        private readonly IResponsibilityFillingService _responsibilityFillingService;
        private readonly IProductBatchService _productBatchService;
        private readonly IUserPermissionsService _userPermissionsService;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;

        public ReprocessingService(
            AppDbContext context,
            ILogger<ReprocessingService> logger,
            IResponsibilityFillingService responsibilityFillingService,
            IProductBatchService productBatchService,
            IUserPermissionsService userPermissionsService,
            IUserService userService,
            IRoleService roleService)
        {
            _context = context;
            _logger = logger;
            _responsibilityFillingService = responsibilityFillingService;
            _productBatchService = productBatchService;
            _userPermissionsService = userPermissionsService;
            _userService = userService;
            _roleService = roleService;
        }

        public async Task<Reprocessing> CreateReprocessingAsync(ReprocessingCreateRequest request, int userId)
        {
            var sources = NormalizeSources(request);
            if (sources.Count == 0)
            {
                throw new InvalidOperationException("At least one source material is required.");
            }

            if (request.Outputs == null || request.Outputs.Count == 0)
            {
                throw new InvalidOperationException("Outputs must contain at least 1 item.");
            }

            if (!await _context.Warehouses.AnyAsync(w => w.Id == request.WarehouseId))
            {
                throw new KeyNotFoundException($"Warehouse with ID {request.WarehouseId} not found");
            }

            var canManage = await HasManageResponsibilityAsync(userId);
            var sourceMaterialIds = sources.Select(s => s.MaterialId).Distinct().ToList();

            foreach (var output in request.Outputs)
            {
                var hasMaterial = output.MaterialId.HasValue;
                var hasNewMaterial = !string.IsNullOrWhiteSpace(output.NewMaterialCode) && 
                                     !string.IsNullOrWhiteSpace(output.NewMaterialName);
                var hasProduct = output.ProductId.HasValue;

                // Один из вариантов должен быть выбран
                var optionsCount = (hasMaterial ? 1 : 0) + (hasNewMaterial ? 1 : 0) + (hasProduct ? 1 : 0);
                if (optionsCount != 1)
                {
                    throw new InvalidOperationException("Each output must have either MaterialId, NewMaterial (Code+Name), or ProductId.");
                }

                if (output.Quantity <= 0)
                {
                    throw new InvalidOperationException("Output quantity must be greater than 0.");
                }

                if (hasMaterial && !await _context.Materials.AnyAsync(m => m.Id == output.MaterialId))
                {
                    throw new KeyNotFoundException($"Material with ID {output.MaterialId} not found");
                }

                if (hasProduct && !await _context.Products.AnyAsync(p => p.Id == output.ProductId))
                {
                    throw new KeyNotFoundException($"Product with ID {output.ProductId} not found");
                }
            }

            var sourceTotals = sources
                .GroupBy(s => s.MaterialId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await ValidateSourceMaterialsAsync(request.WarehouseId, sourceMaterialIds);
                await ValidateSourceResponsibilitiesAsync(userId, canManage, request.WarehouseId, sourceTotals);

                foreach (var sourceTotal in sourceTotals)
                {
                    var sourceFilling = await _context.FillingWarehouses
                        .FirstOrDefaultAsync(fw => fw.WarehouseId == request.WarehouseId && fw.MaterialId == sourceTotal.Key);

                    if (sourceFilling == null)
                    {
                        throw new InvalidOperationException("Source material is not present in the selected warehouse.");
                    }

                    if (sourceFilling.Quantity < sourceTotal.Value)
                    {
                        throw new InvalidOperationException("Source quantity exceeds available stock.");
                    }

                    sourceFilling.Quantity -= sourceTotal.Value;

                    // Уменьшаем ответственность пользователя за материал на складе (ResponsibilityFilling)
                    var decreased = await _responsibilityFillingService.DecreaseMaterialResponsibilityAtWarehouseAsync(
                        request.WarehouseId, sourceTotal.Key, sourceTotal.Value, userId);
                    if (!decreased)
                    {
                        throw new InvalidOperationException($"Недостаточно ответственности за материал (ID {sourceTotal.Key}) на складе.");
                    }
                }

                var productBatchesCreated = new List<(int ProductId, int WarehouseId)>();

                // 1) Обновляем остатки на складе (материалы и партии) — без назначения ответственности
                foreach (var output in request.Outputs)
                {
                    if (output.MaterialId.HasValue)
                    {
                        await ApplyOutputForMaterialAsync(request.WarehouseId, output.MaterialId.Value, output);
                    }
                    else if (!string.IsNullOrWhiteSpace(output.NewMaterialCode) &&
                             !string.IsNullOrWhiteSpace(output.NewMaterialName))
                    {
                        var newMaterial = new Material
                        {
                            Name = output.NewMaterialName,
                            Code = output.NewMaterialCode,
                            MeasuringUnit = output.MeasuringType ?? "шт",
                            IsActive = true
                        };
                        _context.Materials.Add(newMaterial);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("New material created during reprocessing: {MaterialId} - {MaterialName} [{Code}]",
                            newMaterial.Id, newMaterial.Name, newMaterial.Code);
                        output.MaterialId = newMaterial.Id;
                        await ApplyOutputForMaterialAsync(request.WarehouseId, newMaterial.Id, output);
                    }
                    else if (output.ProductId.HasValue && output.ProductId.Value > 0)
                    {
                        var product = await _context.Products.FirstAsync(p => p.Id == output.ProductId.Value);
                        var code = product.Code ?? "PROD";
                        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                        var batch = new ProductBatch
                        {
                            ProductId = output.ProductId.Value,
                            WarehouseId = request.WarehouseId,
                            Quantity = output.Quantity,
                            MeasuringUnit = output.MeasuringType ?? product.MeasuringUnit,
                            CreatedByUserId = userId,
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true,
                            BatchNumber = $"{code}-{timestamp}"
                        };
                        _context.ProductBatches.Add(batch);
                        productBatchesCreated.Add((output.ProductId.Value, request.WarehouseId));
                        _logger.LogInformation("ProductBatch created from reprocessing: Product {ProductId}, Quantity {Quantity}", output.ProductId.Value, output.Quantity);
                    }
                }

                // Сохраняем изменения остатков и партий, чтобы Assign видел актуальный FillingWarehouse
                await _context.SaveChangesAsync();

                // Обновляем FillingWarehouse для продукции (сумма по партиям) до назначения ответственности
                foreach (var (productId, warehouseId) in productBatchesCreated)
                {
                    await _productBatchService.UpdateFillingWarehouseForProductAsync(productId, warehouseId);
                }
                await _context.SaveChangesAsync();

                // 2) Назначаем ответственность — наполнение уже обновлено, ошибки «total responsible > stock» не должно быть
                foreach (var output in request.Outputs)
                {
                    if (output.MaterialId.HasValue)
                    {
                        await _responsibilityFillingService.AssignMaterialAtWarehouseAsync(
                            userId, request.WarehouseId, output.MaterialId.Value, output.Quantity, output.MeasuringType);
                    }
                    else if (output.ProductId.HasValue && output.ProductId.Value > 0)
                    {
                        await _responsibilityFillingService.AssignProductAtWarehouseAsync(
                            userId, request.WarehouseId, output.ProductId.Value, output.Quantity, output.MeasuringType);
                    }
                }

                var reprocessing = new Reprocessing
                {
                    UserId = userId,
                    WarehouseId = request.WarehouseId,
                    SourceMaterialId = sources[0].MaterialId,
                    SourceQuantity = sources[0].Quantity,
                    CreatedAt = DateTime.UtcNow,
                    Items = request.Outputs.Select(o => new ReprocessingItem
                    {
                        MaterialId = o.MaterialId,
                        ProductId = o.ProductId,
                        Quantity = o.Quantity,
                        MeasuringType = o.MeasuringType
                    }).ToList(),
                    Sources = sources.Select(s => new ReprocessingSourceItem
                    {
                        MaterialId = s.MaterialId,
                        Quantity = s.Quantity,
                        MeasuringType = s.MeasuringType
                    }).ToList()
                };

                _context.Reprocessings.Add(reprocessing);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Reprocessing created: SourceMaterials {MaterialIds}, User {UserId}", string.Join(",", sourceMaterialIds), userId);
                return reprocessing;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static List<ReprocessingSource> NormalizeSources(ReprocessingCreateRequest request)
        {
            var sources = request.Sources?.Where(s => s.MaterialId > 0 && s.Quantity > 0).ToList()
                          ?? new List<ReprocessingSource>();

            return sources;
        }

        private async Task ValidateSourceMaterialsAsync(int warehouseId, List<int> sourceMaterialIds)
        {
            var materialIds = await _context.Materials
                .Where(m => sourceMaterialIds.Contains(m.Id))
                .Select(m => m.Id)
                .ToListAsync();

            if (materialIds.Count != sourceMaterialIds.Count)
            {
                var missing = sourceMaterialIds.Except(materialIds).FirstOrDefault();
                throw new KeyNotFoundException($"Material with ID {missing} not found");
            }

            var warehouseExists = await _context.Warehouses.AnyAsync(w => w.Id == warehouseId);
            if (!warehouseExists)
            {
                throw new KeyNotFoundException($"Warehouse with ID {warehouseId} not found");
            }
        }

        private async Task ValidateSourceResponsibilitiesAsync(int userId, bool canManage, int warehouseId, Dictionary<int, int> sourceTotals)
        {
            if (canManage)
            {
                return;
            }

            foreach (var (materialId, requiredQty) in sourceTotals)
            {
                var userQty = await _responsibilityFillingService.GetUserResponsibleQuantityAtWarehouseAsync(userId, warehouseId, materialId);
                if (userQty < requiredQty)
                {
                    throw new InvalidOperationException($"Недостаточно ответственности за материал (ID {materialId}): требуется {requiredQty}, под ответственностью {userQty}.");
                }
            }
        }

        private async Task ApplyOutputForMaterialAsync(int warehouseId, int materialId, ReprocessingOutput output)
        {
            var filling = await _context.FillingWarehouses
                .FirstOrDefaultAsync(fw => fw.WarehouseId == warehouseId && fw.MaterialId == materialId);

            if (filling == null)
            {
                var material = await _context.Materials.FirstAsync(m => m.Id == materialId);
                filling = new FillingWarehouse
                {
                    WarehouseId = warehouseId,
                    MaterialId = materialId,
                    Quantity = 0,
                    MeasuringType = material.MeasuringUnit
                };
                _context.FillingWarehouses.Add(filling);
            }

            filling.Quantity += output.Quantity;
            if (!string.IsNullOrWhiteSpace(output.MeasuringType))
            {
                filling.MeasuringType = output.MeasuringType;
            }
        }

        private async Task<bool> HasManageResponsibilityAsync(int userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            if (user.Login.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (user.RoleId.HasValue)
            {
                var role = await _roleService.GetRoleByIdAsync(user.RoleId.Value);
                if (role != null && (role.Code == "Owner" || role.Code == "Admin"))
                {
                    return true;
                }

                var rolePermissions = await _roleService.GetRolePermissionsAsync(user.RoleId.Value);
                if (rolePermissions.Any(p => p.Code == "ManageResponsibility"))
                {
                    return true;
                }
            }

            return await _userPermissionsService.HasPermissionAsync(userId, "ManageResponsibility");
        }
    }
}
