using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.Services;

namespace server.Controllers;

[ApiController]
[Route("api/product-movement-requests")]
public class ProductMovementRequestsController : ControllerBase
{
    private readonly IProductMovementRequestService _requestService;
    private readonly IHistoryService _historyService;
    private readonly ILogger<ProductMovementRequestsController> _logger;

    public ProductMovementRequestsController(
        IProductMovementRequestService requestService,
        IHistoryService historyService,
        ILogger<ProductMovementRequestsController> logger)
    {
        _requestService = requestService;
        _historyService = historyService;
        _logger = logger;
    }

    // GET: api/product-movement-requests
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var requests = await _requestService.GetAllRequestsAsync();
            return Ok(requests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all product movement requests");
            return StatusCode(500, new { message = "An error occurred while retrieving requests" });
        }
    }

    // GET: api/product-movement-requests/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var request = await _requestService.GetRequestByIdAsync(id);
            if (request == null)
                return NotFound(new { message = $"Request with ID {id} not found" });

            return Ok(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting request {RequestId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the request" });
        }
    }

    // GET: api/product-movement-requests/status/{status}
    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(ProductMovementStatus status)
    {
        try
        {
            var requests = await _requestService.GetRequestsByStatusAsync(status);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting requests by status {Status}", status);
            return StatusCode(500, new { message = "An error occurred while retrieving requests" });
        }
    }

    // GET: api/product-movement-requests/user/{userId}/sent
    [HttpGet("user/{userId}/sent")]
    public async Task<IActionResult> GetSentByUser(int userId)
    {
        try
        {
            var requests = await _requestService.GetRequestsByUserAsync(userId, sent: true);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sent requests for user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while retrieving requests" });
        }
    }

    // GET: api/product-movement-requests/user/{userId}/received
    [HttpGet("user/{userId}/received")]
    public async Task<IActionResult> GetReceivedByUser(int userId)
    {
        try
        {
            var requests = await _requestService.GetRequestsByUserAsync(userId, sent: false);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting received requests for user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while retrieving requests" });
        }
    }

    // POST: api/product-movement-requests
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductMovementRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid model state", errors = ModelState });

        try
        {
            var userId = GetUserIdFromHeader();
            if (userId.HasValue)
            {
                request.FromUserId = userId.Value;
            }

            var created = await _requestService.CreateRequestAsync(request);

            await TryLogAsync(userId, new HistoryEvent
            {
                UserId = userId ?? 0,
                Action = "ProductMovementRequest.Created",
                EntityType = "ProductMovementRequest",
                EntityId = created.Id,
                Description = $"Создана заявка на перемещение партии {created.ProductBatchId}"
            });

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
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
            _logger.LogError(ex, "Error creating product movement request");
            return StatusCode(500, new { message = "An error occurred while creating the request" });
        }
    }

    // PUT: api/product-movement-requests/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProductMovementRequest updated)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid model state", errors = ModelState });

        try
        {
            var request = await _requestService.UpdateRequestAsync(id, updated);

            await TryLogAsync(GetUserIdFromHeader(), new HistoryEvent
            {
                UserId = GetUserIdFromHeader() ?? 0,
                Action = "ProductMovementRequest.Updated",
                EntityType = "ProductMovementRequest",
                EntityId = request.Id,
                Description = $"Обновлена заявка на перемещение {request.Id}"
            });

            return Ok(request);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating request {RequestId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the request" });
        }
    }

    // POST: api/product-movement-requests/{id}/approve
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        try
        {
            var request = await _requestService.ApproveRequestAsync(id);

            await TryLogAsync(GetUserIdFromHeader(), new HistoryEvent
            {
                UserId = GetUserIdFromHeader() ?? 0,
                Action = "ProductMovementRequest.Approved",
                EntityType = "ProductMovementRequest",
                EntityId = request.Id,
                Description = $"Одобрена заявка на перемещение партии {request.ProductBatchId}"
            });

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
            _logger.LogError(ex, "Error approving request {RequestId}", id);
            return StatusCode(500, new { message = "An error occurred while approving the request" });
        }
    }

    // POST: api/product-movement-requests/{id}/reject
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectRequest rejectData)
    {
        try
        {
            var request = await _requestService.RejectRequestAsync(id, rejectData?.Reason);

            await TryLogAsync(GetUserIdFromHeader(), new HistoryEvent
            {
                UserId = GetUserIdFromHeader() ?? 0,
                Action = "ProductMovementRequest.Rejected",
                EntityType = "ProductMovementRequest",
                EntityId = request.Id,
                Description = $"Отклонена заявка на перемещение партии {request.ProductBatchId}"
            });

            return Ok(request);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting request {RequestId}", id);
            return StatusCode(500, new { message = "An error occurred while rejecting the request" });
        }
    }

    // DELETE: api/product-movement-requests/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _requestService.DeleteRequestAsync(id);
            if (!deleted)
                return NotFound(new { message = $"Request with ID {id} not found" });

            await TryLogAsync(GetUserIdFromHeader(), new HistoryEvent
            {
                UserId = GetUserIdFromHeader() ?? 0,
                Action = "ProductMovementRequest.Deleted",
                EntityType = "ProductMovementRequest",
                EntityId = id,
                Description = $"Удалена заявка на перемещение ID {id}"
            });

            return Ok(new { message = "Request deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting request {RequestId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the request" });
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

    public class RejectRequest
    {
        public string? Reason { get; set; }
    }
}
