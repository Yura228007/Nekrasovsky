using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShiftTransfersController : ControllerBase
    {
        private readonly IShiftTransferService _shiftTransferService;
        private readonly ILogger<ShiftTransfersController> _logger;
        private readonly IHistoryService _historyService;

        public ShiftTransfersController(IShiftTransferService shiftTransferService, ILogger<ShiftTransfersController> logger, IHistoryService historyService)
        {
            _shiftTransferService = shiftTransferService;
            _logger = logger;
            _historyService = historyService;
        }

        // GET: api/shift-transfers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShiftTransfer>>> GetAll()
        {
            try
            {
                var transfers = await _shiftTransferService.GetAllShiftTransfersAsync();
                return Ok(transfers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all shift transfers");
                return StatusCode(500, new { message = "An error occurred while retrieving shift transfers" });
            }
        }

        // GET: api/shift-transfers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ShiftTransfer>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                var transfer = await _shiftTransferService.GetShiftTransferByIdAsync(id);
                if (transfer == null)
                {
                    _logger.LogWarning("ShiftTransfer with ID {ShiftTransferId} not found", id);
                    return NotFound(new { message = $"ShiftTransfer with ID {id} not found" });
                }

                return Ok(transfer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting shift transfer with ID {ShiftTransferId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the shift transfer" });
            }
        }

        // GET: api/shift-transfers/user/{userId}?sent=true
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ShiftTransfer>>> GetByUser(int userId, [FromQuery] bool sent = true)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new { message = "UserId must be greater than 0" });
                }

                var transfers = await _shiftTransferService.GetShiftTransfersByUserAsync(userId, sent);
                return Ok(transfers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting shift transfers for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while retrieving shift transfers" });
            }
        }

        // GET: api/shift-transfers/date/{date}
        [HttpGet("date/{date}")]
        public async Task<ActionResult<IEnumerable<ShiftTransfer>>> GetByDate(string date)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(date))
                {
                    return BadRequest(new { message = "Date cannot be empty" });
                }

                if (!DateTime.TryParse(date, out DateTime parsedDate))
                {
                    return BadRequest(new { message = "Date must be a valid DateTime format" });
                }

                var transfers = await _shiftTransferService.GetShiftTransfersByDateAsync(parsedDate);
                return Ok(transfers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting shift transfers by date");
                return StatusCode(500, new { message = "An error occurred while retrieving shift transfers" });
            }
        }

        // GET: api/shift-transfers/pending/{userId}
        [HttpGet("pending/{userId}")]
        public async Task<ActionResult<IEnumerable<ShiftTransfer>>> GetPending(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new { message = "UserId must be greater than 0" });
                }

                var transfers = await _shiftTransferService.GetPendingShiftTransfersAsync(userId);
                return Ok(transfers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting pending shift transfers for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while retrieving shift transfers" });
            }
        }

        // POST: api/shift-transfers
        [HttpPost]
        public async Task<IActionResult> CreateShiftTransfer([FromBody] ShiftTransfer transfer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid model state", errors = ModelState });
            }

            try
            {
                // Validate TransferDate - ensure it's not default
                if (transfer.TransferDate == default(DateTime))
                {
                    return BadRequest(new { message = "TransferDate cannot be empty or default" });
                }

                var createdTransfer = await _shiftTransferService.CreateShiftTransferAsync(transfer);
                _logger.LogInformation("ShiftTransfer created successfully with ID: {ShiftTransferId}", createdTransfer.Id);
                await TryLogAsync(GetUserIdFromHeader() ?? createdTransfer.FromUserId, new HistoryEvent
                {
                    Action = "ShiftTransfer.Created",
                    EntityType = "ShiftTransfer",
                    EntityId = createdTransfer.Id,
                    RelatedUserId = createdTransfer.ToUserId,
                    Description = $"Создана передача смены (от {createdTransfer.FromUserId} к {createdTransfer.ToUserId})"
                });
                return CreatedAtAction(nameof(GetById), new { id = createdTransfer.Id },
                    new { message = "ShiftTransfer created successfully", transfer = createdTransfer });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Key not found while creating shift transfer");
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while creating shift transfer");
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while creating shift transfer");
                return StatusCode(500, new { message = "An error occurred while saving the shift transfer to the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating shift transfer");
                return StatusCode(500, new { message = "An unexpected error occurred while creating the shift transfer" });
            }
        }

        // PUT: api/shift-transfers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShiftTransfer(int id, [FromBody] ShiftTransfer updated)
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
                // Validate TransferDate - ensure it's not default
                if (updated.TransferDate == default(DateTime))
                {
                    return BadRequest(new { message = "TransferDate cannot be empty or default" });
                }

                var transfer = await _shiftTransferService.UpdateShiftTransferAsync(id, updated);
                _logger.LogInformation("ShiftTransfer updated successfully with ID: {ShiftTransferId}", id);
                return Ok(new { message = "ShiftTransfer updated successfully", transfer });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "ShiftTransfer with ID {ShiftTransferId} not found for update", id);
                return NotFound(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating shift transfer with ID {ShiftTransferId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the shift transfer in the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating shift transfer with ID {ShiftTransferId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred while updating the shift transfer" });
            }
        }

        // POST: api/shift-transfers/{id}/confirm
        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> Confirm(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                var transfer = await _shiftTransferService.ConfirmShiftTransferAsync(id);
                _logger.LogInformation("ShiftTransfer confirmed successfully with ID: {ShiftTransferId}", id);
                await TryLogAsync(GetUserIdFromHeader() ?? transfer.ToUserId, new HistoryEvent
                {
                    Action = "ShiftTransfer.Confirmed",
                    EntityType = "ShiftTransfer",
                    EntityId = transfer.Id,
                    RelatedUserId = transfer.FromUserId,
                    Description = $"Подтверждена передача смены (от {transfer.FromUserId} к {transfer.ToUserId})"
                });
                return Ok(new { message = "ShiftTransfer confirmed successfully", transfer });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "ShiftTransfer with ID {ShiftTransferId} not found for confirmation", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while confirming shift transfer with ID {ShiftTransferId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred while confirming the shift transfer" });
            }
        }

        // POST: api/shift-transfers/{id}/cancel
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

                var transfer = await _shiftTransferService.GetShiftTransferByIdAsync(id);
                if (transfer == null)
                {
                    return NotFound(new { message = $"ShiftTransfer with ID {id} not found" });
                }

                if (transfer.FromUserId != userId)
                {
                    return Forbid();
                }

                if (transfer.IsConfirmed)
                {
                    return BadRequest(new { message = "Confirmed transfers cannot be cancelled" });
                }

                var deleted = await _shiftTransferService.DeleteShiftTransferAsync(id);
                if (!deleted)
                {
                    return NotFound(new { message = $"ShiftTransfer with ID {id} not found" });
                }

                await TryLogAsync(userId, new HistoryEvent
                {
                    Action = "ShiftTransfer.Cancelled",
                    EntityType = "ShiftTransfer",
                    EntityId = id,
                    RelatedUserId = transfer.ToUserId,
                    Description = $"Отменена передача смены ID {id}"
                });
                return Ok(new { message = "ShiftTransfer cancelled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while cancelling shift transfer with ID {ShiftTransferId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred while cancelling the shift transfer" });
            }
        }

        // DELETE: api/shift-transfers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShiftTransfer(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                var deleted = await _shiftTransferService.DeleteShiftTransferAsync(id);
                if (!deleted)
                {
                    _logger.LogWarning("ShiftTransfer with ID {ShiftTransferId} not found for deletion", id);
                    return NotFound(new { message = $"ShiftTransfer with ID {id} not found" });
                }

                _logger.LogInformation("ShiftTransfer deleted successfully with ID: {ShiftTransferId}", id);
                await TryLogAsync(GetUserIdFromHeader(), new HistoryEvent
                {
                    Action = "ShiftTransfer.Deleted",
                    EntityType = "ShiftTransfer",
                    EntityId = id,
                    Description = $"Удалена передача смены ID {id}"
                });
                return Ok(new { message = "ShiftTransfer deleted successfully" });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while deleting shift transfer with ID {ShiftTransferId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the shift transfer from the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting shift transfer with ID {ShiftTransferId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred while deleting the shift transfer" });
            }
        }

        private int? GetUserIdFromHeader()
        {
            if (Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) &&
                int.TryParse(userIdHeader.ToString(), out var userId))
            {
                return userId;
            }
            return null;
        }

        private async Task TryLogAsync(int? userId, HistoryEvent historyEvent)
        {
            if (!userId.HasValue || userId.Value <= 0)
            {
                return;
            }

            historyEvent.UserId = userId.Value;
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
}
