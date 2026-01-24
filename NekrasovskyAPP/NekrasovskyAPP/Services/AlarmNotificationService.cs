using NekrasovskyAPP.Models;

namespace NekrasovskyAPP.Services
{
    public class AlarmNotificationService : IAlarmNotificationService
    {
        private readonly ISignalRService _signalRService;
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;
        private readonly IAlarmSoundService _alarmSoundService;
        private bool _isShowingAlert;

        public event EventHandler<AlarmNotificationEventArgs>? AlarmReceived;

        public AlarmNotificationService(
            ISignalRService signalRService,
            IApiService apiService,
            IAuthService authService,
            IAlarmSoundService alarmSoundService)
        {
            _signalRService = signalRService;
            _apiService = apiService;
            _authService = authService;
            _alarmSoundService = alarmSoundService;
        }

        public async Task InitializeAsync()
        {
            // Подключаемся к SignalR
            if (!_signalRService.IsConnected)
            {
                try
                {
                    await _signalRService.ConnectAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"SignalR connect error: {ex.Message}");
                }
            }

            // Подписываемся на уведомления
            _signalRService.SetOnAlarmNotification(OnAlarmNotificationReceived);
        }

        private void OnAlarmNotificationReceived(string message, string location, string userName)
        {
            // Проверяем, не от текущего ли пользователя
            var currentUser = _authService.CurrentUser;
            if (currentUser != null)
            {
                var currentUserFullName = $"{currentUser.Name} {currentUser.Surname}";
                if (userName == currentUserFullName)
                {
                    // Это наша тревога - не показываем дублирующее уведомление
                    return;
                }
            }

            var args = new AlarmNotificationEventArgs
            {
                Message = message,
                Location = location,
                UserName = userName,
                Timestamp = DateTime.Now
            };

            // Вызываем событие
            AlarmReceived?.Invoke(this, args);

            // Показываем уведомление со звуком
            ShowAlarmNotification(args);
        }

        private void ShowAlarmNotification(AlarmNotificationEventArgs args)
        {
            if (_isShowingAlert)
                return;

            _isShowingAlert = true;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    // Воспроизводим звук
                    _alarmSoundService.PlayAlarmSound();

                    // Показываем диалог
                    var page = Application.Current?.MainPage;
                    if (page != null)
                    {
                        await page.DisplayAlert(
                            "🚨 ТРЕВОГА!",
                            $"{args.Message}\n\nМесто: {args.Location}\nПользователь: {args.UserName}",
                            "OK");
                    }

                    // Останавливаем звук
                    _alarmSoundService.StopAlarmSound();
                }
                finally
                {
                    _isShowingAlert = false;
                }
            });
        }

        public async Task<bool> SendAlarmAsync(string location, string? message)
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null)
                return false;

            try
            {
                var alarmEvent = new AlarmEvent
                {
                    UserId = currentUser.Id,
                    Location = location,
                    Message = message ?? string.Empty,
                    CreatedAt = DateTime.UtcNow
                };

                var response = await _apiService.AddAlarmEventAsync(alarmEvent);

                if (response.AlarmEvent != null)
                {
                    // Воспроизводим звук для отправителя
                    _alarmSoundService.PlayAlarmSound();

                    // Показываем подтверждение
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        var page = Application.Current?.MainPage;
                        if (page != null)
                        {
                            await page.DisplayAlert(
                                "🚨 ТРЕВОГА ОТПРАВЛЕНА!",
                                $"Место: {location}\nСообщение: {message ?? "Не указано"}\n\nВсе пользователи получат уведомление.",
                                "OK");
                        }
                        _alarmSoundService.StopAlarmSound();
                    });

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Send alarm error: {ex.Message}");
                return false;
            }
        }
    }
}
