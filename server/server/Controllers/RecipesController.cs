using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipesController : ControllerBase
    {
        private readonly IRecipeService _recipeService;
        private readonly ILogger<RecipesController> _logger;

        public RecipesController(IRecipeService recipeService, ILogger<RecipesController> logger)
        {
            _recipeService = recipeService;
            _logger = logger;
        }

        // GET: api/recipes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Recipe>>> GetAll()
        {
            try
            {
                var recipes = await _recipeService.GetAllRecipesAsync();
                return Ok(recipes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all recipes");
                return StatusCode(500, new { message = "An error occurred while retrieving recipes" });
            }
        }

        // GET: api/recipes/product/5
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<Recipe>>> GetByProduct(int productId)
        {
            try
            {
                if (productId <= 0)
                {
                    return BadRequest(new { message = "ProductId must be greater than 0" });
                }

                var recipes = await _recipeService.GetRecipeByProductAsync(productId);
                return Ok(recipes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting recipes for product {ProductId}", productId);
                return StatusCode(500, new { message = "An error occurred while retrieving recipes" });
            }
        }

        // GET: api/recipes/material/5
        [HttpGet("material/{materialId}")]
        public async Task<ActionResult<IEnumerable<Recipe>>> GetByMaterial(int materialId)
        {
            try
            {
                if (materialId <= 0)
                {
                    return BadRequest(new { message = "MaterialId must be greater than 0" });
                }

                var recipes = await _recipeService.GetRecipesByMaterialAsync(materialId);
                return Ok(recipes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting recipes for material {MaterialId}", materialId);
                return StatusCode(500, new { message = "An error occurred while retrieving recipes" });
            }
        }

        // GET: api/recipes/{productId}/{materialId}
        [HttpGet("{productId}/{materialId}")]
        public async Task<ActionResult<Recipe>> GetRecipe(int productId, int materialId)
        {
            try
            {
                if (productId <= 0)
                {
                    return BadRequest(new { message = "ProductId must be greater than 0" });
                }

                if (materialId <= 0)
                {
                    return BadRequest(new { message = "MaterialId must be greater than 0" });
                }

                var recipe = await _recipeService.GetRecipeAsync(productId, materialId);
                if (recipe == null)
                {
                    _logger.LogWarning("Recipe not found for ProductId {ProductId} and MaterialId {MaterialId}", productId, materialId);
                    return NotFound(new { message = "Recipe not found" });
                }

                return Ok(recipe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting recipe for ProductId {ProductId} and MaterialId {MaterialId}", productId, materialId);
                return StatusCode(500, new { message = "An error occurred while retrieving the recipe" });
            }
        }

        // POST: api/recipes
        [HttpPost]
        public async Task<IActionResult> CreateRecipe([FromBody] Recipe recipe)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid model state", errors = ModelState });
            }

            try
            {
                if (recipe.ProductId <= 0)
                {
                    return BadRequest(new { message = "ProductId must be greater than 0" });
                }

                if (recipe.MaterialId <= 0)
                {
                    return BadRequest(new { message = "MaterialId must be greater than 0" });
                }

                var createdRecipe = await _recipeService.CreateRecipeAsync(recipe);
                _logger.LogInformation("Recipe created successfully for ProductId {ProductId} and MaterialId {MaterialId}", recipe.ProductId, recipe.MaterialId);
                return Ok(new { message = "Recipe created successfully", recipe = createdRecipe });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Key not found while creating recipe");
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while creating recipe");
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while creating recipe");
                return StatusCode(500, new { message = "An error occurred while saving the recipe to the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating recipe");
                return StatusCode(500, new { message = "An unexpected error occurred while creating the recipe" });
            }
        }

        // POST: api/recipes/edit/{productId}/{materialId} - POST because of composite key
        [HttpPost("edit/{productId}/{materialId}")]
        public async Task<IActionResult> UpdateRecipe(int productId, int materialId, [FromBody] Recipe updated)
        {
            if (productId <= 0)
            {
                return BadRequest(new { message = "ProductId must be greater than 0" });
            }

            if (materialId <= 0)
            {
                return BadRequest(new { message = "MaterialId must be greater than 0" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid model state", errors = ModelState });
            }

            try
            {
                var recipe = await _recipeService.UpdateRecipeAsync(productId, materialId, updated);
                _logger.LogInformation("Recipe updated successfully for ProductId {ProductId} and MaterialId {MaterialId}", productId, materialId);
                return Ok(new { message = "Recipe updated successfully", recipe });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Recipe not found for update with ProductId {ProductId} and MaterialId {MaterialId}", productId, materialId);
                return NotFound(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating recipe");
                return StatusCode(500, new { message = "An error occurred while updating the recipe in the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating recipe");
                return StatusCode(500, new { message = "An unexpected error occurred while updating the recipe" });
            }
        }

        // POST: api/recipes/delete/{productId}/{materialId} - POST because of composite key
        [HttpPost("delete/{productId}/{materialId}")]
        public async Task<IActionResult> DeleteRecipe(int productId, int materialId)
        {
            try
            {
                if (productId <= 0)
                {
                    return BadRequest(new { message = "ProductId must be greater than 0" });
                }

                if (materialId <= 0)
                {
                    return BadRequest(new { message = "MaterialId must be greater than 0" });
                }

                var deleted = await _recipeService.DeleteRecipeAsync(productId, materialId);
                if (!deleted)
                {
                    _logger.LogWarning("Recipe not found for deletion with ProductId {ProductId} and MaterialId {MaterialId}", productId, materialId);
                    return NotFound(new { message = "Recipe not found" });
                }

                _logger.LogInformation("Recipe deleted successfully for ProductId {ProductId} and MaterialId {MaterialId}", productId, materialId);
                return Ok(new { message = "Recipe deleted successfully" });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while deleting recipe");
                return StatusCode(500, new { message = "An error occurred while deleting the recipe from the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting recipe");
                return StatusCode(500, new { message = "An unexpected error occurred while deleting the recipe" });
            }
        }

        // POST: api/recipes/add-material
        [HttpPost("add-material")]
        public async Task<IActionResult> AddMaterialToRecipe([FromBody] Recipe recipe)
        {
            try
            {
                if (recipe.ProductId <= 0)
                {
                    return BadRequest(new { message = "ProductId must be greater than 0" });
                }

                if (recipe.MaterialId <= 0)
                {
                    return BadRequest(new { message = "MaterialId must be greater than 0" });
                }

                var createdRecipe = await _recipeService.AddMaterialToRecipeAsync(
                    recipe.ProductId,
                    recipe.MaterialId,
                    recipe.Quantity,
                    recipe.MeasuringType);
                _logger.LogInformation("Material added to recipe successfully for ProductId {ProductId} and MaterialId {MaterialId}", recipe.ProductId, recipe.MaterialId);
                return Ok(new { message = "Material added to recipe successfully", recipe = createdRecipe });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Key not found while adding material to recipe");
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while adding material to recipe");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while adding material to recipe");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        // POST: api/recipes/remove-material
        [HttpPost("remove-material")]
        public async Task<IActionResult> RemoveMaterialFromRecipe([FromBody] Recipe recipe)
        {
            try
            {
                if (recipe.ProductId <= 0)
                {
                    return BadRequest(new { message = "ProductId must be greater than 0" });
                }

                if (recipe.MaterialId <= 0)
                {
                    return BadRequest(new { message = "MaterialId must be greater than 0" });
                }

                var deleted = await _recipeService.RemoveMaterialFromRecipeAsync(recipe.ProductId, recipe.MaterialId);
                if (!deleted)
                {
                    _logger.LogWarning("Recipe not found for removal with ProductId {ProductId} and MaterialId {MaterialId}", recipe.ProductId, recipe.MaterialId);
                    return NotFound(new { message = "Recipe not found" });
                }

                _logger.LogInformation("Material removed from recipe successfully for ProductId {ProductId} and MaterialId {MaterialId}", recipe.ProductId, recipe.MaterialId);
                return Ok(new { message = "Material removed from recipe successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while removing material from recipe");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }
    }
}
