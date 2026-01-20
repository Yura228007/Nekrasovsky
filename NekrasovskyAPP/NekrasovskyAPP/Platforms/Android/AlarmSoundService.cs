using Android.Media;
using Android.Content;

namespace NekrasovskyAPP.Platforms.Android
{
    public class AlarmSoundService
    {
        private MediaPlayer? _mediaPlayer;

        public void PlayAlarmSound()
        {
            try
            {
                // Используем системный звук тревоги
                var audioManager = Android.App.Application.Context.GetSystemService(Context.AudioService) as AudioManager;
                var streamType = Stream.Notification;
                
                // Создаем MediaPlayer для воспроизведения системного звука
                _mediaPlayer = MediaPlayer.Create(Android.App.Application.Context, Android.Resource.Raw.Beep);
                
                if (_mediaPlayer == null)
                {
                    // Если системный звук недоступен, используем тональный сигнал
                    var toneGenerator = new ToneGenerator(streamType, 100);
                    toneGenerator.StartTone(Tone.AlertEmergencyGeneric, 2000);
                    return;
                }

                _mediaPlayer.SetAudioStreamType(streamType);
                _mediaPlayer.SetVolume(1.0f, 1.0f);
                _mediaPlayer.SetLooping(false);
                
                _mediaPlayer.Completion += (sender, e) =>
                {
                    _mediaPlayer?.Release();
                    _mediaPlayer = null;
                };

                _mediaPlayer.Start();
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
                _mediaPlayer?.Stop();
                _mediaPlayer?.Release();
                _mediaPlayer = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка остановки звука: {ex.Message}");
            }
        }
    }
}
