using server.Models;

namespace server.Services;

public interface IFinishedGoodsRequestService
{
    /// <summary>
    /// Создать запрос на перемещение продукции на склад готовой продукции.
    /// Материал уже должен быть перемещен физически, ответственность остается у создателя до одобрения.
    /// </summary>
    Task<FinishedGoodsRequest> CreateFinishedGoodsRequestAsync(
        int fromUserId, int fromWarehouseId, int toWarehouseId,
        int productId, double quantity, string? measuringUnit, FinishedGoodsRequestType requestType, int? productOutputId = null);

    /// <summary>
    /// Получить все ожидающие запросы для указанного склада готовой продукции.
    /// </summary>
    Task<List<FinishedGoodsRequest>> GetPendingFinishedGoodsRequestsAsync(int finishedGoodsWarehouseId);

    /// <summary>
    /// Одобрить запрос. При одобрении ответственность передается менеджеру склада готовой продукции.
    /// </summary>
    Task<FinishedGoodsRequest> ApproveFinishedGoodsRequestAsync(int requestId, int approvedByUserId);

    /// <summary>
    /// Отклонить запрос. Ответственность остается у создателя запроса.
    /// </summary>
    Task<FinishedGoodsRequest> RejectFinishedGoodsRequestAsync(int requestId, int rejectedByUserId);
}
