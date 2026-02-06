using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.Services;

namespace server.Controllers;

[ApiController]
[Route("api/finished-goods")]
public class FinishedGoodsController : ControllerBase
{
    private readonly IFinishedGoodsService _finishedGoodsService;
    private readonly ILogger<FinishedGoodsController> _logger;

    public FinishedGoodsController(
        IFinishedGoodsService finishedGoodsService,
        ILogger<FinishedGoodsController> logger)
    {
        _finishedGoodsService = finishedGoodsService;
        _logger = logger;
    }

    /// <summary>
    /// Оформить продажу готовой продукции
    /// </summary>
    [HttpPost("sale")]
    public async Task<ActionResult> ProcessSale([FromBody] ProductSaleRequest request)
    {
        if (!Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) ||
            !int.TryParse(userIdHeader.ToString(), out var userId) || userId <= 0)
        {
            return BadRequest(new { message = "X-User-Id header is required" });
        }

        try
        {
            await _finishedGoodsService.ProcessSaleAsync(userId, request.WarehouseId, request.ProductId, request.Quantity, request.MeasuringUnit);
            return Ok(new { message = "Продажа оформлена успешно" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing sale for product {ProductId} at warehouse {WarehouseId}", request.ProductId, request.WarehouseId);
            return StatusCode(500, new { message = "An error occurred while processing sale" });
        }
    }

    /// <summary>
    /// Отправить готовую продукцию в утиль
    /// </summary>
    [HttpPost("disposal")]
    public async Task<ActionResult> ProcessDisposal([FromBody] FinishedGoodsDisposalRequest request)
    {
        if (!Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) ||
            !int.TryParse(userIdHeader.ToString(), out var userId) || userId <= 0)
        {
            return BadRequest(new { message = "X-User-Id header is required" });
        }

        try
        {
            await _finishedGoodsService.ProcessDisposalAsync(userId, request.WarehouseId, request.ProductId, request.Quantity, request.MeasuringUnit);
            return Ok(new { message = "Продукция отправлена в утиль" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing disposal for product {ProductId} at warehouse {WarehouseId}", request.ProductId, request.WarehouseId);
            return StatusCode(500, new { message = "An error occurred while processing disposal" });
        }
    }
}

public class ProductSaleRequest
{
    public int WarehouseId { get; set; }
    public int ProductId { get; set; }
    public double Quantity { get; set; }
    public string? MeasuringUnit { get; set; }
}

public class FinishedGoodsDisposalRequest
{
    public int WarehouseId { get; set; }
    public int ProductId { get; set; }
    public double Quantity { get; set; }
    public string? MeasuringUnit { get; set; }
}
