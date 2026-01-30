using Android.Content;
using Android.Media;

namespace NekrasovskyAPP.Services
{
    public partial class AlarmSoundService
    {
        private MediaPlayer? _mediaPlayer;
        private ToneGenerator? _toneGenerator;
        private int? _previousAlarmVolume;
        private int? _previousMusicVolume;

        partial void StartPlatformAlarm(CancellationToken token)
        {
            try
            {
                var context = Android.App.Application.Context;
                if (context == null) return;

                var audioManager = context.GetSystemService(Context.AudioService) as AudioManager;
                if (audioManager != null)
                {
                    _previousAlarmVolume = audioManager.GetStreamVolume(Android.Media.Stream.Alarm);
                    _previousMusicVolume = audioManager.GetStreamVolume(Android.Media.Stream.Music);

                    var maxAlarmVolume = audioManager.GetStreamMaxVolume(Android.Media.Stream.Alarm);
                    var maxMusicVolume = audioManager.GetStreamMaxVolume(Android.Media.Stream.Music);

                    audioManager.SetStreamVolume(Android.Media.Stream.Alarm, maxAlarmVolume, VolumeNotificationFlags.RemoveSoundAndVibrate);
                    audioManager.SetStreamVolume(Android.Media.Stream.Music, maxMusicVolume, VolumeNotificationFlags.RemoveSoundAndVibrate);
                }

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
                var context = Android.App.Application.Context;
                var audioManager = context?.GetSystemService(Context.AudioService) as AudioManager;
                if (audioManager != null)
                {
                    if (_previousAlarmVolume.HasValue)
                    {
                        audioManager.SetStreamVolume(Android.Media.Stream.Alarm, _previousAlarmVolume.Value, VolumeNotificationFlags.RemoveSoundAndVibrate);
                        _previousAlarmVolume = null;
                    }

                    if (_previousMusicVolume.HasValue)
                    {
                        audioManager.SetStreamVolume(Android.Media.Stream.Music, _previousMusicVolume.Value, VolumeNotificationFlags.RemoveSoundAndVibrate);
                        _previousMusicVolume = null;
                    }
                }

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
