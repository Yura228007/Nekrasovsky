using server.Models;

namespace server.Services
{
    public interface IAlarmEventService
    {
        Task<IEnumerable<AlarmEvent>> GetAllAlarmEventsAsync();
        Task<AlarmEvent?> GetAlarmEventByIdAsync(int id);
        Task<IEnumerable<AlarmEvent>> GetAlarmEventsByUserAsync(int userId);
        Task<IEnumerable<AlarmEvent>> GetAlarmEventsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<AlarmEvent>> GetAlarmEventsByLocationAsync(string location);
        Task<AlarmEvent> CreateAlarmEventAsync(AlarmEvent alarmEvent);
        Task<AlarmEvent> UpdateAlarmEventAsync(int id, AlarmEvent updatedAlarmEvent);
        Task<bool> DeleteAlarmEventAsync(int id);
    }
}

