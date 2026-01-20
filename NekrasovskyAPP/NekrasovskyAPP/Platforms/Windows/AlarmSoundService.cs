using System.Media;

namespace NekrasovskyAPP.Platforms.Windows
{
    public class AlarmSoundService
    {
        private SoundPlayer? _soundPlayer;

        public void PlayAlarmSound()
        {
            try
            {
                // Используем системный звук Windows
                SystemSounds.Exclamation.Play();
                
                // Альтернативно можно использовать более громкий звук
                // SystemSounds.Asterisk.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка воспроизведения звука: {ex.Message}");
            }
        }

        public void StopAlarmSound()
        {
            // SystemSounds не требует остановки
        }
    }
}
