using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;
using server.Services;

namespace server.Controllers;

[ApiController]
[Route("api/product-batches")]
public class ProductBatchesController : ControllerBase
{
    private readonly IProductBatchService _batchService;
    private readonly IHistoryService _historyService;
    private readonly ILogger<ProductBatchesController> _logger;
    private readonly AppDbContext _context;

    public ProductBatchesController(
        IProductBatchService batchService,
        IHistoryService historyService,
        ILogger<ProductBatchesController> logger,
        AppDbContext context)
    {
        _batchService = batchService;
        _historyService = historyService;
        _logger = logger;
        _context = context;
    }

    // GET: api/product-batches
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var batches = await _batchService.GetAllBatchesAsync();
            
            // Получаем информацию об ответственности из ResponsibilityFilling
            var batchIds = batches.Select(b => b.Id).ToList();
            var responsibilityFillings = await _context.ResponsibilityFillings
                .Include(rf => rf.User)
                .Where(rf => rf.ProductBatchId.HasValue && batchIds.Contains(rf.ProductBatchId.Value) && rf.IsActive)
                .ToListAsync();

            // Получаем FillingWarehouse для продуктов партий
            var productIds = batches.Select(b => b.ProductId).Distinct().ToList();
            var warehouseIds = batches.Select(b => b.WarehouseId).Distinct().ToList();
            var fillingWarehouses = await _context.FillingWarehouses
                .Where(fw => fw.ProductId.HasValue && productIds.Contains(fw.ProductId.Value) && warehouseIds.Contains(fw.WarehouseId))
                .ToListAsync();

