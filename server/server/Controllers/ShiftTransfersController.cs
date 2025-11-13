using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShiftTransfersController : ControllerBase
    {
        private readonly IShiftTransferService _shiftTransferService;

        public ShiftTransfersController(IShiftTransferService shiftTransferService)
        {
            _shiftTransferService = shiftTransferService;
        }

        // GET: api/shift-transfers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShiftTransfer>>> GetAll()
        {
            var transfers = await _shiftTransferService.GetAllShiftTransfersAsync();
            return Ok(transfers);
        }

        // GET: api/shift-transfers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ShiftTransfer>> GetById(int id)
        {
            var transfer = await _shiftTransferService.GetShiftTransferByIdAsync(id);
            if (transfer == null)
                return NotFound($"ShiftTransfer with ID {id} not found");

            return Ok(transfer);
        }

        // GET: api/shift-transfers/user/{userId}?sent=true
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ShiftTransfer>>> GetByUser(int userId, [FromQuery] bool sent = true)
        {
            var transfers = await _shiftTransferService.GetShiftTransfersByUserAsync(userId, sent);
            return Ok(transfers);
        }

        // GET: api/shift-transfers/date/{date}
        [HttpGet("date/{date}")]
        public async Task<ActionResult<IEnumerable<ShiftTransfer>>> GetByDate(DateTime date)
        {
            var transfers = await _shiftTransferService.GetShiftTransfersByDateAsync(date);
            return Ok(transfers);
        }

        // GET: api/shift-transfers/pending/{userId}
        [HttpGet("pending/{userId}")]
        public async Task<ActionResult<IEnumerable<ShiftTransfer>>> GetPending(int userId)
        {
            var transfers = await _shiftTransferService.GetPendingShiftTransfersAsync(userId);
            return Ok(transfers);
        }

        // POST: api/shift-transfers/add
        [HttpPost("add")]
        public async Task<IActionResult> AddShiftTransfer([FromBody] ShiftTransfer transfer)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdTransfer = await _shiftTransferService.CreateShiftTransferAsync(transfer);
                return Ok(new { message = "ShiftTransfer created successfully", transfer = createdTransfer });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/shift-transfers/edit/{id}
        [HttpPost("edit/{id}")]
        public async Task<IActionResult> EditShiftTransfer(int id, [FromBody] ShiftTransfer updated)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var transfer = await _shiftTransferService.UpdateShiftTransferAsync(id, updated);
                return Ok(new { message = "ShiftTransfer updated successfully", transfer });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/shift-transfers/{id}/confirm
        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> Confirm(int id)
        {
            try
            {
                var transfer = await _shiftTransferService.ConfirmShiftTransferAsync(id);
                return Ok(new { message = "ShiftTransfer confirmed successfully", transfer });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/shift-transfers/delete/{id}
        [HttpPost("delete/{id}")]
        public async Task<IActionResult> DeleteShiftTransfer(int id)
        {
            var deleted = await _shiftTransferService.DeleteShiftTransferAsync(id);
            if (!deleted)
                return NotFound($"ShiftTransfer with ID {id} not found");

            return Ok(new { message = "ShiftTransfer deleted successfully" });
        }
    }
}

