using Microsoft.AspNetCore.SignalR.Client;

namespace NekrasovskyAPP.Services
{
    public interface ISignalRService
    {
        HubConnection? Connection { get; }
        bool IsConnected { get; }
        Task ConnectAsync();
        Task DisconnectAsync();
        Task SubscribeToAlarmNotificationsAsync();
        /// <summary>Регистрация подключения по Id пользователя (для команды разблокировки с админ-панели).</summary>
        Task RegisterUserIdAsync(int userId);
        void SetOnAlarmNotification(Action<string, string, string> callback);
        /// <summary>Подписка на команду разблокировки устройства (с админ-панели).</summary>
        void SetOnUnlockDevice(Action callback);
    }
}
