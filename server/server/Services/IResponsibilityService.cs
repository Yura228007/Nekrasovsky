using server.Models;

namespace server.Services
{
    public interface IResponsibilityService
    {
        Task<IEnumerable<Responsibility>> GetResponsibilitiesByUserAsync(int userId, bool activeOnly = true);
        Task<Responsibility?> GetResponsibilityByMaterialAsync(int materialId, bool activeOnly = true);
        Task<Responsibility?> GetResponsibilityByProductAsync(int productId, bool activeOnly = true);
        Task<Responsibility> AssignMaterialAsync(int materialId, int userId, int? quantity = null, string? measuringUnit = null);
        Task<Responsibility> AssignProductAsync(int productId, int userId, int? quantity = null, string? measuringUnit = null);
        Task<bool> DecreaseResponsibilityQuantityAsync(int materialId, int quantity, int? userId = null);
        Task<bool> DecreaseProductResponsibilityQuantityAsync(int productId, int quantity, int? userId = null);
        Task<bool> IsResponsibleForMaterialAsync(int materialId, int userId);
        Task<bool> ReleaseMaterialAsync(int materialId);
        Task<bool> ReleaseProductAsync(int productId);
        Task<int> TransferAllAsync(int fromUserId, int toUserId);
        Task<List<ResponsibilityStockItem>> GetResponsibilityStockAsync(int userId);
        Task<List<ResponsibilityAssignment>> GetActiveMaterialAssignmentsAsync();
        Task<List<ResponsibilityAssignment>> GetActiveProductAssignmentsAsync();
    }
}
