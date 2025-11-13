using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Data;

namespace server.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(AppDbContext context, ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string? name, string? code)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(p => EF.Functions.ILike(p.Name, $"%{name}%"));

            if (!string.IsNullOrWhiteSpace(code))
                query = query.Where(p => p.Code != null && EF.Functions.ILike(p.Code, $"%{code}%"));

            return await query.ToListAsync();
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product created with ID: {ProductId}, Name: {Name}", product.Id, product.Name);
            return product;
        }

        public async Task<Product> UpdateProductAsync(int id, Product updatedProduct)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {id} not found");
            }

            product.Name = updatedProduct.Name;
            product.Description = updatedProduct.Description;
            product.Code = updatedProduct.Code;
            product.MeasuringUnit = updatedProduct.MeasuringUnit;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Product updated with ID: {ProductId}", product.Id);
            return product;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return false;
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product deleted with ID: {ProductId}", id);
            return true;
        }

        public async Task<IEnumerable<Recipe>> GetProductRecipeAsync(int productId)
        {
            return await _context.Recipes
                .Where(r => r.ProductId == productId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsUsingMaterialAsync(int materialId)
        {
            return await _context.Recipes
                .Where(r => r.MaterialId == materialId)
                .Select(r => r.Product)
                .Distinct()
                .ToListAsync();
        }
    }
}

