namespace NekrasovskyAPP.Services
{
    public interface IAlarmSoundService
    {
        void PlayAlarmSound();
        void StopAlarmSound();
        bool IsPlaying { get; }
    }
}
