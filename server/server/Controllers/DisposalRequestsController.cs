using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.Services;
using server.Data;
using System;

namespace server.Controllers;

[ApiController]
[Route("api/disposal-requests")]
public class DisposalRequestsController : ControllerBase
{
    private readonly IDisposalRequestService _disposalRequestService;
    private readonly ILogger<DisposalRequestsController> _logger;
    private readonly IHistoryService _historyService;
    private readonly AppDbContext _context;

    public DisposalRequestsController(
        IDisposalRequestService disposalRequestService,
        ILogger<DisposalRequestsController> logger,
        IHistoryService historyService,
        AppDbContext context)
    {
        _disposalRequestService = disposalRequestService;
        _logger = logger;
        _historyService = historyService;
        _context = context;
    }

    /// <summary>
    /// Получить все ожидающие запросы на склад утиля
    /// </summary>
    [HttpGet("pending/{disposalWarehouseId}")]
    public async Task<ActionResult<List<DisposalRequestDto>>> GetPendingRequests(int disposalWarehouseId)
    {
        try
        {
            var requests = await _disposalRequestService.GetPendingDisposalRequestsAsync(disposalWarehouseId);
            
            // Преобразуем в DTO с заполненными именами
            var dtos = requests.Select(r => new DisposalRequestDto
            {
                Id = r.Id,
                FromUserId = r.FromUserId,
                ApprovedByUserId = r.ApprovedByUserId,
                FromWarehouseId = r.FromWarehouseId,
                ToWarehouseId = r.ToWarehouseId,
                MaterialId = r.MaterialId,
                ProductId = r.ProductId,
                Quantity = r.Quantity,
                MeasuringUnit = r.MeasuringUnit,
                RequestType = r.RequestType,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                ProcessedAt = r.ProcessedAt,
                MaterialName = r.Material?.Name,
                ProductName = r.Product?.Name,
                FromUserName = r.FromUser != null ? $"{r.FromUser.Surname} {r.FromUser.Name}" : null,
                FromWarehouseName = r.FromWarehouse?.Name,
                ToWarehouseName = r.ToWarehouse?.Name
            }).ToList();
            
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending disposal requests for warehouse {WarehouseId}", disposalWarehouseId);
            return StatusCode(500, new { message = "An error occurred while retrieving disposal requests" });
        }
    }

    /// <summary>
    /// Подтвердить запрос на утиль
    /// </summary>
    [HttpPost("{requestId}/approve")]
    public async Task<ActionResult<DisposalRequest>> ApproveRequest(int requestId)
    {
        if (!Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) ||
            !int.TryParse(userIdHeader.ToString(), out var userId) || userId <= 0)
        {
            return BadRequest(new { message = "X-User-Id header is required" });
        }

        try
        {
            var request = await _disposalRequestService.ApproveDisposalRequestAsync(requestId, userId);
            
            var itemType = request.MaterialId.HasValue ? "Материал" : "Продукт";
            var itemId = request.MaterialId ?? request.ProductId ?? 0;
            await TryLogAsync(userId, new HistoryEvent
            {
                Action = "DisposalRequest.Approved",
                EntityType = "DisposalRequest",
                EntityId = requestId,
                WarehouseId = request.ToWarehouseId,
                MaterialId = request.MaterialId,
                ProductId = request.ProductId,
                Description = $"Одобрен запрос на утиль ID {requestId}: {itemType} ID {itemId}, количество: {request.Quantity} {request.MeasuringUnit ?? "шт"}"
            });
            
            return Ok(new { message = "Disposal request approved", request });
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
            _logger.LogError(ex, "Error approving disposal request {RequestId}", requestId);
            return StatusCode(500, new { message = "An error occurred while approving disposal request" });
        }
    }

    /// <summary>
    /// Отклонить запрос на утиль
    /// </summary>
    [HttpPost("{requestId}/reject")]
    public async Task<ActionResult<DisposalRequest>> RejectRequest(int requestId)
    {
        try
        {
            var request = await _disposalRequestService.RejectDisposalRequestAsync(requestId);
            
            var itemType = request.MaterialId.HasValue ? "Материал" : "Продукт";
            var itemId = request.MaterialId ?? request.ProductId ?? 0;
            await TryLogAsync(request.FromUserId, new HistoryEvent
            {
                Action = "DisposalRequest.Rejected",
                EntityType = "DisposalRequest",
                EntityId = requestId,
                WarehouseId = request.ToWarehouseId,
                MaterialId = request.MaterialId,
                ProductId = request.ProductId,
                Description = $"Отклонен запрос на утиль ID {requestId}: {itemType} ID {itemId}, количество: {request.Quantity} {request.MeasuringUnit ?? "шт"}"
            });
            
            return Ok(new { message = "Disposal request rejected", request });
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
            _logger.LogError(ex, "Error rejecting disposal request {RequestId}", requestId);
            return StatusCode(500, new { message = "An error occurred while rejecting disposal request" });
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
