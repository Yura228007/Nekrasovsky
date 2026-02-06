using server.Models;

namespace server.Services
{
    public interface IRecipeService
    {
        Task<IEnumerable<Recipe>> GetAllRecipesAsync();
        Task<IEnumerable<Recipe>> GetRecipeByProductAsync(int productId);
        Task<IEnumerable<Recipe>> GetRecipesByMaterialAsync(int materialId);
        Task<Recipe?> GetRecipeAsync(int productId, int materialId);
        Task<Recipe> CreateRecipeAsync(Recipe recipe);
        Task<Recipe> UpdateRecipeAsync(int productId, int materialId, Recipe updatedRecipe);
        Task<bool> DeleteRecipeAsync(int productId, int materialId);
        Task<Recipe> AddMaterialToRecipeAsync(int productId, int materialId, double quantity, string? measuringType);
        Task<bool> RemoveMaterialFromRecipeAsync(int productId, int materialId);
    }
}

