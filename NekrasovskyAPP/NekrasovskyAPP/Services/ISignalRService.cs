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
        void SetOnAlarmNotification(Action<string, string, string> callback);
    }
}
