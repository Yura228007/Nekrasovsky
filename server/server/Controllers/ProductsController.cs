using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAll()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all products");
                return StatusCode(500, new { message = "An error occurred while retrieving products" });
            }
        }

        // GET: api/products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found", id);
                    return NotFound(new { message = $"Product with ID {id} not found" });
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting product with ID {ProductId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the product" });
            }
        }

        // GET: api/products/search?name=&code=&isActive=&sortBy=
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Product>>> Search(
            [FromQuery] string? name,
            [FromQuery] string? code,
            [FromQuery] bool? isActive,
            [FromQuery] string? sortBy)
        {
            try
            {
                var products = await _productService.SearchProductsAsync(name, code, isActive, sortBy);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching products");
                return StatusCode(500, new { message = "An error occurred while searching products" });
            }
        }

        // POST: api/products
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid model state", errors = ModelState });
            }

            try
            {
                var createdProduct = await _productService.CreateProductAsync(product);
                _logger.LogInformation("Product created successfully with ID: {ProductId}", createdProduct.Id);
                return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id },
                    new { message = "Product created successfully", product = createdProduct });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error while creating product");
                return Conflict(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while creating product");
                return StatusCode(500, new { message = "An error occurred while saving the product to the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating product");
                return StatusCode(500, new { message = "An unexpected error occurred while creating the product" });
            }
        }

        // PUT: api/products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product updated)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Id must be greater than 0" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid model state", errors = ModelState });
            }

            try
            {
                var product = await _productService.UpdateProductAsync(id, updated);
                _logger.LogInformation("Product updated successfully with ID: {ProductId}", id);
                return Ok(new { message = "Product updated successfully", product });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Product with ID {ProductId} not found for update", id);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error while updating product with ID {ProductId}", id);
                return Conflict(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating product with ID {ProductId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the product in the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating product with ID {ProductId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred while updating the product" });
            }
        }

        // DELETE: api/products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                var deleted = await _productService.DeleteProductAsync(id);
                if (!deleted)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found for deletion", id);
                    return NotFound(new { message = $"Product with ID {id} not found" });
                }

                _logger.LogInformation("Product deleted successfully with ID: {ProductId}", id);
                return Ok(new { message = "Product deleted successfully" });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while deleting product with ID {ProductId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the product from the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting product with ID {ProductId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred while deleting the product" });
            }
        }

        // GET: api/products/{id}/recipe
        [HttpGet("{id}/recipe")]
        public async Task<ActionResult<IEnumerable<Recipe>>> GetRecipe(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                var recipes = await _productService.GetProductRecipeAsync(id);
                return Ok(recipes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting recipe for product {ProductId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving recipe" });
            }
        }

        // GET: api/products/material/{materialId}
        [HttpGet("material/{materialId}")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsUsingMaterial(int materialId)
        {
            try
            {
                if (materialId <= 0)
                {
                    return BadRequest(new { message = "MaterialId must be greater than 0" });
                }

                var products = await _productService.GetProductsUsingMaterialAsync(materialId);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting products using material {MaterialId}", materialId);
                return StatusCode(500, new { message = "An error occurred while retrieving products" });
            }
        }
    }
}
