namespace NekrasovskyAPP.Services
{
    /// <summary>
    /// Заглушка для платформ без поддержки блокировки (Windows и др.). Реальная блокировка только на Android.
    /// </summary>
    public class DeviceLockService : IDeviceLockService
    {
        public bool IsLockSupported => false;

        public Task LockAsync() => Task.CompletedTask;

        public Task UnlockAndCloseAsync() => Task.CompletedTask;
    }
}
