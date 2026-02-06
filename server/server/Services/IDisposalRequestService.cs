using server.Models;

namespace server.Services;

public interface IDisposalRequestService
{
    /// <summary>
    /// Создать запрос на перемещение материала на склад утиля
    /// </summary>
    Task<DisposalRequest> CreateDisposalRequestAsync(int fromUserId, int fromWarehouseId, int toWarehouseId, 
        int materialId, double quantity, string? measuringUnit, DisposalRequestType requestType);

    /// <summary>
    /// Создать запрос на перемещение продукта на склад утиля
    /// </summary>
    Task<DisposalRequest> CreateDisposalRequestForProductAsync(int fromUserId, int fromWarehouseId, int toWarehouseId,
        int productId, double quantity, string? measuringUnit, DisposalRequestType requestType);

    /// <summary>
    /// Получить все запросы на склад утиля (для отображения на странице утиля)
    /// </summary>
    Task<List<DisposalRequest>> GetPendingDisposalRequestsAsync(int disposalWarehouseId);

    /// <summary>
    /// Подтвердить запрос - передать ответственность менеджеру склада утиля
    /// </summary>
    Task<DisposalRequest> ApproveDisposalRequestAsync(int requestId, int approvedByUserId);

    /// <summary>
    /// Отклонить запрос - ответственность остается у создателя
    /// </summary>
    Task<DisposalRequest> RejectDisposalRequestAsync(int requestId);
}
