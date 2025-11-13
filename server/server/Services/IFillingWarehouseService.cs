using server.Models;

namespace server.Services
{
    public interface IFillingWarehouseService
    {
        Task<IEnumerable<FillingWarehouse>> GetAllFillingWarehousesAsync();
        Task<FillingWarehouse?> GetFillingWarehouseAsync(int warehouseId, int materialId);
        Task<IEnumerable<FillingWarehouse>> GetFillingByWarehouseAsync(int warehouseId);
        Task<IEnumerable<FillingWarehouse>> GetFillingByMaterialAsync(int materialId);
        Task<FillingWarehouse> CreateFillingWarehouseAsync(FillingWarehouse filling);
        Task<FillingWarehouse> UpdateFillingWarehouseAsync(int warehouseId, int materialId, FillingWarehouse updatedFilling);
        Task<bool> DeleteFillingWarehouseAsync(int warehouseId, int materialId);
        Task<FillingWarehouse> UpdateQuantityAsync(int warehouseId, int materialId, int quantity);
        Task<IEnumerable<FillingWarehouse>> GetWarehouseStockAsync(int warehouseId);
    }
}

