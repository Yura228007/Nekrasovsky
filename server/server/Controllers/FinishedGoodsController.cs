using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.Services;
using System;

namespace server.Controllers;

[ApiController]
[Route("api/finished-goods")]
public class FinishedGoodsController : ControllerBase
{
    private readonly IFinishedGoodsService _finishedGoodsService;
    private readonly ILogger<FinishedGoodsController> _logger;
    private readonly IHistoryService _historyService;

    public FinishedGoodsController(
        IFinishedGoodsService finishedGoodsService,
        ILogger<FinishedGoodsController> logger,
        IHistoryService historyService)
    {
        _finishedGoodsService = finishedGoodsService;
        _logger = logger;
        _historyService = historyService;
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
            
            await TryLogAsync(userId, new HistoryEvent
            {
                Action = "FinishedGoods.Sale",
                EntityType = "ProductSale",
                WarehouseId = request.WarehouseId,
                ProductId = request.ProductId,
                Description = $"Продажа готовой продукции: продукт ID {request.ProductId} на складе ID {request.WarehouseId}, количество: {request.Quantity} {request.MeasuringUnit ?? "шт"}"
            });
            
            return Ok(new { message = "Продажа оформлена успешно" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing sale for product {ProductId} at warehouse {WarehouseId}. Error: {Error}, StackTrace: {StackTrace}", 
                request.ProductId, request.WarehouseId, ex.Message, ex.StackTrace);
            return StatusCode(500, new { message = $"An error occurred while processing sale: {ex.Message}" });
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
            
            await TryLogAsync(userId, new HistoryEvent
            {
                Action = "FinishedGoods.Disposal",
                EntityType = "FinishedGoodsRequest",
                WarehouseId = request.WarehouseId,
                ProductId = request.ProductId,
                Description = $"Отправка готовой продукции в утиль: продукт ID {request.ProductId} на складе ID {request.WarehouseId}, количество: {request.Quantity} {request.MeasuringUnit ?? "шт"}"
            });
            
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

    private async Task TryLogAsync(int userId, HistoryEvent historyEvent)
    {
        if (userId <= 0) return;
        historyEvent.UserId = userId;
        try
        {
            await _historyService.AddEventAsync(historyEvent);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write history event");
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
