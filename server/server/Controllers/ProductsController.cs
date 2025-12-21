using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAll()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        // GET: api/products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound($"Product with ID {id} not found");

            return Ok(product);
        }

        // GET: api/products/search?name=&code=
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Product>>> Search([FromQuery] string? name, [FromQuery] string? code)
        {
            var products = await _productService.SearchProductsAsync(name, code);
            return Ok(products);
        }

        // POST: api/products/add
        [HttpPost("add")]
        public async Task<IActionResult> AddProduct([FromBody] Product product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdProduct = await _productService.CreateProductAsync(product);
            return Ok(new { message = "Product created successfully", product = createdProduct });
        }

        // POST: api/products/edit/{id}
        [HttpPost("edit/{id}")]
        public async Task<IActionResult> EditProduct(int id, [FromBody] Product updated)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var product = await _productService.UpdateProductAsync(id, updated);
                return Ok(new { message = "Product updated successfully", product });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/products/delete/{id}
        [HttpPost("delete/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var deleted = await _productService.DeleteProductAsync(id);
            if (!deleted)
                return NotFound($"Product with ID {id} not found");

            return Ok(new { message = "Product deleted successfully" });
        }

        // GET: api/products/{id}/recipe
        [HttpGet("{id}/recipe")]
        public async Task<ActionResult<IEnumerable<Recipe>>> GetRecipe(int id)
        {
            var recipes = await _productService.GetProductRecipeAsync(id);
            return Ok(recipes);
        }

        // GET: api/products/material/{materialId}
        [HttpGet("material/{materialId}")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsUsingMaterial(int materialId)
        {
            var products = await _productService.GetProductsUsingMaterialAsync(materialId);
            return Ok(products);
        }
    }
}

