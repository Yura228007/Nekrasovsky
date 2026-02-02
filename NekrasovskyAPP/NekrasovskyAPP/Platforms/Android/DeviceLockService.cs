#if ANDROID
using NekrasovskyAPP.Services;

namespace NekrasovskyAPP.Platforms.Android
{
    /// <summary>
    /// Реализация блокировки устройства на Android (Screen Pinning / Lock Task Mode).
    /// Разблокировка и закрытие приложения по команде с админ-панели.
    /// </summary>
    public class DeviceLockService : IDeviceLockService
    {
        public bool IsLockSupported => true;

        public Task LockAsync()
        {
            MainActivity.Instance?.EnterLockTask();
            return Task.CompletedTask;
        }

        public Task UnlockAndCloseAsync()
        {
            MainActivity.Instance?.ExitLockTask();
            MainActivity.Instance?.Finish();
            return Task.CompletedTask;
        }
    }
}
#endif
