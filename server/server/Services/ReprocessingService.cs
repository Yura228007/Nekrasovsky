using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;
using System.Linq;

namespace server.Services
{
    public class ReprocessingService : IReprocessingService
    {
        private const string DisposalWarehouseType = "Утиль";

        private readonly AppDbContext _context;
        private readonly ILogger<ReprocessingService> _logger;
        private readonly IResponsibilityFillingService _responsibilityFillingService;
        private readonly IProductBatchService _productBatchService;
        private readonly IUserPermissionsService _userPermissionsService;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IFillingWarehouseService _fillingWarehouseService;
        private readonly IDisposalRequestService _disposalRequestService;

        public ReprocessingService(
            AppDbContext context,
            ILogger<ReprocessingService> logger,
            IResponsibilityFillingService responsibilityFillingService,
            IProductBatchService productBatchService,
            IUserPermissionsService userPermissionsService,
            IUserService userService,
            IRoleService roleService,
            IFillingWarehouseService fillingWarehouseService,
            IDisposalRequestService disposalRequestService)
        {
            _context = context;
            _logger = logger;
            _responsibilityFillingService = responsibilityFillingService;
            _productBatchService = productBatchService;
            _userPermissionsService = userPermissionsService;
            _userService = userService;
            _roleService = roleService;
            _fillingWarehouseService = fillingWarehouseService;
            _disposalRequestService = disposalRequestService;
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

            // Валидация количества (с учетом единиц измерения)
            ValidateQuantitiesBalance(sources, request.Outputs, request.DefectQuantity, request.RecyclingQuantity);

            var canManage = await HasManageResponsibilityAsync(userId);
            var sourceMaterialIds = sources.Select(s => s.MaterialId).Distinct().ToList();

            // Валидация outputs
            foreach (var output in request.Outputs)
            {
                var hasMaterial = output.MaterialId.HasValue;
                var hasNewMaterial = !string.IsNullOrWhiteSpace(output.NewMaterialCode) && 
                                     !string.IsNullOrWhiteSpace(output.NewMaterialName);
                var hasProduct = output.ProductId.HasValue;

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

            // Валидация складов утиля
            if (request.DefectQuantity > 0 && !request.DefectWarehouseId.HasValue)
            {
                throw new InvalidOperationException("DefectWarehouseId is required when DefectQuantity > 0");
            }

            if (request.RecyclingQuantity > 0 && !request.RecyclingWarehouseId.HasValue)
            {
                throw new InvalidOperationException("RecyclingWarehouseId is required when RecyclingQuantity > 0");
            }

            var sourceTotals = sources
                .GroupBy(s => s.MaterialId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

            // Вычисляем общее количество для списания (исходные - брак - переработка)
            var firstSourceMaterialId = sources[0].MaterialId;
            var firstSourceMaterial = await _context.Materials.FindAsync(firstSourceMaterialId);
            var firstSourceMeasuringUnit = firstSourceMaterial?.MeasuringUnit ?? sources[0].MeasuringType ?? "шт";

            // Количество для списания = исходные - брак - переработка
            var totalToDeduct = sourceTotals.Values.Sum() - request.DefectQuantity - request.RecyclingQuantity;
            if (totalToDeduct <= 0)
            {
                throw new InvalidOperationException("Сумма брака и переработки не может превышать количество исходных материалов.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await ValidateSourceMaterialsAsync(request.WarehouseId, sourceMaterialIds);
                
                // Валидация ответственности для количества, которое будет списано (без брака и переработки)
                var sourceTotalsForDeduction = new Dictionary<int, double>();
                foreach (var (materialId, totalQty) in sourceTotals)
                {
                    // Распределяем брак и переработку пропорционально (берем из первого материала)
                    var materialDefectQty = materialId == firstSourceMaterialId ? request.DefectQuantity : 0;
                    var materialRecyclingQty = materialId == firstSourceMaterialId ? request.RecyclingQuantity : 0;
                    sourceTotalsForDeduction[materialId] = totalQty - materialDefectQty - materialRecyclingQty;
                }
                await ValidateSourceResponsibilitiesAsync(userId, canManage, request.WarehouseId, sourceTotalsForDeduction);

                // 1) Списываем исходные материалы (только часть, которая идет в результаты, без брака и переработки)
                foreach (var (materialId, totalQty) in sourceTotals)
                {
                    var materialDefectQty = materialId == firstSourceMaterialId ? request.DefectQuantity : 0;
                    var materialRecyclingQty = materialId == firstSourceMaterialId ? request.RecyclingQuantity : 0;
                    var qtyToDeduct = totalQty - materialDefectQty - materialRecyclingQty;

                    if (qtyToDeduct <= 0) continue;

                    var sourceFilling = await _context.FillingWarehouses
                        .FirstOrDefaultAsync(fw => fw.WarehouseId == request.WarehouseId && fw.MaterialId == materialId);

                    if (sourceFilling == null)
                    {
                        throw new InvalidOperationException("Source material is not present in the selected warehouse.");
                    }

                    if (sourceFilling.Quantity < qtyToDeduct)
                    {
                        throw new InvalidOperationException("Source quantity exceeds available stock.");
                    }

                    sourceFilling.Quantity -= qtyToDeduct;

                    // Уменьшаем ответственность пользователя (только для части, которая идет в результаты)
                    var decreased = await _responsibilityFillingService.DecreaseMaterialResponsibilityAtWarehouseAsync(
                        request.WarehouseId, materialId, qtyToDeduct, userId);
                    if (!decreased)
                    {
                        throw new InvalidOperationException($"Недостаточно ответственности за материал (ID {materialId}) на складе.");
                    }
                }

                // 1.5) Списываем брак и переработку со склада (но ответственность НЕ уменьшаем - остается у создателя)
                if (request.DefectQuantity > 0)
                {
                    var defectFilling = await _context.FillingWarehouses
                        .FirstOrDefaultAsync(fw => fw.WarehouseId == request.WarehouseId && fw.MaterialId == firstSourceMaterialId);

                    if (defectFilling == null || defectFilling.Quantity < request.DefectQuantity)
                    {
                        throw new InvalidOperationException($"Недостаточно материала для брака: требуется {request.DefectQuantity}, доступно {defectFilling?.Quantity ?? 0}.");
                    }

                    defectFilling.Quantity -= request.DefectQuantity;

                    // Перемещаем на склад утиля, если указан
                    if (request.DefectWarehouseId.HasValue)
                    {
                        await MoveMaterialToDisposalWarehouseAsync(
                            request.WarehouseId, request.DefectWarehouseId.Value,
                            firstSourceMaterialId, request.DefectQuantity, firstSourceMeasuringUnit, userId);
                    }
                }

                if (request.RecyclingQuantity > 0)
                {
                    var recyclingFilling = await _context.FillingWarehouses
                        .FirstOrDefaultAsync(fw => fw.WarehouseId == request.WarehouseId && fw.MaterialId == firstSourceMaterialId);

                    if (recyclingFilling == null || recyclingFilling.Quantity < request.RecyclingQuantity)
                    {
                        throw new InvalidOperationException($"Недостаточно материала для переработки: требуется {request.RecyclingQuantity}, доступно {recyclingFilling?.Quantity ?? 0}.");
                    }

                    recyclingFilling.Quantity -= request.RecyclingQuantity;

                    // Перемещаем на склад утиля, если указан
                    if (request.RecyclingWarehouseId.HasValue)
                    {
                        await MoveMaterialToDisposalWarehouseAsync(
                            request.WarehouseId, request.RecyclingWarehouseId.Value,
                            firstSourceMaterialId, request.RecyclingQuantity, firstSourceMeasuringUnit, userId);
                    }
                }

                var productBatchesCreated = new List<(int BatchId, ReprocessingOutput Output)>();

                // 2) Создаем результаты (нормальные и ЭКО)
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
                            Description = output.NewMaterialDescription,
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
                        
                        // Для ЭКО продукции добавляем "ECO" в начало BatchNumber
                        var batchNumberPrefix = output.OutputType == ReprocessingOutputType.Eco ? "ECO-" : "";
                        var batch = new ProductBatch
                        {
                            ProductId = output.ProductId.Value,
                            WarehouseId = request.WarehouseId,
                            Quantity = output.Quantity,
                            MeasuringUnit = output.MeasuringType ?? product.MeasuringUnit,
                            CreatedByUserId = userId,
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true,
                            BatchNumber = $"{batchNumberPrefix}{code}-{timestamp}",
                            Note = request.Note // Сохраняем примечание
                        };
                        _context.ProductBatches.Add(batch);
                        await _context.SaveChangesAsync(); // Сохраняем, чтобы получить ID партии
                        productBatchesCreated.Add((batch.Id, output));
                        _logger.LogInformation("ProductBatch created from reprocessing: Batch {BatchId}, Product {ProductId}, Quantity {Quantity}, Type {OutputType}",
                            batch.Id, output.ProductId.Value, output.Quantity, output.OutputType);
                    }
                }

                // Обновляем FillingWarehouse для продукции
                var uniqueProductIds = productBatchesCreated.Select(x => x.Output.ProductId!.Value).Distinct().ToList();
                foreach (var productId in uniqueProductIds)
                {
                    await _productBatchService.UpdateFillingWarehouseForProductAsync(productId, request.WarehouseId);
                }
                await _context.SaveChangesAsync();

                // 3) Назначаем ответственность за результаты
                // Для материалов - как раньше
                foreach (var output in request.Outputs)
                {
                    if (output.MaterialId.HasValue)
                    {
                        await _responsibilityFillingService.AssignMaterialAtWarehouseAsync(
                            userId, request.WarehouseId, output.MaterialId.Value, output.Quantity, output.MeasuringType);
                    }
                }

                // Для продуктов - назначаем ответственность на партии
                foreach (var (batchId, output) in productBatchesCreated)
                {
                    var batch = await _context.ProductBatches.FindAsync(batchId);
                    if (batch != null)
                    {
                        await _responsibilityFillingService.AssignBatchResponsibilityAsync(
                            userId, batchId, output.Quantity, output.MeasuringType ?? batch.MeasuringUnit);
                        _logger.LogInformation("Batch responsibility assigned: User {UserId}, Batch {BatchId}, Quantity {Quantity}",
                            userId, batchId, output.Quantity);
                    }
                }

                // 4) Создаем запросы на утиль для брака и переработки (материал уже перемещен через MoveMaterialToDisposalWarehouseAsync)
                if (request.DefectQuantity > 0 && request.DefectWarehouseId.HasValue)
                {
                    var defectRequest = new DisposalRequest
                    {
                        FromUserId = userId,
                        FromWarehouseId = request.WarehouseId,
                        ToWarehouseId = request.DefectWarehouseId.Value,
                        MaterialId = firstSourceMaterialId,
                        Quantity = request.DefectQuantity,
                        MeasuringUnit = firstSourceMeasuringUnit,
                        RequestType = DisposalRequestType.Defect,
                        Status = DisposalRequestStatus.Pending,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.DisposalRequests.Add(defectRequest);
                    _logger.LogInformation("DisposalRequest created for defect: Material {MaterialId}, Quantity {Quantity}, Warehouse {WarehouseId}",
                        firstSourceMaterialId, request.DefectQuantity, request.DefectWarehouseId.Value);
                }

                if (request.RecyclingQuantity > 0 && request.RecyclingWarehouseId.HasValue)
                {
                    var recyclingRequest = new DisposalRequest
                    {
                        FromUserId = userId,
                        FromWarehouseId = request.WarehouseId,
                        ToWarehouseId = request.RecyclingWarehouseId.Value,
                        MaterialId = firstSourceMaterialId,
                        Quantity = request.RecyclingQuantity,
                        MeasuringUnit = firstSourceMeasuringUnit,
                        RequestType = DisposalRequestType.Recycling,
                        Status = DisposalRequestStatus.Pending,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.DisposalRequests.Add(recyclingRequest);
                    _logger.LogInformation("DisposalRequest created for recycling: Material {MaterialId}, Quantity {Quantity}, Warehouse {WarehouseId}",
                        firstSourceMaterialId, request.RecyclingQuantity, request.RecyclingWarehouseId.Value);
                }

                // 5) Сохраняем запись переработки
                var reprocessing = new Reprocessing
                {
                    UserId = userId,
                    WarehouseId = request.WarehouseId,
                    SourceMaterialId = sources[0].MaterialId,
                    SourceQuantity = sources[0].Quantity,
                    DefectQuantity = request.DefectQuantity,
                    MachineId = request.MachineId,
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

        private async Task ValidateSourceResponsibilitiesAsync(int userId, bool canManage, int warehouseId, Dictionary<int, double> sourceTotals)
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

        /// <summary>Добавляет количество материала на склад (создаёт или обновляет FillingWarehouse).</summary>
        private async Task AddMaterialQuantityToWarehouseAsync(int warehouseId, int materialId, double quantity, string? measuringType)
        {
            if (quantity <= 0) return;

            var filling = await _fillingWarehouseService.GetFillingByMaterialAsync(warehouseId, materialId);
            if (filling == null)
            {
                await _fillingWarehouseService.CreateFillingWarehouseAsync(new FillingWarehouse
                {
                    WarehouseId = warehouseId,
                    MaterialId = materialId,
                    Quantity = quantity,
                    MeasuringType = measuringType
                });
            }
            else
            {
                await _fillingWarehouseService.UpdateQuantityByMaterialAsync(
                    warehouseId, materialId, filling.Quantity + quantity);
            }
        }

        /// <summary>
        /// Конвертирует граммы в килограммы для сравнения количеств
        /// </summary>
        /// <summary>
        /// Перемещает материал на склад утиля без передачи ответственности (ответственность остается у создателя)
        /// </summary>
        /// <summary>
        /// Перемещает материал на склад утиля без передачи ответственности (ответственность остается у создателя)
        /// Примечание: материал уже списан со склада fromWarehouseId выше, здесь только добавляем на склад утиля
        /// </summary>
        private async Task MoveMaterialToDisposalWarehouseAsync(
            int fromWarehouseId, int toWarehouseId, int materialId, double quantity, string? measuringUnit, int userId)
        {
            // Добавляем материал на склад утиля
            var toFilling = await _context.FillingWarehouses
                .FirstOrDefaultAsync(fw => fw.WarehouseId == toWarehouseId && fw.MaterialId == materialId);

            if (toFilling == null)
            {
                var material = await _context.Materials.FindAsync(materialId);
                toFilling = new FillingWarehouse
                {
                    WarehouseId = toWarehouseId,
                    MaterialId = materialId,
                    Quantity = quantity,
                    MeasuringType = measuringUnit ?? material?.MeasuringUnit ?? "шт"
                };
                _context.FillingWarehouses.Add(toFilling);
            }
            else
            {
                toFilling.Quantity += quantity;
            }

            // Списываем ответственность с текущего склада
            await _responsibilityFillingService.DecreaseMaterialResponsibilityAtWarehouseAsync(
                fromWarehouseId, materialId, quantity, userId);

            // Добавляем ответственность на склад утиля (отдельной строкой)
            // Ответственность остается у того же пользователя (userId), но теперь на складе утиля
            await _responsibilityFillingService.AssignMaterialAtWarehouseAsync(
                userId, toWarehouseId, materialId, quantity, measuringUnit);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Material {MaterialId} moved to disposal warehouse {ToWarehouseId} from {FromWarehouseId}, quantity {Quantity}. Responsibility transferred to disposal warehouse for user {UserId}",
                materialId, toWarehouseId, fromWarehouseId, quantity, userId);
        }

        private static double ConvertToKilograms(double quantity, string? measuringUnit)
        {
            if (string.IsNullOrWhiteSpace(measuringUnit))
                return quantity;

            var unit = measuringUnit.Trim().ToLowerInvariant();
            if (unit == "г" || unit == "грамм" || unit == "граммы" || unit == "g" || unit == "gram" || unit == "grams")
            {
                return quantity / 1000.0; // граммы -> килограммы
            }
            else if (unit == "кг" || unit == "килограмм" || unit == "килограммы" || unit == "kg" || unit == "kilogram" || unit == "kilograms")
            {
                return quantity; // уже в килограммах
            }

            // Для других единиц измерения возвращаем как есть (шт, л, м и т.д.)
            return quantity;
        }

        /// <summary>
        /// Проверяет, является ли единица измерения весовой (кг, г)
        /// </summary>
        private static bool IsWeightUnit(string? measuringUnit)
        {
            if (string.IsNullOrWhiteSpace(measuringUnit))
                return false;

            var unit = measuringUnit.Trim().ToLowerInvariant();
            return unit == "г" || unit == "грамм" || unit == "граммы" || 
                   unit == "g" || unit == "gram" || unit == "grams" ||
                   unit == "кг" || unit == "килограмм" || unit == "килограммы" || 
                   unit == "kg" || unit == "kilogram" || unit == "kilograms";
        }

        /// <summary>
        /// Валидирует, что сумма исходных материалов равна сумме результатов (с учетом единиц измерения)
        /// Сравнивает все количества как числа, без учета единиц измерения (шт сравнивается с кг и т.д.)
        /// </summary>
        private static void ValidateQuantitiesBalance(List<ReprocessingSource> sources, List<ReprocessingOutput> outputs, 
            double defectQuantity, double recyclingQuantity)
        {
            if (sources.Count == 0)
                return;

            var firstSourceMeasuringUnit = sources.FirstOrDefault()?.MeasuringType;
            bool allSourcesAreWeight = sources.All(s => IsWeightUnit(s.MeasuringType));

            // Если исходные материалы в весовых единицах, конвертируем граммы в килограммы
            if (allSourcesAreWeight)
            {
                // Суммируем исходные материалы в килограммах (конвертируем граммы в кг)
                double totalSource = 0;
                foreach (var source in sources)
                {
                    totalSource += ConvertToKilograms(source.Quantity, source.MeasuringType);
                }

                // Суммируем все результаты как числа (шт, л и т.д. просто как числа, граммы конвертируем в кг)
                double totalOutput = 0;
                foreach (var output in outputs)
                {
                    if (IsWeightUnit(output.MeasuringType))
                    {
                        // Для весовых единиц конвертируем граммы в кг
                        totalOutput += ConvertToKilograms(output.Quantity, output.MeasuringType);
                    }
                    else
                    {
                        // Для невесовых единиц просто добавляем как число
                        totalOutput += output.Quantity;
                    }
                }

                // Добавляем брак и переработку (конвертируем граммы в кг, если нужно)
                totalOutput += ConvertToKilograms(defectQuantity, firstSourceMeasuringUnit);
                totalOutput += ConvertToKilograms(recyclingQuantity, firstSourceMeasuringUnit);

                // Сравниваем с небольшой погрешностью (0.001) для учета округления
                if (Math.Abs(totalSource - totalOutput) > 0.001)
                {
                    throw new InvalidOperationException(
                        $"Количество исходных материалов ({totalSource:F3}) не совпадает с количеством результатов ({totalOutput:F3}). " +
                        $"Разница: {Math.Abs(totalSource - totalOutput):F3}");
                }
            }
            // Если исходные материалы в невесовых единицах
            else
            {
                // Просто сравниваем все как числа
                double totalSource = sources.Sum(s => s.Quantity);
                double totalOutput = outputs.Sum(o => o.Quantity) + defectQuantity + recyclingQuantity;

                if (totalSource != totalOutput)
                {
                    throw new InvalidOperationException(
                        $"Количество исходных материалов ({totalSource} {sources.First().MeasuringType}) не совпадает с количеством результатов ({totalOutput}). " +
                        $"Разница: {Math.Abs(totalSource - totalOutput)}");
                }
            }
        }
    }
}
