using Android.Media;

namespace NekrasovskyAPP.Platforms.AndroidPlatform
{
    public class AlarmSoundService
    {
        private ToneGenerator? _toneGenerator;

        public void PlayAlarmSound()
        {
            try
            {
                // Используем ToneGenerator для воспроизведения тонального сигнала тревоги
                _toneGenerator = new ToneGenerator(Android.Media.Stream.Notification, 100);
                
                // Воспроизводим тональный сигнал (тон DTMF для экстренных ситуаций)
                // Используем TONE_CDMA_EMERGENCY_RINGBACK как сигнал тревоги
                _toneGenerator.StartTone(Android.Media.Tone.CdmaEmergencyRingback, 2000);
                
                System.Diagnostics.Debug.WriteLine("Звук тревоги воспроизведен");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка воспроизведения звука: {ex.Message}");
            }
        }

        public void StopAlarmSound()
        {
            try
            {
                _toneGenerator?.StopTone();
                _toneGenerator?.Release();
                _toneGenerator = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка остановки звука: {ex.Message}");
            }
        }
    }
}
