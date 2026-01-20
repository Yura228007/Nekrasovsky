using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace NekrasovskyAPP.Services
{
    public class SignalRService : ISignalRService, IDisposable
    {
        private HubConnection? _connection;
        private readonly ILogger<SignalRService>? _logger;
        private readonly string _baseUrl;
        private event Action<string, string, string>? OnAlarmNotificationReceived;

        public HubConnection? Connection => _connection;
        public bool IsConnected => _connection?.State == HubConnectionState.Connected;
        
        public void SetOnAlarmNotification(Action<string, string, string> callback)
        {
            OnAlarmNotificationReceived += callback;
        }

        public SignalRService(ILogger<SignalRService>? logger = null)
        {
            _logger = logger;
            _baseUrl = GetBaseUrl();
        }

        private static string GetBaseUrl()
        {
#if ANDROID
            return "http://192.168.1.121:9000/";
#else
            return "http://localhost:9000/";
#endif
        }

        public async Task ConnectAsync()
        {
            if (_connection != null && IsConnected)
            {
                return;
            }

            try
            {
                _connection = new HubConnectionBuilder()
                    .WithUrl($"{_baseUrl}hubs/notifications")
                    .WithAutomaticReconnect()
                    .Build();

                // Обработка переподключения
                _connection.Reconnecting += error =>
                {
                    _logger?.LogWarning("SignalR переподключение... {Error}", error?.Message);
                    return Task.CompletedTask;
                };

                _connection.Reconnected += connectionId =>
                {
                    _logger?.LogInformation("SignalR переподключен. ConnectionId: {ConnectionId}", connectionId);
                    // Автоматически подписываемся на уведомления при переподключении
                    _ = SubscribeToAlarmNotificationsAsync();
                    return Task.CompletedTask;
                };

                _connection.Closed += error =>
                {
                    _logger?.LogError("SignalR соединение закрыто. {Error}", error?.Message);
                    return Task.CompletedTask;
                };

                // Настраиваем обработчик уведомлений о тревогах
                _connection.On<object>("AlarmNotification", (notification) =>
                {
                    try
                    {
                        // Парсим уведомление
                        var json = System.Text.Json.JsonSerializer.Serialize(notification);
                        var doc = System.Text.Json.JsonDocument.Parse(json);
                        var root = doc.RootElement;

                        var message = root.TryGetProperty("message", out var msgProp) 
                            ? msgProp.GetString() ?? "СОБЫТИЕ ТРЕВОГИ!" 
                            : "СОБЫТИЕ ТРЕВОГИ!";
                        var location = root.TryGetProperty("location", out var locProp) 
                            ? locProp.GetString() ?? "Неизвестно" 
                            : "Неизвестно";
                        var user = root.TryGetProperty("user", out var userProp) 
                            ? userProp.GetString() ?? "Неизвестный пользователь" 
                            : "Неизвестный пользователь";

                        OnAlarmNotificationReceived?.Invoke(message, location, user);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Ошибка обработки уведомления о тревоге");
                    }
                });

                await _connection.StartAsync();
                _logger?.LogInformation("SignalR подключен успешно");

                // Подписываемся на уведомления о тревогах
                await SubscribeToAlarmNotificationsAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка подключения к SignalR Hub");
                throw;
            }
        }

        public async Task DisconnectAsync()
        {
            if (_connection != null)
            {
                try
                {
                    await _connection.StopAsync();
                    await _connection.DisposeAsync();
                    _connection = null;
                    _logger?.LogInformation("SignalR отключен");
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Ошибка при отключении от SignalR");
                }
            }
        }

        public async Task SubscribeToAlarmNotificationsAsync()
        {
            if (_connection != null && IsConnected)
            {
                try
                {
                    await _connection.InvokeAsync("SubscribeToAlarmNotifications");
                    _logger?.LogInformation("Подписка на уведомления о тревогах активирована");
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Ошибка при подписке на уведомления о тревогах");
                }
            }
        }

        public void Dispose()
        {
            DisconnectAsync().Wait(5000);
        }
    }
}
