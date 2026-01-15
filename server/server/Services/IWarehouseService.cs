using server.Models;

namespace server.Services
{
    public interface IWarehouseService
    {
        Task<IEnumerable<Warehouse>> GetAllWarehousesAsync();
        Task<Warehouse?> GetWarehouseByIdAsync(int id);
        Task<IEnumerable<Warehouse>> GetWarehousesByTypeAsync(string type);
        Task<IEnumerable<Warehouse>> SearchWarehousesAsync(string? name, string? type);
        Task<Warehouse> CreateWarehouseAsync(Warehouse warehouse);
        Task<Warehouse> UpdateWarehouseAsync(int id, Warehouse updatedWarehouse);
        Task<bool> DeleteWarehouseAsync(int id);
        Task<Warehouse> StopWarehouseAsync(int id);
        Task<Warehouse> StartWarehouseAsync(int id);
        Task<IEnumerable<AccessibleMovement>> GetWarehouseMovementsAsync(int warehouseId);
        Task<IEnumerable<PartRequest>> GetWarehouseRequestsAsync(int warehouseId);
    }
}

