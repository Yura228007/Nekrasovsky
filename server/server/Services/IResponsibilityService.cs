using server.Models;

namespace server.Services
{
    public interface IResponsibilityService
    {
        Task<IEnumerable<Responsibility>> GetResponsibilitiesByUserAsync(int userId, bool activeOnly = true);
        Task<Responsibility?> GetResponsibilityByMaterialAsync(int materialId, bool activeOnly = true);
        Task<Responsibility?> GetResponsibilityByProductAsync(int productId, bool activeOnly = true);
        Task<Responsibility> AssignMaterialAsync(int materialId, int userId);
        Task<Responsibility> AssignProductAsync(int productId, int userId);
        Task<bool> IsResponsibleForMaterialAsync(int materialId, int userId);
        Task<bool> ReleaseMaterialAsync(int materialId);
        Task<int> TransferAllAsync(int fromUserId, int toUserId);
        Task<List<ResponsibilityStockItem>> GetResponsibilityStockAsync(int userId);
    }
}
