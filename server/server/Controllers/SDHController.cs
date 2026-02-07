using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;
using server.Services;
using System;

namespace server.Controllers;

[ApiController]
[Route("api/sdh")]
public class SDHController : ControllerBase
{
    private readonly ISDHService _sdhService;
    private readonly ILogger<SDHController> _logger;
    private readonly IHistoryService _historyService;

    public SDHController(ISDHService sdhService, ILogger<SDHController> logger, IHistoryService historyService)
    {
        _sdhService = sdhService;
        _logger = logger;
        _historyService = historyService;
    }

    [HttpPost("non-returnable-defect")]
    public async Task<IActionResult> ProcessNonReturnableDefect([FromBody] SDHProcessRequest request)
    {
        try
        {
            var userId = GetUserIdFromRequest();
            if (!userId.HasValue)
                return Unauthorized(new { message = "User ID is required" });

            await _sdhService.ProcessNonReturnableDefectAsync(
                userId.Value,
                request.WarehouseId,
                request.MaterialId,
                request.ProductId,
                request.Quantity,
                request.MeasuringUnit);

            var itemType = request.MaterialId.HasValue ? "Материал" : "Продукт";
            var itemId = request.MaterialId ?? request.ProductId ?? 0;
            await TryLogAsync(userId.Value, new HistoryEvent
            {
                Action = "SDH.NonReturnableDefect",
                EntityType = "MaterialSDHSale",
                WarehouseId = request.WarehouseId,
                MaterialId = request.MaterialId,
                ProductId = request.ProductId,
                Description = $"Невозвратный брак из СДХ: {itemType} ID {itemId} на складе ID {request.WarehouseId}, количество: {request.Quantity} {request.MeasuringUnit ?? "шт"}"
            });

            return Ok(new { message = "Невозвратный брак успешно списан" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SDH non-returnable defect");
            return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
        }
    }

    [HttpPost("sale")]
    public async Task<IActionResult> ProcessSale([FromBody] SDHProcessRequest request)
    {
        try
        {
            var userId = GetUserIdFromRequest();
            if (!userId.HasValue)
                return Unauthorized(new { message = "User ID is required" });

            await _sdhService.ProcessSaleAsync(
                userId.Value,
                request.WarehouseId,
                request.MaterialId,
                request.ProductId,
                request.Quantity,
                request.MeasuringUnit);

            var itemType = request.MaterialId.HasValue ? "Материал" : "Продукт";
            var itemId = request.MaterialId ?? request.ProductId ?? 0;
            await TryLogAsync(userId.Value, new HistoryEvent
            {
                Action = "SDH.Sale",
                EntityType = "MaterialSDHSale",
                WarehouseId = request.WarehouseId,
                MaterialId = request.MaterialId,
                ProductId = request.ProductId,
                Description = $"Продажа из СДХ: {itemType} ID {itemId} на складе ID {request.WarehouseId}, количество: {request.Quantity} {request.MeasuringUnit ?? "шт"}"
            });

            return Ok(new { message = "Продажа успешно оформлена" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SDH sale");
            return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
        }
    }

    [HttpPost("request")]
    public async Task<IActionResult> CreateRequest([FromBody] SDHCreateRequest request)
    {
        try
        {
            var userId = GetUserIdFromRequest();
            if (!userId.HasValue)
                return Unauthorized(new { message = "User ID is required" });

            await _sdhService.CreateSDHRequestAsync(
                userId.Value,
                request.FromWarehouseId,
                request.ToWarehouseId,
                request.MaterialId,
                request.ProductId,
                request.Quantity,
                request.MeasuringUnit);

            var itemType = request.MaterialId.HasValue ? "Материал" : "Продукт";
            var itemId = request.MaterialId ?? request.ProductId ?? 0;
            await TryLogAsync(userId.Value, new HistoryEvent
            {
                Action = "SDH.RequestCreated",
                EntityType = "SDHRequest",
                WarehouseId = request.ToWarehouseId,
                MaterialId = request.MaterialId,
                ProductId = request.ProductId,
                Description = $"Создан запрос на отправку на СДХ: {itemType} ID {itemId} со склада ID {request.FromWarehouseId} на склад ID {request.ToWarehouseId}, количество: {request.Quantity} {request.MeasuringUnit ?? "шт"}"
            });

            return Ok(new { message = "Запрос на отправку на СДХ создан" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating SDH request");
            return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
        }
    }

    [HttpGet("requests/{warehouseId}")]
    public async Task<ActionResult<List<SDHRequestDto>>> GetPendingRequests(int warehouseId)
    {
        try
        {
            using var context = HttpContext.RequestServices.GetRequiredService<server.Data.AppDbContext>();
            var requests = await context.SDHRequests
                .Where(r => r.ToWarehouseId == warehouseId && r.Status == SDHRequestStatus.Pending)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var requestDtos = new List<SDHRequestDto>();
            foreach (var request in requests)
            {
                var dto = new SDHRequestDto
                {
                    Id = request.Id,
                    FromUserId = request.FromUserId,
                    ApprovedByUserId = request.ApprovedByUserId,
                    FromWarehouseId = request.FromWarehouseId,
                    ToWarehouseId = request.ToWarehouseId,
                    MaterialId = request.MaterialId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    MeasuringUnit = request.MeasuringUnit,
                    Status = request.Status,
                    CreatedAt = request.CreatedAt,
                    ProcessedAt = request.ProcessedAt
                };

                // Заполняем имена
                if (request.MaterialId.HasValue)
                {
                    var material = await context.Materials.FindAsync(request.MaterialId.Value);
                    dto.MaterialName = material?.Name;
                }
                if (request.ProductId.HasValue)
                {
                    var product = await context.Products.FindAsync(request.ProductId.Value);
                    dto.ProductName = product?.Name;
                }
                if (request.FromUserId > 0)
                {
                    var fromUser = await context.Users.FindAsync(request.FromUserId);
                    dto.FromUserName = fromUser != null ? $"{fromUser.Surname} {fromUser.Name}" : null;
                }
                if (request.FromWarehouseId > 0)
                {
                    var fromWarehouse = await context.Warehouses.FindAsync(request.FromWarehouseId);
                    dto.FromWarehouseName = fromWarehouse?.Name;
                }
                if (request.ToWarehouseId > 0)
                {
                    var toWarehouse = await context.Warehouses.FindAsync(request.ToWarehouseId);
                    dto.ToWarehouseName = toWarehouse?.Name;
                }

                requestDtos.Add(dto);
            }

            return Ok(requestDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending SDH requests");
            return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
        }
    }

    [HttpPost("requests/{requestId}/approve")]
    public async Task<IActionResult> ApproveRequest(int requestId)
    {
        try
        {
            var userId = GetUserIdFromRequest();
            if (!userId.HasValue)
                return Unauthorized(new { message = "User ID is required" });

            await _sdhService.ApproveSDHRequestAsync(requestId, userId.Value);
            
            using var context = HttpContext.RequestServices.GetRequiredService<server.Data.AppDbContext>();
            var request = await context.SDHRequests.FindAsync(requestId);
            if (request != null)
            {
                var itemType = request.MaterialId.HasValue ? "Материал" : "Продукт";
                var itemId = request.MaterialId ?? request.ProductId ?? 0;
                await TryLogAsync(userId.Value, new HistoryEvent
                {
                    Action = "SDH.RequestApproved",
                    EntityType = "SDHRequest",
                    EntityId = requestId,
                    WarehouseId = request.ToWarehouseId,
                    MaterialId = request.MaterialId,
                    ProductId = request.ProductId,
                    Description = $"Одобрен запрос на СДХ ID {requestId}: {itemType} ID {itemId}, количество: {request.Quantity} {request.MeasuringUnit ?? "шт"}"
                });
            }
            
            return Ok(new { message = "Запрос подтвержден. Ответственность передана вам." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving SDH request");
            return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
        }
    }

    [HttpPost("requests/{requestId}/reject")]
    public async Task<IActionResult> RejectRequest(int requestId)
    {
        try
        {
            await _sdhService.RejectSDHRequestAsync(requestId);
            
            using var context = HttpContext.RequestServices.GetRequiredService<server.Data.AppDbContext>();
            var request = await context.SDHRequests.FindAsync(requestId);
            if (request != null)
            {
                var itemType = request.MaterialId.HasValue ? "Материал" : "Продукт";
                var itemId = request.MaterialId ?? request.ProductId ?? 0;
                await TryLogAsync(request.FromUserId, new HistoryEvent
                {
                    Action = "SDH.RequestRejected",
                    EntityType = "SDHRequest",
                    EntityId = requestId,
                    WarehouseId = request.ToWarehouseId,
                    MaterialId = request.MaterialId,
                    ProductId = request.ProductId,
                    Description = $"Отклонен запрос на СДХ ID {requestId}: {itemType} ID {itemId}, количество: {request.Quantity} {request.MeasuringUnit ?? "шт"}"
                });
            }
            
            return Ok(new { message = "Запрос отклонен. Ответственность осталась у создателя." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting SDH request");
            return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
        }
    }

    private int? GetUserIdFromRequest()
    {
        if (Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) &&
            int.TryParse(userIdHeader.ToString(), out var headerUserId))
        {
            return headerUserId;
        }

        if (Request.Query.TryGetValue("userId", out var userIdQuery) &&
            int.TryParse(userIdQuery.ToString(), out var queryUserId))
        {
            return queryUserId;
        }

        return null;
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

public class SDHProcessRequest
{
    public int WarehouseId { get; set; }
    public int? MaterialId { get; set; }
    public int? ProductId { get; set; }
    public double Quantity { get; set; }
    public string? MeasuringUnit { get; set; }
}

public class SDHCreateRequest
{
    public int FromWarehouseId { get; set; }
    public int ToWarehouseId { get; set; }
    public int? MaterialId { get; set; }
    public int? ProductId { get; set; }
    public double Quantity { get; set; }
    public string? MeasuringUnit { get; set; }
}

public class SDHRequestDto
{
    public int Id { get; set; }
    public int FromUserId { get; set; }
    public int? ApprovedByUserId { get; set; }
    public int FromWarehouseId { get; set; }
    public int ToWarehouseId { get; set; }
    public int? MaterialId { get; set; }
    public int? ProductId { get; set; }
    public double Quantity { get; set; }
    public string? MeasuringUnit { get; set; }
    public SDHRequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? MaterialName { get; set; }
    public string? ProductName { get; set; }
    public string? FromUserName { get; set; }
    public string? FromWarehouseName { get; set; }
    public string? ToWarehouseName { get; set; }
}
