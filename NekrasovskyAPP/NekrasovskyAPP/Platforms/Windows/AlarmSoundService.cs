using System.Media;

namespace NekrasovskyAPP.Services
{
    public partial class AlarmSoundService
    {
        partial void StartPlatformAlarm(CancellationToken token)
        {
            // Сразу воспроизводим звук
            SystemSounds.Exclamation.Play();

            // Запускаем цикл повторения
            Task.Run(async () =>
            {
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        await Task.Delay(800, token);
                        if (!token.IsCancellationRequested)
                        {
                            SystemSounds.Exclamation.Play();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Нормальная отмена
                }
            }, token);
        }

        partial void StopPlatformAlarm()
        {
            // SystemSounds не требует явной остановки
        }
    }
}
