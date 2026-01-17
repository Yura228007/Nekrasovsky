using Microsoft.AspNetCore.SignalR;

namespace server.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Подписка на уведомления об отказах (для администраторов и супервайзеров)
        /// </summary>
        public async Task SubscribeToRejectionNotifications()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "RejectionNotifications");
            _logger.LogInformation("Client {ConnectionId} subscribed to rejection notifications", Context.ConnectionId);
        }

        /// <summary>
        /// Отписка от уведомлений об отказах
        /// </summary>
        public async Task UnsubscribeFromRejectionNotifications()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "RejectionNotifications");
            _logger.LogInformation("Client {ConnectionId} unsubscribed from rejection notifications", Context.ConnectionId);
        }

        /// <summary>
        /// Подписка на уведомления о тревогах (для всех пользователей)
        /// </summary>
        public async Task SubscribeToAlarmNotifications()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "AlarmNotifications");
            _logger.LogInformation("Client {ConnectionId} subscribed to alarm notifications", Context.ConnectionId);
        }

        /// <summary>
        /// Отписка от уведомлений о тревогах
        /// </summary>
        public async Task UnsubscribeFromAlarmNotifications()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "AlarmNotifications");
            _logger.LogInformation("Client {ConnectionId} unsubscribed from alarm notifications", Context.ConnectionId);
        }
    }
}
