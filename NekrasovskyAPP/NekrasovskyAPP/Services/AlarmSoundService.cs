namespace NekrasovskyAPP.Services
{
    public class AlarmSoundService : IAlarmSoundService
    {
#if ANDROID
        private Platforms.Android.AlarmSoundService? _androidService;
#elif WINDOWS
        private Platforms.Windows.AlarmSoundService? _windowsService;
#endif

        public AlarmSoundService()
        {
#if ANDROID
            _androidService = new Platforms.Android.AlarmSoundService();
#elif WINDOWS
            _windowsService = new Platforms.Windows.AlarmSoundService();
#endif
        }

        public void PlayAlarmSound()
        {
#if ANDROID
            _androidService?.PlayAlarmSound();
#elif WINDOWS
            _windowsService?.PlayAlarmSound();
#else
            // Для других платформ можно добавить реализацию
            System.Diagnostics.Debug.WriteLine("Воспроизведение звука тревоги");
#endif
        }

        public void StopAlarmSound()
        {
#if ANDROID
            _androidService?.StopAlarmSound();
#elif WINDOWS
            _windowsService?.StopAlarmSound();
#endif
        }
    }
}
