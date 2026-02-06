using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.Services;

namespace server.Controllers;

[Route("api/finished-goods-requests")]
[ApiController]
public class FinishedGoodsRequestsController : ControllerBase
{
    private readonly IFinishedGoodsRequestService _finishedGoodsRequestService;
    private readonly ILogger<FinishedGoodsRequestsController> _logger;

    public FinishedGoodsRequestsController(
        IFinishedGoodsRequestService finishedGoodsRequestService,
        ILogger<FinishedGoodsRequestsController> logger)
    {
        _finishedGoodsRequestService = finishedGoodsRequestService;
        _logger = logger;
    }

    /// <summary>
    /// Создать запрос на перемещение продукции на склад готовой продукции
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<FinishedGoodsRequest>> CreateFinishedGoodsRequest([FromBody] CreateFinishedGoodsRequestDto dto)
    {
        try
        {
            var userId = GetUserIdFromHeader();
            if (!userId.HasValue)
                return Unauthorized(new { message = "User ID is required (X-User-Id)." });

            var request = await _finishedGoodsRequestService.CreateFinishedGoodsRequestAsync(
                userId.Value,
                dto.FromWarehouseId,
                dto.ToWarehouseId,
                dto.ProductId,
                dto.Quantity,
                dto.MeasuringUnit,
                dto.RequestType,
                dto.ProductOutputId);

            return CreatedAtAction(nameof(GetById), new { id = request.Id }, request);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating finished goods request");
            return StatusCode(500, new { message = "An error occurred while creating the request" });
        }
    }

    /// <summary>
    /// Получить все ожидающие запросы для склада готовой продукции
    /// </summary>
    [HttpGet("pending/{warehouseId}")]
    public async Task<ActionResult<List<FinishedGoodsRequest>>> GetPendingRequests(int warehouseId)
    {
        try
        {
            var requests = await _finishedGoodsRequestService.GetPendingFinishedGoodsRequestsAsync(warehouseId);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending finished goods requests");
            return StatusCode(500, new { message = "An error occurred while getting requests" });
        }
    }

    /// <summary>
    /// Получить запрос по ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<FinishedGoodsRequest>> GetById(int id)
    {
        // TODO: Implement if needed
        return NotFound();
    }

    /// <summary>
    /// Одобрить запрос
    /// </summary>
    [HttpPost("{id}/approve")]
    public async Task<ActionResult<FinishedGoodsRequest>> ApproveRequest(int id)
    {
        try
        {
            var userId = GetUserIdFromHeader();
            if (!userId.HasValue)
                return Unauthorized(new { message = "User ID is required (X-User-Id)." });

            var request = await _finishedGoodsRequestService.ApproveFinishedGoodsRequestAsync(id, userId.Value);
            return Ok(request);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving finished goods request");
            return StatusCode(500, new { message = "An error occurred while approving the request" });
        }
    }

    /// <summary>
    /// Отклонить запрос
    /// </summary>
    [HttpPost("{id}/reject")]
    public async Task<ActionResult<FinishedGoodsRequest>> RejectRequest(int id)
    {
        try
        {
            var userId = GetUserIdFromHeader();
            if (!userId.HasValue)
                return Unauthorized(new { message = "User ID is required (X-User-Id)." });

            var request = await _finishedGoodsRequestService.RejectFinishedGoodsRequestAsync(id, userId.Value);
            return Ok(request);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting finished goods request");
            return StatusCode(500, new { message = "An error occurred while rejecting the request" });
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
}

public class CreateFinishedGoodsRequestDto
{
    public int FromWarehouseId { get; set; }
    public int ToWarehouseId { get; set; }
    public int ProductId { get; set; }
    public double Quantity { get; set; }
    public string? MeasuringUnit { get; set; }
    public FinishedGoodsRequestType RequestType { get; set; }
    public int? ProductOutputId { get; set; }
}
