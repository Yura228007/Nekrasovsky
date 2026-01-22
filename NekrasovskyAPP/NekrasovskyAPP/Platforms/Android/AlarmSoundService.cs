using Android.Content.Res;
using Android.Media;
using Android.App;

namespace NekrasovskyAPP.Platforms.AndroidPlatform
{
    public class AlarmSoundService
    {
        private MediaPlayer? _mediaPlayer;
        private ToneGenerator? _toneGenerator;
        private CancellationTokenSource? _fallbackToneCts;

        public void PlayAlarmSound()
        {
            try
            {
                StopAlarmSound();

                var context = Android.App.Application.Context;
                if (context == null)
                {
                    return;
                }

                // Требуется файл Resources/Raw/alarm.mp3 (MAUI asset -> Android assets)
                try
                {
                    using var assetFd = context.Assets.OpenFd("alarm.mp3");
                    _mediaPlayer = new MediaPlayer();
                    _mediaPlayer.SetDataSource(assetFd.FileDescriptor, assetFd.StartOffset, assetFd.Length);
                    _mediaPlayer.Looping = true;
                    _mediaPlayer.Prepare();
                    _mediaPlayer.Start();
                }
                catch (Exception)
                {
                    // Fallback: looped tone if mp3 is missing/unavailable
                    _toneGenerator = new ToneGenerator(Android.Media.Stream.Notification, 100);
                    _fallbackToneCts = new CancellationTokenSource();
                    var token = _fallbackToneCts.Token;
                    Task.Run(async () =>
                    {
                        try
                        {
                            while (!token.IsCancellationRequested)
                            {
                                _toneGenerator.StartTone(Tone.CdmaEmergencyRingback, 2000);
                                await Task.Delay(2000, token);
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
                if (_fallbackToneCts != null)
                {
                    _fallbackToneCts.Cancel();
                    _fallbackToneCts.Dispose();
                    _fallbackToneCts = null;
                }

                _toneGenerator?.StopTone();
                _toneGenerator?.Release();
                _toneGenerator = null;

                if (_mediaPlayer != null)
                {
                    _mediaPlayer.Stop();
                    _mediaPlayer.Release();
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
