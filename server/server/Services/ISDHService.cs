namespace server.Services;

public interface ISDHService
{
    Task ProcessNonReturnableDefectAsync(int userId, int warehouseId, int? materialId, int? productId, double quantity, string? measuringUnit);
    Task ProcessSaleAsync(int userId, int warehouseId, int? materialId, int? productId, double quantity, string? measuringUnit);
    Task CreateSDHRequestAsync(int userId, int fromWarehouseId, int toWarehouseId, int? materialId, int? productId, double quantity, string? measuringUnit);
    Task ApproveSDHRequestAsync(int requestId, int approvedByUserId);
    Task RejectSDHRequestAsync(int requestId);
}
