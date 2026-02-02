using server.Models;

namespace server.Services;

public interface IProductMovementRequestService
{
    Task<IEnumerable<ProductMovementRequest>> GetAllRequestsAsync();
    Task<ProductMovementRequest?> GetRequestByIdAsync(int id);
    Task<IEnumerable<ProductMovementRequest>> GetRequestsByStatusAsync(ProductMovementStatus status);
    Task<IEnumerable<ProductMovementRequest>> GetRequestsByUserAsync(int userId, bool sent = true);
    Task<IEnumerable<ProductMovementRequest>> GetRequestsByWarehouseAsync(int warehouseId, bool from = true);
    Task<IEnumerable<ProductMovementRequest>> GetRequestsByBatchAsync(int batchId);
    Task<ProductMovementRequest> CreateRequestAsync(ProductMovementRequest request);
    Task<ProductMovementRequest> UpdateRequestAsync(int id, ProductMovementRequest updatedRequest);
    Task<ProductMovementRequest> ApproveRequestAsync(int id);
    Task<ProductMovementRequest> RejectRequestAsync(int id, string? reason);
    Task<bool> DeleteRequestAsync(int id);
}
