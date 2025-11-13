using server.Models;

namespace server.Services
{
    public interface IAccessibleMovementService
    {
        Task<IEnumerable<AccessibleMovement>> GetAllMovementsAsync();
        Task<AccessibleMovement?> GetMovementAsync(int fromWarehouseId, int toWarehouseId, int materialId);
        Task<IEnumerable<AccessibleMovement>> GetMovementsFromWarehouseAsync(int warehouseId);
        Task<IEnumerable<AccessibleMovement>> GetMovementsToWarehouseAsync(int warehouseId);
        Task<IEnumerable<AccessibleMovement>> GetMovementsByMaterialAsync(int materialId);
        Task<AccessibleMovement> CreateMovementAsync(AccessibleMovement movement);
        Task<AccessibleMovement> UpdateMovementAsync(int fromWarehouseId, int toWarehouseId, int materialId, AccessibleMovement updatedMovement);
        Task<bool> DeleteMovementAsync(int fromWarehouseId, int toWarehouseId, int materialId);
        Task<bool> IsMovementAllowedAsync(int fromWarehouseId, int toWarehouseId, int materialId);
    }
}

