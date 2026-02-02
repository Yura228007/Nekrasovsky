using server.Models;

namespace server.Services;

public interface IProductBatchService
{
    Task<IEnumerable<ProductBatch>> GetAllBatchesAsync();
    Task<ProductBatch?> GetBatchByIdAsync(int id);
    Task<IEnumerable<ProductBatch>> GetBatchesByProductAsync(int productId);
    Task<IEnumerable<ProductBatch>> GetBatchesByWarehouseAsync(int warehouseId);
    Task<IEnumerable<ProductBatch>> GetBatchesByUserAsync(int userId);
    Task<IEnumerable<ProductBatch>> GetBatchesByProductAndWarehouseAsync(int productId, int warehouseId);
    Task<ProductBatch> CreateBatchAsync(ProductBatch batch);
    Task<ProductBatch> UpdateBatchAsync(int id, ProductBatch updatedBatch);
    Task<bool> DeleteBatchAsync(int id);
    Task<ProductBatch> DecreaseBatchQuantityAsync(int batchId, int quantity);
    Task<int> GetTotalQuantityByProductAndWarehouseAsync(int productId, int warehouseId);
    /// <summary>Пересчитывает и обновляет FillingWarehouse для продукта на складе (после создания партии в той же транзакции).</summary>
    Task UpdateFillingWarehouseForProductAsync(int productId, int warehouseId);
}
