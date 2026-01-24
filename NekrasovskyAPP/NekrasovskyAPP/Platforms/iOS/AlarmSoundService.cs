using AVFoundation;
using AudioToolbox;

namespace NekrasovskyAPP.Services
{
    public partial class AlarmSoundService
    {
        private AVAudioPlayer? _audioPlayer;

        partial void StartPlatformAlarm(CancellationToken token)
        {
            try
            {
                // Воспроизводим системный звук сразу
                SystemSound.FromFile("/System/Library/Audio/UISounds/alarm.caf")?.PlayAlertSound();

                // Пробуем загрузить MP3
                try
                {
                    var path = NSBundle.MainBundle.PathForResource("alarm", "mp3");
                    if (!string.IsNullOrEmpty(path))
                    {
                        var url = NSUrl.FromFilename(path);
                        _audioPlayer = AVAudioPlayer.FromUrl(url);
                        if (_audioPlayer != null)
                        {
                            _audioPlayer.NumberOfLoops = -1; // Бесконечный цикл
                            _audioPlayer.Volume = 1.0f;
                            _audioPlayer.PrepareToPlay();
                            _audioPlayer.Play();
                            return;
                        }
                    }
                }
                catch { }

                // Fallback: системные звуки в цикле
                Task.Run(async () =>
                {
                    try
                    {
                        while (!token.IsCancellationRequested)
                        {
                            SystemSound.FromFile("/System/Library/Audio/UISounds/alarm.caf")?.PlayAlertSound();
                            await Task.Delay(1500, token);
                        }
                    }
                    catch (OperationCanceledException) { }
                }, token);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"iOS alarm error: {ex.Message}");
            }
        }

        partial void StopPlatformAlarm()
        {
            try
            {
                _audioPlayer?.Stop();
                _audioPlayer?.Dispose();
                _audioPlayer = null;
            }
            catch { }
        }
    }
}
