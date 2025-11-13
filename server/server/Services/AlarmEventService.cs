using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Data;

namespace server.Services
{
    public class AlarmEventService : IAlarmEventService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AlarmEventService> _logger;

        public AlarmEventService(AppDbContext context, ILogger<AlarmEventService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<AlarmEvent>> GetAllAlarmEventsAsync()
        {
            return await _context.AlarmEvents.ToListAsync();
        }

        public async Task<AlarmEvent?> GetAlarmEventByIdAsync(int id)
        {
            return await _context.AlarmEvents.FindAsync(id);
        }

        public async Task<IEnumerable<AlarmEvent>> GetAlarmEventsByUserAsync(int userId)
        {
            return await _context.AlarmEvents
                .Where(ae => ae.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<AlarmEvent>> GetAlarmEventsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.AlarmEvents
                .Where(ae => ae.CreatedAt >= startDate && ae.CreatedAt <= endDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<AlarmEvent>> GetAlarmEventsByLocationAsync(string location)
        {
            return await _context.AlarmEvents
                .Where(ae => EF.Functions.ILike(ae.Location, $"%{location}%"))
                .ToListAsync();
        }

        public async Task<AlarmEvent> CreateAlarmEventAsync(AlarmEvent alarmEvent)
        {
            // Check if user exists
            if (!await _context.Users.AnyAsync(u => u.Id == alarmEvent.UserId))
            {
                throw new KeyNotFoundException($"User with ID {alarmEvent.UserId} not found");
            }

            alarmEvent.CreatedAt = DateTime.UtcNow;

            _context.AlarmEvents.Add(alarmEvent);
            await _context.SaveChangesAsync();

            _logger.LogInformation("AlarmEvent created with ID: {EventId}, User: {UserId}, Location: {Location}", 
                alarmEvent.Id, alarmEvent.UserId, alarmEvent.Location);
            return alarmEvent;
        }

        public async Task<AlarmEvent> UpdateAlarmEventAsync(int id, AlarmEvent updatedAlarmEvent)
        {
            var alarmEvent = await _context.AlarmEvents.FindAsync(id);
            if (alarmEvent == null)
            {
                throw new KeyNotFoundException($"AlarmEvent with ID {id} not found");
            }

            alarmEvent.Message = updatedAlarmEvent.Message;
            alarmEvent.Location = updatedAlarmEvent.Location;

            await _context.SaveChangesAsync();

            _logger.LogInformation("AlarmEvent updated with ID: {EventId}", alarmEvent.Id);
            return alarmEvent;
        }

        public async Task<bool> DeleteAlarmEventAsync(int id)
        {
            var alarmEvent = await _context.AlarmEvents.FindAsync(id);
            if (alarmEvent == null)
            {
                return false;
            }

            _context.AlarmEvents.Remove(alarmEvent);
            await _context.SaveChangesAsync();

            _logger.LogInformation("AlarmEvent deleted with ID: {EventId}", id);
            return true;
        }
    }
}

