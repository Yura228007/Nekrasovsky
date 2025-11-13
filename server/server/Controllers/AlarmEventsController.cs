using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlarmEventsController : ControllerBase
    {
        private readonly IAlarmEventService _alarmEventService;

        public AlarmEventsController(IAlarmEventService alarmEventService)
        {
            _alarmEventService = alarmEventService;
        }

        // GET: api/alarm-events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AlarmEvent>>> GetAll()
        {
            var events = await _alarmEventService.GetAllAlarmEventsAsync();
            return Ok(events);
        }

        // GET: api/alarm-events/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AlarmEvent>> GetById(int id)
        {
            var alarmEvent = await _alarmEventService.GetAlarmEventByIdAsync(id);
            if (alarmEvent == null)
                return NotFound($"AlarmEvent with ID {id} not found");

            return Ok(alarmEvent);
        }

        // GET: api/alarm-events/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<AlarmEvent>>> GetByUser(int userId)
        {
            var events = await _alarmEventService.GetAlarmEventsByUserAsync(userId);
            return Ok(events);
        }

        // GET: api/alarm-events/date-range?startDate=&endDate=
        [HttpGet("date-range")]
        public async Task<ActionResult<IEnumerable<AlarmEvent>>> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var events = await _alarmEventService.GetAlarmEventsByDateRangeAsync(startDate, endDate);
            return Ok(events);
        }

        // GET: api/alarm-events/location/{location}
        [HttpGet("location/{location}")]
        public async Task<ActionResult<IEnumerable<AlarmEvent>>> GetByLocation(string location)
        {
            var events = await _alarmEventService.GetAlarmEventsByLocationAsync(location);
            return Ok(events);
        }

        // POST: api/alarm-events/add
        [HttpPost("add")]
        public async Task<IActionResult> AddAlarmEvent([FromBody] AlarmEvent alarmEvent)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdEvent = await _alarmEventService.CreateAlarmEventAsync(alarmEvent);
                return Ok(new { message = "AlarmEvent created successfully", alarmEvent = createdEvent });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/alarm-events/edit/{id}
        [HttpPost("edit/{id}")]
        public async Task<IActionResult> EditAlarmEvent(int id, [FromBody] AlarmEvent updated)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var alarmEvent = await _alarmEventService.UpdateAlarmEventAsync(id, updated);
                return Ok(new { message = "AlarmEvent updated successfully", alarmEvent });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/alarm-events/delete/{id}
        [HttpPost("delete/{id}")]
        public async Task<IActionResult> DeleteAlarmEvent(int id)
        {
            var deleted = await _alarmEventService.DeleteAlarmEventAsync(id);
            if (!deleted)
                return NotFound($"AlarmEvent with ID {id} not found");

            return Ok(new { message = "AlarmEvent deleted successfully" });
        }
    }
}

