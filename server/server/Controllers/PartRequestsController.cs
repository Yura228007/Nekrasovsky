using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PartRequestsController : ControllerBase
    {
        private readonly IPartRequestService _partRequestService;

        public PartRequestsController(IPartRequestService partRequestService)
        {
            _partRequestService = partRequestService;
        }

        // GET: api/part-requests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PartRequest>>> GetAll()
        {
            var requests = await _partRequestService.GetAllPartRequestsAsync();
            return Ok(requests);
        }

        // GET: api/part-requests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PartRequest>> GetById(int id)
        {
            var request = await _partRequestService.GetPartRequestByIdAsync(id);
            if (request == null)
                return NotFound($"PartRequest with ID {id} not found");

            return Ok(request);
        }

        // GET: api/part-requests/status/{status}
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<PartRequest>>> GetByStatus(PartRequestStatus status)
        {
            var requests = await _partRequestService.GetPartRequestsByStatusAsync(status);
            return Ok(requests);
        }

        // GET: api/part-requests/user/{userId}?sent=true
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<PartRequest>>> GetByUser(int userId, [FromQuery] bool sent = true)
        {
            var requests = await _partRequestService.GetPartRequestsByUserAsync(userId, sent);
            return Ok(requests);
        }

        // GET: api/part-requests/warehouse/{warehouseId}?from=true
        [HttpGet("warehouse/{warehouseId}")]
        public async Task<ActionResult<IEnumerable<PartRequest>>> GetByWarehouse(int warehouseId, [FromQuery] bool from = true)
        {
            var requests = await _partRequestService.GetPartRequestsByWarehouseAsync(warehouseId, from);
            return Ok(requests);
        }

        // GET: api/part-requests/material/{materialId}
        [HttpGet("material/{materialId}")]
        public async Task<ActionResult<IEnumerable<PartRequest>>> GetByMaterial(int materialId)
        {
            var requests = await _partRequestService.GetPartRequestsByMaterialAsync(materialId);
            return Ok(requests);
        }

        // POST: api/part-requests/add
        [HttpPost("add")]
        public async Task<IActionResult> AddPartRequest([FromBody] PartRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdRequest = await _partRequestService.CreatePartRequestAsync(request);
                return Ok(new { message = "PartRequest created successfully", request = createdRequest });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/part-requests/edit/{id}
        [HttpPost("edit/{id}")]
        public async Task<IActionResult> EditPartRequest(int id, [FromBody] PartRequest updated)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var request = await _partRequestService.UpdatePartRequestAsync(id, updated);
                return Ok(new { message = "PartRequest updated successfully", request });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/part-requests/{id}/approve
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                var request = await _partRequestService.ApprovePartRequestAsync(id);
                return Ok(new { message = "PartRequest approved successfully", request });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/part-requests/{id}/reject
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(int id, [FromBody] string? reason = null)
        {
            try
            {
                var request = await _partRequestService.RejectPartRequestAsync(id, reason);
                return Ok(new { message = "PartRequest rejected successfully", request });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/part-requests/delete/{id}
        [HttpPost("delete/{id}")]
        public async Task<IActionResult> DeletePartRequest(int id)
        {
            var deleted = await _partRequestService.DeletePartRequestAsync(id);
            if (!deleted)
                return NotFound($"PartRequest with ID {id} not found");

            return Ok(new { message = "PartRequest deleted successfully" });
        }
    }
}