            // Группируем по партиям
            var responsibilityByBatch = responsibilityFillings
                .GroupBy(rf => rf.ProductBatchId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Группируем FillingWarehouse по продукту и складу
            var fillingByProductWarehouse = fillingWarehouses
                .GroupBy(fw => new { ProductId = fw.ProductId!.Value, WarehouseId = fw.WarehouseId })
                .ToDictionary(g => g.Key, g => g.Sum(fw => fw.Quantity));

            // Создаем DTO с информацией об ответственности
            var dtos = batches.Select(batch =>
            {
                var dto = new ProductBatchDto
                {
                    Id = batch.Id,
                    ProductId = batch.ProductId,
                    WarehouseId = batch.WarehouseId,
                    Quantity = batch.Quantity,
                    MeasuringUnit = batch.MeasuringUnit,
                    CreatedByUserId = batch.CreatedByUserId,
                    CreatedAt = batch.CreatedAt,
                    BatchNumber = batch.BatchNumber,
                    Note = batch.Note,
                    IsActive = batch.IsActive,
                    Product = batch.Product,
                    Warehouse = batch.Warehouse,
                    CreatedByUser = batch.CreatedByUser
                };

                // Получаем информацию об ответственности из ResponsibilityFilling
                if (responsibilityByBatch.TryGetValue(batch.Id, out var fillings) && fillings.Count > 0)
                {
                    // Берем первого ответственного (или можно взять того, у кого больше количество)
                    var firstFilling = fillings.OrderByDescending(f => f.Quantity).First();
                    dto.ResponsibleUserId = firstFilling.UserId;
                    dto.ResponsibleUserName = firstFilling.User != null 
                        ? $"{firstFilling.User.Surname} {firstFilling.User.Name}" 
                        : $"Пользователь #{firstFilling.UserId}";
                    dto.ResponsibleQuantity = fillings.Sum(f => f.Quantity);
                    
                    // Получаем количество из FillingWarehouse для этого продукта и склада
                    var fillingKey = new { ProductId = batch.ProductId, WarehouseId = batch.WarehouseId };
                    var totalFillingQuantity = fillingByProductWarehouse.TryGetValue(fillingKey, out var fillingQty) ? fillingQty : 0;
                    
                    // Неответственное количество = количество в FillingWarehouse минус ответственное
                    dto.UnassignedQuantity = Math.Max(0, totalFillingQuantity - dto.ResponsibleQuantity.Value);
                }
                else
                {
                    // Нет ответственного - берем количество из FillingWarehouse
                    var fillingKey = new { ProductId = batch.ProductId, WarehouseId = batch.WarehouseId };
                    dto.UnassignedQuantity = fillingByProductWarehouse.TryGetValue(fillingKey, out var fillingQty) ? fillingQty : 0;
                }

                return dto;
            }).ToList();

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all product batches");
            return StatusCode(500, new { message = "An error occurred while retrieving batches" });
        }
    }

    // GET: api/product-batches/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var batch = await _batchService.GetBatchByIdAsync(id);
            if (batch == null)
                return NotFound(new { message = $"Batch with ID {id} not found" });

            return Ok(batch);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting batch {BatchId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the batch" });
        }
    }

    // GET: api/product-batches/product/{productId}
    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetByProduct(int productId)
    {
        try
        {
            var batches = await _batchService.GetBatchesByProductAsync(productId);
            return Ok(batches);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting batches for product {ProductId}", productId);
            return StatusCode(500, new { message = "An error occurred while retrieving batches" });
        }
    }

    // GET: api/product-batches/warehouse/{warehouseId}
    [HttpGet("warehouse/{warehouseId}")]
    public async Task<IActionResult> GetByWarehouse(int warehouseId)
    {
        try
        {
            var batches = await _batchService.GetBatchesByWarehouseAsync(warehouseId);
            return Ok(batches);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting batches for warehouse {WarehouseId}", warehouseId);
            return StatusCode(500, new { message = "An error occurred while retrieving batches" });
        }
    }

    // GET: api/product-batches/user/{userId}
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(int userId)
    {
        try
        {
            var batches = await _batchService.GetBatchesByUserAsync(userId);
            return Ok(batches);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting batches for user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while retrieving batches" });
        }
    }

    // POST: api/product-batches
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductBatch batch)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid model state", errors = ModelState });

        try
        {
            var userId = GetUserIdFromHeader();
            if (userId.HasValue)
            {
                batch.CreatedByUserId = userId.Value;
            }

            var created = await _batchService.CreateBatchAsync(batch);

            await TryLogAsync(userId, new HistoryEvent
            {
                UserId = userId ?? 0,
                Action = "ProductBatch.Created",
                EntityType = "ProductBatch",
                EntityId = created.Id,
                ProductId = created.ProductId,
                WarehouseId = created.WarehouseId,
                Description = $"Создана партия: {created.BatchNumber}, количество {created.Quantity}"
            });

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product batch");
            return StatusCode(500, new { message = "An error occurred while creating the batch" });
        }
    }

    // PUT: api/product-batches/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProductBatch updated)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid model state", errors = ModelState });

        try
        {
            var batch = await _batchService.UpdateBatchAsync(id, updated);

            await TryLogAsync(GetUserIdFromHeader(), new HistoryEvent
            {
                UserId = GetUserIdFromHeader() ?? 0,
                Action = "ProductBatch.Updated",
                EntityType = "ProductBatch",
                EntityId = batch.Id,
                ProductId = batch.ProductId,
                WarehouseId = batch.WarehouseId,
                Description = $"Обновлена партия: {batch.BatchNumber}"
            });

            return Ok(batch);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating batch {BatchId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the batch" });
        }
    }

    // DELETE: api/product-batches/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _batchService.DeleteBatchAsync(id);
            if (!deleted)
                return NotFound(new { message = $"Batch with ID {id} not found" });

            await TryLogAsync(GetUserIdFromHeader(), new HistoryEvent
            {
                UserId = GetUserIdFromHeader() ?? 0,
                Action = "ProductBatch.Deleted",
                EntityType = "ProductBatch",
                EntityId = id,
                Description = $"Удалена партия ID {id}"
            });

            return Ok(new { message = "Batch deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting batch {BatchId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the batch" });
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
