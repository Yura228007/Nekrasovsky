namespace NekrasovskyAPP.Platforms.Windows
{
    public class AlarmSoundService
    {
        public void PlayAlarmSound()
        {
            try
            {
                // В .NET MAUI для Windows используем системный звук через консольный Beep
                // Это простой способ воспроизвести звук без дополнительных зависимостей
                
                // Воспроизводим несколько коротких звуковых сигналов для имитации тревоги
                Task.Run(() =>
                {
                    try
                    {
                        // Три коротких сигнала
                        for (int i = 0; i < 3; i++)
                        {
                            Console.Beep(1000, 200); // Частота 1000 Гц, длительность 200 мс
                            Thread.Sleep(100);
                        }
                    }
                    catch
                    {
                        // Если Beep не поддерживается, просто игнорируем
                        System.Diagnostics.Debug.WriteLine("Beep не поддерживается на этой платформе");
                    }
                });
                
                System.Diagnostics.Debug.WriteLine("Звук тревоги воспроизведен");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка воспроизведения звука: {ex.Message}");
            }
        }

        public void StopAlarmSound()
        {
            // Console.Beep не требует явной остановки
        }
    }
}
