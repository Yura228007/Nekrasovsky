using Android.App;
using Android.Content;
using Android.OS;
using Android.Content.PM;
using AndroidX.Core.App;
using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;

namespace NekrasovskyAPP.Platforms.Android
{
    [Service(
    Enabled = true,
    Exported = false,
    ForegroundServiceType = ForegroundService.TypeDataSync
)]
    public class AlarmForegroundService : Service
    {
        private const int ServiceNotificationId = 2001;
        private const int AlarmNotificationId = 2002;
        private const string ServiceChannelId = "alarm_service_channel";
        private const string AlarmChannelId = "alarm_alerts_channel";
        private const string ServiceChannelName = "Alarm Monitoring";
        private const string AlarmChannelName = "Alarm Alerts";

        private HubConnection? _connection;

        public override void OnCreate()
        {
            base.OnCreate();
            CreateNotificationChannels();
        }

        public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
        {
            var notification = BuildServiceNotification();
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                StartForeground(ServiceNotificationId, notification, ForegroundService.TypeDataSync);
            }
            else
            {
                StartForeground(ServiceNotificationId, notification);
            }
            _ = EnsureSignalRConnectedAsync();
            return StartCommandResult.Sticky;
        }

        public override IBinder? OnBind(Intent? intent) => null;

        public override void OnDestroy()
        {
            _ = DisconnectAsync();
            base.OnDestroy();
        }

        private void CreateNotificationChannels()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            var notificationManager = (NotificationManager?)GetSystemService(NotificationService);
            if (notificationManager == null)
            {
                return;
            }

            var serviceChannel = new NotificationChannel(
                ServiceChannelId,
                ServiceChannelName,
                NotificationImportance.Low)
            {
                Description = "Keeps alarm monitoring active"
            };

            var alarmChannel = new NotificationChannel(
                AlarmChannelId,
                AlarmChannelName,
                NotificationImportance.High)
            {
                Description = "Alarm notifications"
            };

            alarmChannel.EnableVibration(true);
            alarmChannel.SetVibrationPattern(new long[] { 0, 500, 300, 500, 300, 500 });

            notificationManager.CreateNotificationChannel(serviceChannel);
            notificationManager.CreateNotificationChannel(alarmChannel);
        }

        private Notification BuildServiceNotification()
        {
            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);

            var pendingIntent = PendingIntent.GetActivity(
                this,
                0,
                intent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            return new NotificationCompat.Builder(this, ServiceChannelId)
                .SetContentTitle("Мониторинг тревог")
                .SetContentText("Подключение к тревогам активно")
                .SetSmallIcon(Resource.Mipmap.appicon)
                .SetOngoing(true)
                .SetContentIntent(pendingIntent)
                .Build();
        }

        private async Task EnsureSignalRConnectedAsync()
        {
            if (_connection != null && _connection.State == HubConnectionState.Connected)
            {
                return;
            }

            try
            {
                _connection = new HubConnectionBuilder()
                    .WithUrl($"{GetBaseUrl()}hubs/notifications")
                    .WithAutomaticReconnect()
                    .Build();

                _connection.On<object>("AlarmNotification", notification =>
                {
                    try
                    {
                        var json = JsonSerializer.Serialize(notification);
                        var doc = JsonDocument.Parse(json);
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

                        ShowAlarmNotification(message, location, user);
                    }
                    catch
                    {
                        ShowAlarmNotification("СОБЫТИЕ ТРЕВОГИ!", "Неизвестно", "Неизвестный пользователь");
                    }
                });

                await _connection.StartAsync();
                await _connection.InvokeAsync("SubscribeToAlarmNotifications");
            }
            catch
            {
                // Service will retry on next start; keep running.
            }
        }

        private void ShowAlarmNotification(string message, string location, string user)
        {
            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);

            var pendingIntent = PendingIntent.GetActivity(
                this,
                1,
                intent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            var notification = new NotificationCompat.Builder(this, AlarmChannelId)
                .SetContentTitle("🚨 ТРЕВОГА!")
                .SetContentText($"{message} · {location} · {user}")
                .SetStyle(new NotificationCompat.BigTextStyle()
                    .BigText($"{message}\nМесто: {location}\nПользователь: {user}"))
                .SetSmallIcon(Resource.Mipmap.appicon)
                .SetAutoCancel(true)
                .SetCategory(NotificationCompat.CategoryAlarm)
                .SetPriority(NotificationCompat.PriorityMax)
                .SetContentIntent(pendingIntent)
                .Build();

            NotificationManagerCompat.From(this).Notify(AlarmNotificationId, notification);
        }

        private async Task DisconnectAsync()
        {
            try
            {
                if (_connection != null)
                {
                    await _connection.StopAsync();
                    await _connection.DisposeAsync();
                    _connection = null;
                }
            }
            catch
            {
                // Ignore shutdown errors.
            }
        }

        private static string GetBaseUrl()
        {
#if ANDROID || IOS
            return "http://192.168.1.128:9000/";
#else
            return "http://localhost:9000/";
#endif
        }
    }
}
