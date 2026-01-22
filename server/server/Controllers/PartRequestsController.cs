using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Services;
using server.Hubs;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PartRequestsController : ControllerBase
    {
        private readonly IPartRequestService _partRequestService;
        private readonly ILogger<PartRequestsController> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IUserPermissionsService _userPermissionsService;

        public PartRequestsController(
            IPartRequestService partRequestService, 
            ILogger<PartRequestsController> logger,
            IHubContext<NotificationHub> hubContext,
            IUserPermissionsService userPermissionsService)
        {
            _partRequestService = partRequestService;
            _logger = logger;
            _hubContext = hubContext;
            _userPermissionsService = userPermissionsService;
        }

        // GET: api/part-requests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PartRequest>>> GetAll()
        {
            try
            {
                var requests = await _partRequestService.GetAllPartRequestsAsync();
                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all part requests");
                return StatusCode(500, new { message = "An error occurred while retrieving part requests" });
            }
        }

        // GET: api/part-requests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PartRequest>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                var request = await _partRequestService.GetPartRequestByIdAsync(id);
                if (request == null)
                {
                    _logger.LogWarning("PartRequest with ID {PartRequestId} not found", id);
                    return NotFound(new { message = $"PartRequest with ID {id} not found" });
                }

                return Ok(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting part request with ID {PartRequestId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the part request" });
            }
        }

        // GET: api/part-requests/status/{status}
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<PartRequest>>> GetByStatus(PartRequestStatus status)
        {
            try
            {
                var requests = await _partRequestService.GetPartRequestsByStatusAsync(status);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting part requests by status {Status}", status);
                return StatusCode(500, new { message = "An error occurred while retrieving part requests" });
            }
        }

        // GET: api/part-requests/user/{userId}?sent=true
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<PartRequest>>> GetByUser(int userId, [FromQuery] bool sent = true)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new { message = "UserId must be greater than 0" });
                }

                var requests = await _partRequestService.GetPartRequestsByUserAsync(userId, sent);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting part requests for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while retrieving part requests" });
            }
        }

        // GET: api/part-requests/warehouse/{warehouseId}?from=true
        [HttpGet("warehouse/{warehouseId}")]
        public async Task<ActionResult<IEnumerable<PartRequest>>> GetByWarehouse(int warehouseId, [FromQuery] bool from = true)
        {
            try
            {
                if (warehouseId <= 0)
                {
                    return BadRequest(new { message = "WarehouseId must be greater than 0" });
                }

                var requests = await _partRequestService.GetPartRequestsByWarehouseAsync(warehouseId, from);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting part requests for warehouse {WarehouseId}", warehouseId);
                return StatusCode(500, new { message = "An error occurred while retrieving part requests" });
            }
        }

        // GET: api/part-requests/material/{materialId}
        [HttpGet("material/{materialId}")]
        public async Task<ActionResult<IEnumerable<PartRequest>>> GetByMaterial(int materialId)
        {
            try
            {
                if (materialId <= 0)
                {
                    return BadRequest(new { message = "MaterialId must be greater than 0" });
                }

                var requests = await _partRequestService.GetPartRequestsByMaterialAsync(materialId);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting part requests for material {MaterialId}", materialId);
                return StatusCode(500, new { message = "An error occurred while retrieving part requests" });
            }
        }

        // POST: api/part-requests
        [HttpPost]
        public async Task<IActionResult> CreatePartRequest([FromBody] PartRequest? request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Request body is required" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid model state", errors = ModelState });
            }

            try
            {
                var createdRequest = await _partRequestService.CreatePartRequestAsync(request);
                _logger.LogInformation("PartRequest created successfully with ID: {PartRequestId}", createdRequest.Id);
                return CreatedAtAction(nameof(GetById), new { id = createdRequest.Id },
                    new { message = "PartRequest created successfully", request = createdRequest });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Key not found while creating part request");
                return NotFound(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while creating part request");
                return StatusCode(500, new { message = "An error occurred while saving the part request to the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating part request");
                return StatusCode(500, new { message = "An unexpected error occurred while creating the part request" });
            }
        }

        // PUT: api/part-requests/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePartRequest(int id, [FromBody] PartRequest updated)
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
                var request = await _partRequestService.UpdatePartRequestAsync(id, updated);
                _logger.LogInformation("PartRequest updated successfully with ID: {PartRequestId}", id);
                return Ok(new { message = "PartRequest updated successfully", request });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "PartRequest with ID {PartRequestId} not found for update", id);
                return NotFound(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating part request with ID {PartRequestId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the part request in the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating part request with ID {PartRequestId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred while updating the part request" });
            }
        }

        // POST: api/part-requests/{id}/approve
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                var request = await _partRequestService.ApprovePartRequestAsync(id);
                _logger.LogInformation("PartRequest approved successfully with ID: {PartRequestId}", id);
                return Ok(new { message = "PartRequest approved successfully", request });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "PartRequest with ID {PartRequestId} not found for approval", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while approving part request with ID {PartRequestId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred while approving the part request" });
            }
        }

        // POST: api/part-requests/{id}/reject
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(int id, [FromBody] string? reason = null)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                // Получаем запрос перед отклонением, чтобы узнать FromUserId и ToUserId
                var requestBeforeReject = await _partRequestService.GetPartRequestByIdAsync(id);
                if (requestBeforeReject == null)
                {
                    return NotFound(new { message = $"PartRequest with ID {id} not found" });
                }

                var request = await _partRequestService.RejectPartRequestAsync(id, reason);
                
                // Проверяем количество отказов и отправляем уведомление при достижении 3 отказов
                var rejectionCount = await _partRequestService.GetRejectionCountAsync(
                    request.FromUserId, request.ToUserId);

                if (rejectionCount >= 3)
                {
                    // Получаем информацию о пользователях для уведомления
                    var userService = HttpContext.RequestServices.GetRequiredService<IUserService>();
                    var fromUser = await userService.GetUserByIdAsync(request.FromUserId);
                    var toUser = await userService.GetUserByIdAsync(request.ToUserId);

                    var notificationMessage = new
                    {
                        type = "RejectionThreshold",
                        message = $"Пользователь {fromUser?.Name} {fromUser?.Surname} (ID: {request.FromUserId}) получил {rejectionCount} отказов от пользователя {toUser?.Name} {toUser?.Surname} (ID: {request.ToUserId}). Требуется внимание администратора.",
                        fromUserId = request.FromUserId,
                        toUserId = request.ToUserId,
                        rejectionCount = rejectionCount,
                        timestamp = DateTime.UtcNow
                    };

                    // Отправляем уведомление всем подписанным на группу "RejectionNotifications" (администраторы и супервайзеры)
                    await _hubContext.Clients.Group("RejectionNotifications")
                        .SendAsync("RejectionNotification", notificationMessage);

                    _logger.LogWarning(
                        "Rejection threshold reached! User {FromUserId} has {RejectionCount} rejections from user {ToUserId}. Notification sent.",
                        request.FromUserId, rejectionCount, request.ToUserId);
                }

                _logger.LogInformation("PartRequest rejected successfully with ID: {PartRequestId}", id);
                return Ok(new { message = "PartRequest rejected successfully", request, rejectionCount });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "PartRequest with ID {PartRequestId} not found for rejection", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while rejecting part request with ID {PartRequestId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred while rejecting the part request" });
            }
        }

        // POST: api/part-requests/{id}/cancel
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                if (!Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) ||
                    !int.TryParse(userIdHeader.ToString(), out var userId))
                {
                    return BadRequest(new { message = "X-User-Id header is required" });
                }

                var request = await _partRequestService.GetPartRequestByIdAsync(id);
                if (request == null)
                {
                    return NotFound(new { message = $"PartRequest with ID {id} not found" });
                }

                if (request.FromUserId != userId)
                {
                    return Forbid();
                }

                if (request.Status != PartRequestStatus.Pending)
                {
                    return BadRequest(new { message = "Only pending requests can be cancelled" });
                }

                var deleted = await _partRequestService.DeletePartRequestAsync(id);
                if (!deleted)
                {
                    return NotFound(new { message = $"PartRequest with ID {id} not found" });
                }

                return Ok(new { message = "PartRequest cancelled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while cancelling part request with ID {PartRequestId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred while cancelling the part request" });
            }
        }

        // GET: api/part-requests/rejection-count?fromUserId=&toUserId=
        [HttpGet("rejection-count")]
        public async Task<ActionResult<int>> GetRejectionCount([FromQuery] int fromUserId, [FromQuery] int toUserId)
        {
            try
            {
                if (fromUserId <= 0 || toUserId <= 0)
                {
                    return BadRequest(new { message = "FromUserId and ToUserId must be greater than 0" });
                }

                var count = await _partRequestService.GetRejectionCountAsync(fromUserId, toUserId);
                return Ok(new { fromUserId, toUserId, rejectionCount = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting rejection count");
                return StatusCode(500, new { message = "An error occurred while retrieving rejection count" });
            }
        }

        // DELETE: api/part-requests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePartRequest(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                var deleted = await _partRequestService.DeletePartRequestAsync(id);
                if (!deleted)
                {
                    _logger.LogWarning("PartRequest with ID {PartRequestId} not found for deletion", id);
                    return NotFound(new { message = $"PartRequest with ID {id} not found" });
                }

                _logger.LogInformation("PartRequest deleted successfully with ID: {PartRequestId}", id);
                return Ok(new { message = "PartRequest deleted successfully" });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while deleting part request with ID {PartRequestId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the part request from the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting part request with ID {PartRequestId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred while deleting the part request" });
            }
        }
    }
}
