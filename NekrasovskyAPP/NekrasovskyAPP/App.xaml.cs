using Microsoft.Maui.ApplicationModel;
using NekrasovskyAPP.Services;

namespace NekrasovskyAPP
{
    public partial class App : Application
    {
        private readonly IAlarmNotificationService _alarmNotificationService;

        public App(IAlarmNotificationService alarmNotificationService)
        {
            InitializeComponent();
            _alarmNotificationService = alarmNotificationService;

            // Устанавливаем темную тему по умолчанию
            UserAppTheme = AppTheme.Dark;

            MainPage = new AppShell();

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await _alarmNotificationService.InitializeAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Alarm init error: {ex.Message}");
                }
            });
        }
    }
}
