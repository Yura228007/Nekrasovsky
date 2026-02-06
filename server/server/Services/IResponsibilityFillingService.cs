using server.Models;

namespace server.Services;

public interface IResponsibilityFillingService
{
    /// <summary>
    /// Назначить ответственность за количество материала на складе.
    /// Проверяет: сумма по ResponsibilityFilling + quantity не превышает FillingWarehouse.Quantity.
    /// </summary>
    Task<ResponsibilityFilling> AssignMaterialAtWarehouseAsync(int userId, int warehouseId, int materialId, double quantity, string? measuringUnit = null);

    /// <summary>
    /// Назначить ответственность за количество продукта на складе.
    /// </summary>
    Task<ResponsibilityFilling> AssignProductAtWarehouseAsync(int userId, int warehouseId, int productId, double quantity, string? measuringUnit = null);

    /// <summary>
    /// Уменьшить ответственность пользователя за материал на складе (при выдаче/перемещении).
    /// Уменьшает записи ResponsibilityFilling по дате AssignedAt (сначала самые новые).
    /// </summary>
    Task<bool> DecreaseMaterialResponsibilityAtWarehouseAsync(int warehouseId, int materialId, double quantity, int userId);

    /// <summary>
    /// Уменьшить ответственность пользователя за продукт на складе.
    /// </summary>
    Task<bool> DecreaseProductResponsibilityAtWarehouseAsync(int warehouseId, int productId, double quantity, int userId);

    /// <summary>
    /// Уменьшить ответственность за материал на складе на указанное количество (по записям AssignedAt, без привязки к userId).
    /// Используется при утиле: часть, уходящая с склада утиля (в т.ч. на ЭКО), снимает ответственность.
    /// </summary>
    Task<bool> DecreaseMaterialResponsibilityAtWarehouseByQuantityAsync(int warehouseId, int materialId, double quantity);

    /// <summary>
    /// Уменьшить ответственность за продукт на складе на указанное количество.
    /// </summary>
    Task<bool> DecreaseProductResponsibilityAtWarehouseByQuantityAsync(int warehouseId, int productId, double quantity);

    /// <summary>
    /// Сколько единиц материала на складе находится под ответственностью пользователя.
    /// </summary>
    Task<double> GetUserResponsibleQuantityAtWarehouseAsync(int userId, int warehouseId, int materialId);

    /// <summary>
    /// Сколько единиц продукта на складе находится под ответственностью пользователя.
    /// </summary>
    Task<double> GetUserResponsibleProductQuantityAtWarehouseAsync(int userId, int warehouseId, int productId);

    /// <summary>
    /// Все активные записи ответственности пользователя по складам (материалы и продукты).
    /// </summary>
    Task<List<ResponsibilityFilling>> GetResponsibilityFillingsByUserAsync(int userId);

    /// <summary>
    /// Все активные записи ответственности за материал на складе.
    /// </summary>
    Task<List<ResponsibilityFilling>> GetByWarehouseAndMaterialAsync(int warehouseId, int materialId);

    /// <summary>
    /// Все активные записи ответственности за продукт на складе.
    /// </summary>
    Task<List<ResponsibilityFilling>> GetByWarehouseAndProductAsync(int warehouseId, int productId);

    /// <summary>
    /// Назначения по материалам, агрегированные из ResponsibilityFilling (по MaterialId + UserId, сумма Quantity).
    /// </summary>
    Task<List<ResponsibilityAssignment>> GetActiveMaterialAssignmentsFromFillingAsync();

    /// <summary>
    /// Назначения по продуктам, агрегированные из ResponsibilityFilling.
    /// </summary>
    Task<List<ResponsibilityAssignment>> GetActiveProductAssignmentsFromFillingAsync();

    /// <summary>
    /// Передать ответственность за материал на складе от одного пользователя другому.
    /// Если quantityToTransfer не указано или равно полному количеству — передаётся всё; иначе у fromUserId остаётся остаток.
    /// </summary>
    Task TransferMaterialResponsibilityAsync(int warehouseId, int materialId, int fromUserId, int toUserId, double? quantityToTransfer = null);

    /// <summary>
    /// Передать ответственность за продукт на складе от одного пользователя другому.
    /// </summary>
    Task TransferProductResponsibilityAsync(int warehouseId, int productId, int fromUserId, int toUserId, double? quantityToTransfer = null);

    // Batch-specific methods
    Task AssignBatchResponsibilityAsync(int userId, int batchId, double quantity, string? measuringUnit);
    Task DecreaseBatchResponsibilityAsync(int batchId, double quantity, int userId);
    Task<double> GetUserResponsibleQuantityForBatchAsync(int userId, int batchId);

    /// <summary>
    /// Передать ответственность за партию от одного пользователя другому (часть или всё).
    /// </summary>
    Task TransferBatchResponsibilityAsync(int batchId, int fromUserId, int toUserId, double? quantityToTransfer = null);

    /// <summary>
    /// Снять ответственность пользователя за партию (вся его доля по ResponsibilityFilling).
    /// </summary>
    Task ReleaseBatchResponsibilityAsync(int batchId, int userId);

    /// <summary>
    /// Передать все активные ответственности по складам (ResponsibilityFilling) от одного пользователя другому.
    /// Используется при подтверждении передачи смены.
    /// </summary>
    Task<int> TransferAllResponsibilityFillingAsync(int fromUserId, int toUserId);

    /// <summary>
    /// Остатки под ответственностью пользователя по данным ResponsibilityFilling (то же, что передаётся при передаче смены).
    /// Используется для отображения при подтверждении принятия смены.
    /// </summary>
    Task<List<ResponsibilityStockItem>> GetResponsibilityStockForUserAsync(int userId);
}
