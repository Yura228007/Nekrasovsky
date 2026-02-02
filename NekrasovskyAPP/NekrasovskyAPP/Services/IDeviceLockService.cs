namespace NekrasovskyAPP.Services
{
    /// <summary>
    /// Сервис блокировки устройства (режим киоска): блокировка выхода из приложения и разблокировка по команде с сервера.
    /// На Android используется Screen Pinning (Lock Task Mode).
    /// </summary>
    public interface IDeviceLockService
    {
        /// <summary>
        /// Поддерживается ли блокировка на текущей платформе (например, только Android).
        /// </summary>
        bool IsLockSupported { get; }

        /// <summary>
        /// Включить блокировку: пользователь не может выйти из приложения (на Android — закрепить экран).
        /// </summary>
        Task LockAsync();

        /// <summary>
        /// Снять блокировку и при необходимости закрыть приложение (вызывается по команде с админ-панели).
        /// </summary>
        Task UnlockAndCloseAsync();
    }
}
