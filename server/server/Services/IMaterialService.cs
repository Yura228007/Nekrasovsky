using server.Models;

namespace server.Services
{
    public interface IMaterialService
    {
        Task<IEnumerable<Material>> GetAllMaterialsAsync();
        Task<Material?> GetMaterialByIdAsync(int id);
        Task<IEnumerable<Material>> SearchMaterialsAsync(string? name, string? code, bool? isActive = null, string? sortBy = null);
        Task<IEnumerable<Material>> GetMaterialsByMeasuringUnitAsync(string unit);
        Task<Material> CreateMaterialAsync(Material material, int userId, double? quantity = null, string? measuringUnit = null, int? warehouseId = null);
        Task<Material> UpdateMaterialAsync(int id, Material updatedMaterial);
        Task<bool> DeleteMaterialAsync(int id);
        Task<IEnumerable<AccessibleMovement>> GetMaterialMovementsAsync(int materialId);
        Task<object> GetMaterialUsageAsync(int materialId);
    }
}

