namespace NekrasovskyAPP.Services
{
    public partial class AlarmSoundService : IAlarmSoundService
    {
        private CancellationTokenSource? _cts;

        public bool IsPlaying { get; private set; }

        public void PlayAlarmSound()
        {
            if (IsPlaying)
                return;

            IsPlaying = true;
            _cts = new CancellationTokenSource();

            StartPlatformAlarm(_cts.Token);
        }

        public void StopAlarmSound()
        {
            IsPlaying = false;

            try
            {
                _cts?.Cancel();
                _cts?.Dispose();
                _cts = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping alarm sound: {ex.Message}");
            }

            StopPlatformAlarm();
        }

        // Платформенные методы реализуются в partial классах
        partial void StartPlatformAlarm(CancellationToken token);
        partial void StopPlatformAlarm();
    }
}
