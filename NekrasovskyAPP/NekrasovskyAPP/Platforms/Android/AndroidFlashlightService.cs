#if ANDROID
using Android.Content;
using Android.Hardware.Camera2;
using NekrasovskyAPP.Services;

namespace NekrasovskyAPP.Platforms.AndroidPlatform
{
    public class AndroidFlashlightService : IFlashlightService
    {
        private readonly CameraManager _cameraManager;
        private string? _cameraId;
        private bool _isOn;

        public AndroidFlashlightService()
        {
            _cameraManager = (CameraManager)
                Android.App.Application.Context
                    .GetSystemService(Context.CameraService)!;

            // ✅ ИЩЕМ КАМЕРУ С ФОНАРИКОМ
            foreach (var id in _cameraManager.GetCameraIdList())
            {
                var characteristics = _cameraManager.GetCameraCharacteristics(id);
                var hasFlash =
                    (bool?)characteristics.Get(
                        CameraCharacteristics.FlashInfoAvailable) == true;

                if (hasFlash)
                {
                    _cameraId = id;
                    break;
                }
            }
        }

        public bool IsSupported => _cameraId != null;

        public Task<bool> ToggleAsync()
        {
            if (_cameraId == null)
                return Task.FromResult(false);

            try
            {
                _isOn = !_isOn;
                _cameraManager.SetTorchMode(_cameraId, _isOn);
                return Task.FromResult(_isOn);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task TurnOffAsync()
        {
            if (_cameraId == null)
                return Task.CompletedTask;

            try
            {
                if (_isOn)
                {
                    _cameraManager.SetTorchMode(_cameraId, false);
                    _isOn = false;
                }
            }
            catch
            {
                // игнорируем
            }

            return Task.CompletedTask;
        }
    }
}
#endif
