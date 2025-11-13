using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipesController : ControllerBase
    {
        private readonly IRecipeService _recipeService;

        public RecipesController(IRecipeService recipeService)
        {
            _recipeService = recipeService;
        }

        // GET: api/recipes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Recipe>>> GetAll()
        {
            var recipes = await _recipeService.GetAllRecipesAsync();
            return Ok(recipes);
        }

        // GET: api/recipes/product/5
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<Recipe>>> GetByProduct(int productId)
        {
            var recipes = await _recipeService.GetRecipeByProductAsync(productId);
            return Ok(recipes);
        }

        // GET: api/recipes/material/5
        [HttpGet("material/{materialId}")]
        public async Task<ActionResult<IEnumerable<Recipe>>> GetByMaterial(int materialId)
        {
            var recipes = await _recipeService.GetRecipesByMaterialAsync(materialId);
            return Ok(recipes);
        }

        // GET: api/recipes/{productId}/{materialId}
        [HttpGet("{productId}/{materialId}")]
        public async Task<ActionResult<Recipe>> GetRecipe(int productId, int materialId)
        {
            var recipe = await _recipeService.GetRecipeAsync(productId, materialId);
            if (recipe == null)
                return NotFound($"Recipe not found");

            return Ok(recipe);
        }

        // POST: api/recipes/add
        [HttpPost("add")]
        public async Task<IActionResult> AddRecipe([FromBody] Recipe recipe)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdRecipe = await _recipeService.CreateRecipeAsync(recipe);
                return Ok(new { message = "Recipe created successfully", recipe = createdRecipe });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/recipes/edit/{productId}/{materialId}
        [HttpPost("edit/{productId}/{materialId}")]
        public async Task<IActionResult> EditRecipe(int productId, int materialId, [FromBody] Recipe updated)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var recipe = await _recipeService.UpdateRecipeAsync(productId, materialId, updated);
                return Ok(new { message = "Recipe updated successfully", recipe });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/recipes/delete/{productId}/{materialId}
        [HttpPost("delete/{productId}/{materialId}")]
        public async Task<IActionResult> DeleteRecipe(int productId, int materialId)
        {
            var deleted = await _recipeService.DeleteRecipeAsync(productId, materialId);
            if (!deleted)
                return NotFound($"Recipe not found");

            return Ok(new { message = "Recipe deleted successfully" });
        }

        // POST: api/recipes/add-material
        [HttpPost("add-material")]
        public async Task<IActionResult> AddMaterialToRecipe([FromBody] Recipe recipe)
        {
            try
            {
                var createdRecipe = await _recipeService.AddMaterialToRecipeAsync(
                    recipe.ProductId, 
                    recipe.MaterialId, 
                    recipe.Quantity, 
                    recipe.MeasuringType);
                return Ok(new { message = "Material added to recipe successfully", recipe = createdRecipe });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/recipes/remove-material
        [HttpPost("remove-material")]
        public async Task<IActionResult> RemoveMaterialFromRecipe([FromBody] Recipe recipe)
        {
            var deleted = await _recipeService.RemoveMaterialFromRecipeAsync(recipe.ProductId, recipe.MaterialId);
            if (!deleted)
                return NotFound($"Recipe not found");

            return Ok(new { message = "Material removed from recipe successfully" });
        }
    }
}

