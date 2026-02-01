using server.Models;

namespace server.Services;

public interface IResponsibilityFillingService
{
    /// <summary>
    /// Назначить ответственность за количество материала на складе.
    /// Проверяет: сумма по ResponsibilityFilling + quantity не превышает FillingWarehouse.Quantity.
    /// </summary>
    Task<ResponsibilityFilling> AssignMaterialAtWarehouseAsync(int userId, int warehouseId, int materialId, int quantity, string? measuringUnit = null);

    /// <summary>
    /// Назначить ответственность за количество продукта на складе.
    /// </summary>
    Task<ResponsibilityFilling> AssignProductAtWarehouseAsync(int userId, int warehouseId, int productId, int quantity, string? measuringUnit = null);

    /// <summary>
    /// Уменьшить ответственность пользователя за материал на складе (при выдаче/перемещении).
    /// Уменьшает записи ResponsibilityFilling по дате AssignedAt (сначала самые новые).
    /// </summary>
    Task<bool> DecreaseMaterialResponsibilityAtWarehouseAsync(int warehouseId, int materialId, int quantity, int userId);

    /// <summary>
    /// Уменьшить ответственность пользователя за продукт на складе.
    /// </summary>
    Task<bool> DecreaseProductResponsibilityAtWarehouseAsync(int warehouseId, int productId, int quantity, int userId);

    /// <summary>
    /// Сколько единиц материала на складе находится под ответственностью пользователя.
    /// </summary>
    Task<int> GetUserResponsibleQuantityAtWarehouseAsync(int userId, int warehouseId, int materialId);

    /// <summary>
    /// Сколько единиц продукта на складе находится под ответственностью пользователя.
    /// </summary>
    Task<int> GetUserResponsibleProductQuantityAtWarehouseAsync(int userId, int warehouseId, int productId);

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
}
