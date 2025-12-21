using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlarmEventsController : ControllerBase
    {
        private readonly IAlarmEventService _alarmEventService;
        private readonly ILogger<AlarmEventsController> _logger;

        public AlarmEventsController(IAlarmEventService alarmEventService, ILogger<AlarmEventsController> logger)
        {
            _alarmEventService = alarmEventService;
            _logger = logger;
        }

        // GET: api/alarm-events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AlarmEvent>>> GetAll()
        {
            try
            {
                var events = await _alarmEventService.GetAllAlarmEventsAsync();
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all alarm events");
                return StatusCode(500, new { message = "An error occurred while retrieving alarm events" });
            }
        }

        // GET: api/alarm-events/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AlarmEvent>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                var alarmEvent = await _alarmEventService.GetAlarmEventByIdAsync(id);
                if (alarmEvent == null)
                {
                    _logger.LogWarning("AlarmEvent with ID {AlarmEventId} not found", id);
                    return NotFound(new { message = $"AlarmEvent with ID {id} not found" });
                }

                return Ok(alarmEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting alarm event with ID {AlarmEventId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the alarm event" });
            }
        }

        // GET: api/alarm-events/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<AlarmEvent>>> GetByUser(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new { message = "UserId must be greater than 0" });
                }

                var events = await _alarmEventService.GetAlarmEventsByUserAsync(userId);
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting alarm events for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while retrieving alarm events" });
            }
        }

        // GET: api/alarm-events/date-range?startDate=&endDate=
        [HttpGet("date-range")]
        public async Task<ActionResult<IEnumerable<AlarmEvent>>> GetByDateRange([FromQuery] string? startDate, [FromQuery] string? endDate)
        {
            try
            {
                // Validate and parse startDate
                if (string.IsNullOrWhiteSpace(startDate))
                {
                    return BadRequest(new { message = "StartDate cannot be empty" });
                }

                if (!DateTime.TryParse(startDate, out DateTime parsedStartDate))
                {
                    return BadRequest(new { message = "StartDate must be a valid DateTime format" });
                }

                // Validate and parse endDate
                if (string.IsNullOrWhiteSpace(endDate))
                {
                    return BadRequest(new { message = "EndDate cannot be empty" });
                }

                if (!DateTime.TryParse(endDate, out DateTime parsedEndDate))
                {
                    return BadRequest(new { message = "EndDate must be a valid DateTime format" });
                }

                if (parsedStartDate > parsedEndDate)
                {
                    return BadRequest(new { message = "StartDate must be less than or equal to EndDate" });
                }

                var events = await _alarmEventService.GetAlarmEventsByDateRangeAsync(parsedStartDate, parsedEndDate);
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting alarm events by date range");
                return StatusCode(500, new { message = "An error occurred while retrieving alarm events" });
            }
        }

        // GET: api/alarm-events/location/{location}
        [HttpGet("location/{location}")]
        public async Task<ActionResult<IEnumerable<AlarmEvent>>> GetByLocation(string location)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest(new { message = "Location cannot be empty" });
                }

                var events = await _alarmEventService.GetAlarmEventsByLocationAsync(location);
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting alarm events by location '{Location}'", location);
                return StatusCode(500, new { message = "An error occurred while retrieving alarm events" });
            }
        }

        // POST: api/alarm-events
        [HttpPost]
        public async Task<IActionResult> CreateAlarmEvent([FromBody] AlarmEvent alarmEvent)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid model state", errors = ModelState });
            }

            try
            {
                // Validate CreatedAt - ensure it's not default or empty
                if (alarmEvent.CreatedAt == default(DateTime))
                {
                    alarmEvent.CreatedAt = DateTime.UtcNow;
                }

                // Validate Location - ensure it's not empty
                if (string.IsNullOrWhiteSpace(alarmEvent.Location))
                {
                    return BadRequest(new { message = "Location cannot be empty" });
                }

                var createdEvent = await _alarmEventService.CreateAlarmEventAsync(alarmEvent);
                _logger.LogInformation("AlarmEvent created successfully with ID: {AlarmEventId}", createdEvent.Id);
                return CreatedAtAction(nameof(GetById), new { id = createdEvent.Id },
                    new { message = "AlarmEvent created successfully", alarmEvent = createdEvent });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Key not found while creating alarm event");
                return NotFound(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while creating alarm event");
                return StatusCode(500, new { message = "An error occurred while saving the alarm event to the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating alarm event");
                return StatusCode(500, new { message = "An unexpected error occurred while creating the alarm event" });
            }
        }

        // PUT: api/alarm-events/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAlarmEvent(int id, [FromBody] AlarmEvent updated)
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
                // Validate CreatedAt - ensure it's not default or empty
                if (updated.CreatedAt == default(DateTime))
                {
                    return BadRequest(new { message = "CreatedAt cannot be empty or default" });
                }

                // Validate Location - ensure it's not empty
                if (string.IsNullOrWhiteSpace(updated.Location))
                {
                    return BadRequest(new { message = "Location cannot be empty" });
                }

                var alarmEvent = await _alarmEventService.UpdateAlarmEventAsync(id, updated);
                _logger.LogInformation("AlarmEvent updated successfully with ID: {AlarmEventId}", id);
                return Ok(new { message = "AlarmEvent updated successfully", alarmEvent });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "AlarmEvent with ID {AlarmEventId} not found for update", id);
                return NotFound(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating alarm event with ID {AlarmEventId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the alarm event in the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating alarm event with ID {AlarmEventId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred while updating the alarm event" });
            }
        }

        // DELETE: api/alarm-events/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlarmEvent(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                var deleted = await _alarmEventService.DeleteAlarmEventAsync(id);
                if (!deleted)
                {
                    _logger.LogWarning("AlarmEvent with ID {AlarmEventId} not found for deletion", id);
                    return NotFound(new { message = $"AlarmEvent with ID {id} not found" });
                }

                _logger.LogInformation("AlarmEvent deleted successfully with ID: {AlarmEventId}", id);
                return Ok(new { message = "AlarmEvent deleted successfully" });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while deleting alarm event with ID {AlarmEventId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the alarm event from the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting alarm event with ID {AlarmEventId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred while deleting the alarm event" });
            }
        }
    }
}

