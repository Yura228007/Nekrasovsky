using server.Models;

namespace server.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(int id);
        Task<IEnumerable<Product>> SearchProductsAsync(string? name, string? code);
        Task<Product> CreateProductAsync(Product product);
        Task<Product> UpdateProductAsync(int id, Product updatedProduct);
        Task<bool> DeleteProductAsync(int id);
        Task<IEnumerable<Recipe>> GetProductRecipeAsync(int productId);
        Task<IEnumerable<Product>> GetProductsUsingMaterialAsync(int materialId);
    }
}

