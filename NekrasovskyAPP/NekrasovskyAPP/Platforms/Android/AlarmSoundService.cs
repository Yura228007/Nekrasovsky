using Android.Media;

namespace NekrasovskyAPP.Services
{
    public partial class AlarmSoundService
    {
        private MediaPlayer? _mediaPlayer;
        private ToneGenerator? _toneGenerator;

        partial void StartPlatformAlarm(CancellationToken token)
        {
            try
            {
                var context = Android.App.Application.Context;
                if (context == null) return;

                // Пробуем загрузить MP3
                try
                {
                    var assets = context.Assets;
                    if (assets != null)
                    {
                        using var assetFd = assets.OpenFd("alarm.mp3");
                        _mediaPlayer = new MediaPlayer();
                        _mediaPlayer.SetDataSource(assetFd.FileDescriptor, assetFd.StartOffset, assetFd.Length);
                        _mediaPlayer.Looping = true;
                        _mediaPlayer.Prepare();
                        _mediaPlayer.Start();
                        return;
                    }
                }
                catch
                {
                    // MP3 не найден - используем ToneGenerator
                }

                // Fallback: ToneGenerator
                _toneGenerator = new ToneGenerator(Android.Media.Stream.Alarm, 100);

                Task.Run(async () =>
                {
                    try
                    {
                        while (!token.IsCancellationRequested)
                        {
                            _toneGenerator?.StartTone(Tone.CdmaEmergencyRingback, 1500);
                            await Task.Delay(2000, token);
                        }
                    }
                    catch (OperationCanceledException) { }
                }, token);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Android alarm error: {ex.Message}");
            }
        }

        partial void StopPlatformAlarm()
        {
            try
            {
                _toneGenerator?.StopTone();
                _toneGenerator?.Release();
                _toneGenerator = null;

                _mediaPlayer?.Stop();
                _mediaPlayer?.Release();
                _mediaPlayer?.Dispose();
                _mediaPlayer = null;
            }
            catch { }
        }
    }
}
