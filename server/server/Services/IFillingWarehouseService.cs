using server.Models;

namespace server.Services
{
    public interface IFillingWarehouseService
    {
        Task<IEnumerable<FillingWarehouse>> GetAllFillingWarehousesAsync();
        Task<FillingWarehouse?> GetFillingByMaterialAsync(int warehouseId, int materialId);
        Task<FillingWarehouse?> GetFillingByProductAsync(int warehouseId, int productId);
        Task<IEnumerable<FillingWarehouse>> GetFillingByWarehouseAsync(int warehouseId);
        Task<IEnumerable<FillingWarehouse>> GetFillingsByMaterialAsync(int materialId);
        Task<IEnumerable<FillingWarehouse>> GetFillingsByProductAsync(int productId);
        Task<FillingWarehouse> CreateFillingWarehouseAsync(FillingWarehouse filling);
        Task<FillingWarehouse> UpdateFillingWarehouseByMaterialAsync(int warehouseId, int materialId, FillingWarehouse updatedFilling);
        Task<FillingWarehouse> UpdateFillingWarehouseByProductAsync(int warehouseId, int productId, FillingWarehouse updatedFilling);
        Task<bool> DeleteFillingWarehouseByMaterialAsync(int warehouseId, int materialId);
        Task<bool> DeleteFillingWarehouseByProductAsync(int warehouseId, int productId);
        Task<FillingWarehouse> UpdateQuantityByMaterialAsync(int warehouseId, int materialId, double quantity);
        Task<FillingWarehouse> UpdateQuantityByProductAsync(int warehouseId, int productId, double quantity);
        Task<IEnumerable<FillingWarehouse>> GetWarehouseStockAsync(int warehouseId);
    }
}

