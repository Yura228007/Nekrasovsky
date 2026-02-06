using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Data;

namespace server.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RecipeService> _logger;

        public RecipeService(AppDbContext context, ILogger<RecipeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Recipe>> GetAllRecipesAsync()
        {
            return await _context.Recipes.ToListAsync();
        }

        public async Task<IEnumerable<Recipe>> GetRecipeByProductAsync(int productId)
        {
            return await _context.Recipes
                .Where(r => r.ProductId == productId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Recipe>> GetRecipesByMaterialAsync(int materialId)
        {
            return await _context.Recipes
                .Where(r => r.MaterialId == materialId)
                .ToListAsync();
        }

        public async Task<Recipe?> GetRecipeAsync(int productId, int materialId)
        {
            return await _context.Recipes
                .FirstOrDefaultAsync(r => r.ProductId == productId && r.MaterialId == materialId);
        }

        public async Task<Recipe> CreateRecipeAsync(Recipe recipe)
        {
            // Check if product exists
            if (!await _context.Products.AnyAsync(p => p.Id == recipe.ProductId))
            {
                throw new KeyNotFoundException($"Product with ID {recipe.ProductId} not found");
            }

            // Check if material exists
            if (!await _context.Materials.AnyAsync(m => m.Id == recipe.MaterialId))
            {
                throw new KeyNotFoundException($"Material with ID {recipe.MaterialId} not found");
            }

            // Check if recipe already exists
            if (await _context.Recipes.AnyAsync(r => r.ProductId == recipe.ProductId && r.MaterialId == recipe.MaterialId))
            {
                throw new InvalidOperationException($"Recipe for this product and material already exists");
            }

            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Recipe created for Product {ProductId} and Material {MaterialId}", recipe.ProductId, recipe.MaterialId);
            return recipe;
        }

        public async Task<Recipe> UpdateRecipeAsync(int productId, int materialId, Recipe updatedRecipe)
        {
            var recipe = await _context.Recipes
                .FirstOrDefaultAsync(r => r.ProductId == productId && r.MaterialId == materialId);

            if (recipe == null)
            {
                throw new KeyNotFoundException($"Recipe not found");
            }

            recipe.Quantity = updatedRecipe.Quantity;
            recipe.MeasuringType = updatedRecipe.MeasuringType;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Recipe updated for Product {ProductId} and Material {MaterialId}", productId, materialId);
            return recipe;
        }

        public async Task<bool> DeleteRecipeAsync(int productId, int materialId)
        {
            var recipe = await _context.Recipes
                .FirstOrDefaultAsync(r => r.ProductId == productId && r.MaterialId == materialId);

            if (recipe == null)
            {
                return false;
            }

            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Recipe deleted for Product {ProductId} and Material {MaterialId}", productId, materialId);
            return true;
        }

        public async Task<Recipe> AddMaterialToRecipeAsync(int productId, int materialId, double quantity, string? measuringType)
        {
            var recipe = new Recipe
            {
                ProductId = productId,
                MaterialId = materialId,
                Quantity = quantity,
                MeasuringType = measuringType
            };

            return await CreateRecipeAsync(recipe);
        }

        public async Task<bool> RemoveMaterialFromRecipeAsync(int productId, int materialId)
        {
            return await DeleteRecipeAsync(productId, materialId);
        }
    }
}

