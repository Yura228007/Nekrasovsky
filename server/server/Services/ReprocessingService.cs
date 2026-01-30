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
        private readonly IResponsibilityService _responsibilityService;
        private readonly IUserPermissionsService _userPermissionsService;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;

        public ReprocessingService(
            AppDbContext context,
            ILogger<ReprocessingService> logger,
            IResponsibilityService responsibilityService,
            IUserPermissionsService userPermissionsService,
            IUserService userService,
            IRoleService roleService)
        {
            _context = context;
            _logger = logger;
            _responsibilityService = responsibilityService;
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
                var hasProduct = output.ProductId.HasValue;
                if (hasMaterial == hasProduct)
                {
                    throw new InvalidOperationException("Each output must have either MaterialId or ProductId.");
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

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await ValidateSourceMaterialsAsync(request.WarehouseId, sourceMaterialIds);
                await ValidateSourceResponsibilitiesAsync(userId, canManage, sourceMaterialIds);

                var sourceTotals = sources
                    .GroupBy(s => s.MaterialId)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

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
                }

                foreach (var output in request.Outputs)
                {
                    if (output.MaterialId.HasValue)
                    {
                        await ApplyOutputForMaterialAsync(request.WarehouseId, output.MaterialId.Value, output);
                        await _responsibilityService.AssignMaterialAsync(output.MaterialId.Value, userId);
                    }
                    else if (output.ProductId.HasValue)
                    {
                        await ApplyOutputForProductAsync(request.WarehouseId, output.ProductId.Value, output);
                        await _responsibilityService.AssignProductAsync(output.ProductId.Value, userId);
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

                await ReleaseResponsibilitiesIfEmptyAsync(sourceMaterialIds);

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

        private async Task ValidateSourceResponsibilitiesAsync(int userId, bool canManage, List<int> sourceMaterialIds)
        {
            if (canManage)
            {
                return;
            }

            foreach (var materialId in sourceMaterialIds)
            {
                var isResponsible = await _responsibilityService.IsResponsibleForMaterialAsync(materialId, userId);
                if (!isResponsible)
                {
                    throw new InvalidOperationException("User is not responsible for this material.");
                }
            }
        }

        private async Task ReleaseResponsibilitiesIfEmptyAsync(List<int> materialIds)
        {
            foreach (var materialId in materialIds)
            {
                var totalRemaining = await _context.FillingWarehouses
                    .Where(fw => fw.MaterialId == materialId)
                    .SumAsync(fw => fw.Quantity);

                if (totalRemaining <= 0)
                {
                    await _responsibilityService.ReleaseMaterialAsync(materialId);
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

        private async Task ApplyOutputForProductAsync(int warehouseId, int productId, ReprocessingOutput output)
        {
            var filling = await _context.FillingWarehouses
                .FirstOrDefaultAsync(fw => fw.WarehouseId == warehouseId && fw.ProductId == productId);

            if (filling == null)
            {
                var product = await _context.Products.FirstAsync(p => p.Id == productId);
                filling = new FillingWarehouse
                {
                    WarehouseId = warehouseId,
                    ProductId = productId,
                    Quantity = 0,
                    MeasuringType = product.MeasuringUnit
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
