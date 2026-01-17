using server.Models;

namespace server.Services
{
    public interface IPartRequestService
    {
        Task<IEnumerable<PartRequest>> GetAllPartRequestsAsync();
        Task<PartRequest?> GetPartRequestByIdAsync(int id);
        Task<IEnumerable<PartRequest>> GetPartRequestsByStatusAsync(PartRequestStatus status);
        Task<IEnumerable<PartRequest>> GetPartRequestsByUserAsync(int userId, bool sent = true);
        Task<IEnumerable<PartRequest>> GetPartRequestsByWarehouseAsync(int warehouseId, bool from = true);
        Task<IEnumerable<PartRequest>> GetPartRequestsByMaterialAsync(int materialId);
        Task<PartRequest> CreatePartRequestAsync(PartRequest request);
        Task<PartRequest> UpdatePartRequestAsync(int id, PartRequest updatedRequest);
        Task<PartRequest> ApprovePartRequestAsync(int id);
        Task<PartRequest> RejectPartRequestAsync(int id, string? reason);
        Task<bool> DeletePartRequestAsync(int id);
        Task<int> GetRejectionCountAsync(int fromUserId, int toUserId);
    }
}

