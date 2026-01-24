namespace NekrasovskyAPP.Services
{
    public interface IAlarmNotificationService
    {
        /// <summary>
        /// Инициализирует сервис и подключается к SignalR
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Отправляет тревогу на сервер
        /// </summary>
        Task<bool> SendAlarmAsync(string location, string? message);

        /// <summary>
        /// Событие при получении тревоги от другого пользователя
        /// </summary>
        event EventHandler<AlarmNotificationEventArgs>? AlarmReceived;
    }

    public class AlarmNotificationEventArgs : EventArgs
    {
        public string Message { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
