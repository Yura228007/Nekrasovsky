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

        public async Task<IEnumerable<Product>> SearchProductsAsync(string? name, string? code, bool? isActive = null, string? sortBy = null)
        {
            var query = _context.Products.AsQueryable();

            // Если указаны и name, и code - используем OR логику
            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(code))
            {
                query = query.Where(p =>
                    EF.Functions.ILike(p.Name, $"%{name}%") ||
                    (p.Code != null && EF.Functions.ILike(p.Code, $"%{code}%")));
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(name))
                    query = query.Where(p => EF.Functions.ILike(p.Name, $"%{name}%"));

                if (!string.IsNullOrWhiteSpace(code))
                    query = query.Where(p => p.Code != null && EF.Functions.ILike(p.Code, $"%{code}%"));
            }

            if (isActive.HasValue)
                query = query.Where(p => p.IsActive == isActive.Value);

            // Сортировка
            query = sortBy?.ToLower() switch
            {
                "name" => query.OrderBy(p => p.Name),
                "name_desc" => query.OrderByDescending(p => p.Name),
                "code" => query.OrderBy(p => p.Code),
                "code_desc" => query.OrderByDescending(p => p.Code),
                _ => query.OrderBy(p => p.Name) // По умолчанию
            };

            return await query.ToListAsync();
        }

        public async Task<Product> CreateProductAsync(Product product, int userId)
        {
            // Проверка уникальности артикула
            if (!string.IsNullOrWhiteSpace(product.Code))
            {
                var existingProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.Code == product.Code);
                if (existingProduct != null)
                {
                    throw new InvalidOperationException($"Продукт с артикулом '{product.Code}' уже существует");
                }
            }

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

            // Проверка уникальности артикула (исключая текущий продукт)
            if (!string.IsNullOrWhiteSpace(updatedProduct.Code))
            {
                var existingProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.Code == updatedProduct.Code && p.Id != id);
                if (existingProduct != null)
                {
                    throw new InvalidOperationException($"Продукт с артикулом '{updatedProduct.Code}' уже существует");
                }
            }

            product.Name = updatedProduct.Name;
            product.Description = updatedProduct.Description;
            product.Code = updatedProduct.Code;
            product.MeasuringUnit = updatedProduct.MeasuringUnit;
            product.IsActive = updatedProduct.IsActive;

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

            // Нельзя удалить продукт, если есть активные ответственности
            var hasActiveFilling = await _context.ResponsibilityFillings
                .AnyAsync(rf => rf.ProductId == id && rf.IsActive && rf.Quantity > 0);
            if (hasActiveFilling)
            {
                throw new InvalidOperationException("Нельзя удалить продукт, пока есть активные ответственности. Сначала снимите или передайте ответственность.");
            }

            // Не удаляем продукт, если есть выпуски (ProductOutput) — у них FK Restrict
            var hasOutputs = await _context.ProductOutputs.AnyAsync(po => po.ProductId == id);
            if (hasOutputs)
            {
                throw new InvalidOperationException("Невозможно удалить продукт: есть записи о выпуске. Сначала удалите или измените выпуски по этому продукту.");
            }

            // Удаляем все ответственности и заполнения (история по продукту исчезнет)
            var responsibilityFillings = await _context.ResponsibilityFillings
                .Where(rf => rf.ProductId == id)
                .ToListAsync();
            _context.ResponsibilityFillings.RemoveRange(responsibilityFillings);

            var fillingWarehouses = await _context.FillingWarehouses
                .Where(fw => fw.ProductId == id)
                .ToListAsync();
            _context.FillingWarehouses.RemoveRange(fillingWarehouses);

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product deleted with ID: {ProductId}. Removed {Rf} responsibility fillings, {Fw} filling warehouse records.",
                id, responsibilityFillings.Count, fillingWarehouses.Count);
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

