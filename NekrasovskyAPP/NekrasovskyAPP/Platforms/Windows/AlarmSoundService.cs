using Windows.Media.Core;
using Windows.Media.Playback;

namespace NekrasovskyAPP.Platforms.Windows
{
    public class AlarmSoundService
    {
        private MediaPlayer? _mediaPlayer;
        private CancellationTokenSource? _fallbackBeepCts;

        public void PlayAlarmSound()
        {
            try
            {
                StopAlarmSound();

                // Требуется файл Resources/Raw/alarm.mp3
                try
                {
                    _mediaPlayer = new MediaPlayer
                    {
                        AutoPlay = false,
                        IsLoopingEnabled = true
                    };

                    _mediaPlayer.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Resources/Raw/alarm.mp3"));
                    _mediaPlayer.Play();
                }
                catch (Exception)
                {
                    // Fallback: looped beep if mp3 is missing/unavailable
                    _fallbackBeepCts = new CancellationTokenSource();
                    var token = _fallbackBeepCts.Token;
                    Task.Run(async () =>
                    {
                        try
                        {
                            while (!token.IsCancellationRequested)
                            {
                                Console.Beep(1000, 300);
                                await Task.Delay(200, token);
                            }
                        }
                        catch
                        {
                            // ignore
                        }
                    }, token);
                }

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
                if (_fallbackBeepCts != null)
                {
                    _fallbackBeepCts.Cancel();
                    _fallbackBeepCts.Dispose();
                    _fallbackBeepCts = null;
                }

                if (_mediaPlayer != null)
                {
                    _mediaPlayer.Pause();
                    _mediaPlayer.Dispose();
                    _mediaPlayer = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка остановки звука: {ex.Message}");
            }
        }
    }
}
