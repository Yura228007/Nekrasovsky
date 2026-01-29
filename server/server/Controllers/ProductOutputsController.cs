using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Services;

namespace server.Controllers
{
    [Route("api/product-outputs")]
    [ApiController]
    public class ProductOutputsController : ControllerBase
    {
        private readonly IProductOutputService _productOutputService;
        private readonly IHistoryService _historyService;
        private readonly ILogger<ProductOutputsController> _logger;

        public ProductOutputsController(
            IProductOutputService productOutputService,
            IHistoryService historyService,
            ILogger<ProductOutputsController> logger)
        {
            _productOutputService = productOutputService;
            _historyService = historyService;
            _logger = logger;
        }

        // GET: api/product-outputs
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var outputs = await _productOutputService.GetAllAsync();
            return Ok(outputs);
        }

        // GET: api/product-outputs/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest(new { message = "Invalid user ID" });
            }

            var outputs = await _productOutputService.GetByUserAsync(userId);
            return Ok(outputs);
        }

        // GET: api/product-outputs/work-report/{workReportId}
        [HttpGet("work-report/{workReportId}")]
        public async Task<IActionResult> GetByWorkReport(int workReportId)
        {
            if (workReportId <= 0)
            {
                return BadRequest(new { message = "Invalid work report ID" });
            }

            var outputs = await _productOutputService.GetByWorkReportAsync(workReportId);
            return Ok(outputs);
        }

        // GET: api/product-outputs/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid ID" });
            }

            var output = await _productOutputService.GetByIdAsync(id);
            if (output == null)
            {
                return NotFound(new { message = $"ProductOutput with ID {id} not found" });
            }

            return Ok(output);
        }

        // POST: api/product-outputs
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductOutput productOutput)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid model state", errors = ModelState });
            }

            try
            {
                var userId = GetUserIdFromHeader();
                if (userId.HasValue)
                {
                    productOutput.UserId = userId.Value;
                }

                var created = await _productOutputService.CreateAsync(productOutput);
                _logger.LogInformation("ProductOutput created with ID: {Id}", created.Id);

                await TryLogAsync(userId, new HistoryEvent
                {
                    UserId = userId ?? 0,
                    Action = "ProductOutput.Created",
                    EntityType = "ProductOutput",
                    EntityId = created.Id,
                    ProductId = created.ProductId,
                    WarehouseId = created.WarehouseId,
                    Description = $"Выпуск продукции: произведено {created.ProducedQuantity}, брак {created.DefectQuantity}, эко {created.EcoQuantity}"
                });

                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while creating product output");
                return StatusCode(500, new { message = "An error occurred while saving" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating product output");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        // PUT: api/product-outputs/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductOutput updated)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid ID" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid model state", errors = ModelState });
            }

            try
            {
                var result = await _productOutputService.UpdateAsync(id, updated);
                _logger.LogInformation("ProductOutput updated with ID: {Id}", id);

                await TryLogAsync(GetUserIdFromHeader(), new HistoryEvent
                {
                    UserId = GetUserIdFromHeader() ?? 0,
                    Action = "ProductOutput.Updated",
                    EntityType = "ProductOutput",
                    EntityId = result.Id,
                    ProductId = result.ProductId,
                    WarehouseId = result.WarehouseId,
                    Description = $"Обновлен выпуск продукции: произведено {result.ProducedQuantity}, брак {result.DefectQuantity}, эко {result.EcoQuantity}"
                });

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating product output");
                return StatusCode(500, new { message = "An error occurred while updating" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating product output");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        // DELETE: api/product-outputs/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid ID" });
            }

            try
            {
                await _productOutputService.DeleteAsync(id);
                _logger.LogInformation("ProductOutput deleted with ID: {Id}", id);

                await TryLogAsync(GetUserIdFromHeader(), new HistoryEvent
                {
                    UserId = GetUserIdFromHeader() ?? 0,
                    Action = "ProductOutput.Deleted",
                    EntityType = "ProductOutput",
                    EntityId = id,
                    Description = $"Удален выпуск продукции ID {id}"
                });

                return Ok(new { message = "ProductOutput deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting product output");
                return StatusCode(500, new { message = "An error occurred while deleting" });
            }
        }

        private int? GetUserIdFromHeader()
        {
            if (Request.Headers.TryGetValue("X-User-Id", out var userIdValue) &&
                int.TryParse(userIdValue.FirstOrDefault(), out var userId))
            {
                return userId;
            }
            return null;
        }

        private async Task TryLogAsync(int? userId, HistoryEvent evt)
        {
            try
            {
                if (userId.HasValue)
                {
                    evt.UserId = userId.Value;
                    await _historyService.AddEventAsync(evt);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to log history event");
            }
        }
    }
}
