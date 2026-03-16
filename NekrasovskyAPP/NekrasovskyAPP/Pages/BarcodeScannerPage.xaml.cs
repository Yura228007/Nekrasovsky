#if ANDROID || IOS
using System.Windows.Input;
using BarcodeScanning;
using Microsoft.Maui.ApplicationModel;


namespace NekrasovskyAPP.Pages
{
    public partial class BarcodeScannerPage : ContentPage
    {
        private bool _isCameraEnabled;
        private bool _isProcessing; // 🔒 защита от повторного сканирования
        private bool _isTorchOn = false;

        public string ScannedBarcode { get; private set; } = string.Empty;
        public event EventHandler<string>? BarcodeScanned;

        public bool IsCameraEnabled
        {
            get => _isCameraEnabled;
            set
            {
                if (_isCameraEnabled == value)
                    return;

                _isCameraEnabled = value;
                OnPropertyChanged();
            }
        }

        public ICommand DetectionFinishedCommand { get; }

        public BarcodeScannerPage()
        {
            InitializeComponent();

            DetectionFinishedCommand =
                new Command<IReadOnlySet<BarcodeResult>?>(OnDetectionFinished);

            BindingContext = this;
        }


        private void OnDetectionFinished(IReadOnlySet<BarcodeResult>? results)
        {
            // ❌ ничего не найдено
            if (results == null || results.Count == 0)
                return;

            // 🔒 защита от повторных вызовов
            if (_isProcessing)
                return;

            _isProcessing = true;

            var barcode = results.FirstOrDefault();
            if (barcode == null)
            {
                _isProcessing = false;
                return;
            }

            // ✅ корректно для BarcodeScanning.Native.Maui
            var value = barcode.DisplayValue?.ToString();

            if (string.IsNullOrWhiteSpace(value))
            {
                _isProcessing = false;
                return;
            }

            ScannedBarcode = value;
            BarcodeScanned?.Invoke(this, value);

            // ⛔ сразу выключаем камеру
            IsCameraEnabled = false;

            // 📳 ВИБРАЦИЯ ПРИ УСПЕШНОМ СКАНЕ
            try
            {
                Vibration.Default.Vibrate(
                    TimeSpan.FromMilliseconds(150));
            }
            catch (Exception ex)
            {
                // Устройство может не поддерживать вибрацию
                Console.WriteLine($"Vibration not supported: {ex.Message}");
            }

            MainThread.BeginInvokeOnMainThread(async () =>
            {

                await DisplayAlert(
                    "Штрих-код найден",
                    value,
                    "OK");

                await Navigation.PopModalAsync();
            });
        }

        private void ToggleFlashlight(object? sender, EventArgs e)
        {
            try
            {
                _isTorchOn = !_isTorchOn;
                BarcodeCamera.TorchOn = _isTorchOn;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error toggling flashlight: {ex.Message}");
                DisplayAlert(
                    "Фонарик",
                    "Не удалось включить фонарик",
                    "OK");
            }
        }

        private async void CloseScanner(object? sender, EventArgs e)
        {
            IsCameraEnabled = false;
            await Navigation.PopModalAsync();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _isProcessing = false;

            bool hasPermission = await RequestCameraPermissionAsync();
            if (hasPermission)
            {
                IsCameraEnabled = true;
            }
            else
            {
                await DisplayAlert(
                    "Камера недоступна",
                    "Без разрешения на камеру сканирование невозможно.",
                    "OK");
                await Navigation.PopModalAsync();
            }
        }


        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            IsCameraEnabled = false;

            try
            {
                BarcodeCamera.TorchOn = false;
                _isTorchOn = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error turning off flashlight on close: {ex.Message}");
            }
        }

        private async Task<bool> RequestCameraPermissionAsync()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();

            if (status == PermissionStatus.Granted)
                return true;

            if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
            {
                // На iOS если пользователь ранее отказал, надо открывать настройки
                await DisplayAlert(
                    "Разрешение на камеру",
                    "Разрешение на камеру отключено. Пожалуйста, включите его в настройках.",
                    "OK");
                return false;
            }

            status = await Permissions.RequestAsync<Permissions.Camera>();

            return status == PermissionStatus.Granted;
        }

    }
}
#endif
