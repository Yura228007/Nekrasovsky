using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;

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
            if (request.SourceQuantity <= 0)
            {
                throw new InvalidOperationException("SourceQuantity must be greater than 0.");
            }

            if (request.Outputs == null || request.Outputs.Count == 0 || request.Outputs.Count > 2)
            {
                throw new InvalidOperationException("Outputs must contain 1 or 2 items.");
            }

            if (!await _context.Warehouses.AnyAsync(w => w.Id == request.WarehouseId))
            {
                throw new KeyNotFoundException($"Warehouse with ID {request.WarehouseId} not found");
            }

            if (!await _context.Materials.AnyAsync(m => m.Id == request.SourceMaterialId))
            {
                throw new KeyNotFoundException($"Material with ID {request.SourceMaterialId} not found");
            }

            var canManage = await HasManageResponsibilityAsync(userId);
            if (!canManage)
            {
                var isResponsible = await _responsibilityService.IsResponsibleForMaterialAsync(request.SourceMaterialId, userId);
                if (!isResponsible)
                {
                    throw new InvalidOperationException("User is not responsible for this material.");
                }
            }

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
                var sourceFilling = await _context.FillingWarehouses
                    .FirstOrDefaultAsync(fw => fw.WarehouseId == request.WarehouseId && fw.MaterialId == request.SourceMaterialId);

                if (sourceFilling == null)
                {
                    throw new InvalidOperationException("Source material is not present in the selected warehouse.");
                }

                if (sourceFilling.Quantity < request.SourceQuantity)
                {
                    throw new InvalidOperationException("Source quantity exceeds available stock.");
                }

                sourceFilling.Quantity -= request.SourceQuantity;

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
                    SourceMaterialId = request.SourceMaterialId,
                    SourceQuantity = request.SourceQuantity,
                    CreatedAt = DateTime.UtcNow,
                    Items = request.Outputs.Select(o => new ReprocessingItem
                    {
                        MaterialId = o.MaterialId,
                        ProductId = o.ProductId,
                        Quantity = o.Quantity,
                        MeasuringType = o.MeasuringType
                    }).ToList()
                };

                _context.Reprocessings.Add(reprocessing);
                await _context.SaveChangesAsync();

                if (sourceFilling.Quantity == 0)
                {
                    var totalRemaining = await _context.FillingWarehouses
                        .Where(fw => fw.MaterialId == request.SourceMaterialId)
                        .SumAsync(fw => fw.Quantity);

                    if (totalRemaining <= 0)
                    {
                        await _responsibilityService.ReleaseMaterialAsync(request.SourceMaterialId);
                    }
                }

                await transaction.CommitAsync();

                _logger.LogInformation("Reprocessing created: SourceMaterial {MaterialId}, User {UserId}", request.SourceMaterialId, userId);
                return reprocessing;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
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
