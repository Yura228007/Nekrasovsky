#if ANDROID || IOS
using System.Windows.Input;
using BarcodeScanning;
using NekrasovskyAPP.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui;


namespace NekrasovskyAPP.Pages
{
    public partial class BarcodeScannerPage : ContentPage
    {
        private bool _isCameraEnabled;
        private bool _isProcessing; // 🔒 защита от повторного сканирования

        public string ScannedBarcode { get; private set; } = string.Empty;
        public event EventHandler<string>? BarcodeScanned;
        private readonly IFlashlightService? _flashlightService;

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

            _flashlightService =
                Microsoft.Maui.MauiApplication.Current.Services
                    .GetService<IFlashlightService>();

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
            catch
            {
                // устройство может не поддерживать вибрацию
            }

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                // 🔦 ОБЯЗАТЕЛЬНО выключаем фонарик, если был включён
                if (_flashlightService != null)
                    await _flashlightService.TurnOffAsync();

                await DisplayAlert(
                    "Штрих-код найден",
                    value,
                    "OK");

                await Navigation.PopModalAsync();
            });
        }


        private async void ToggleFlashlight(object? sender, EventArgs e)
        {
            if (_flashlightService == null)
            {
                await DisplayAlert("Ошибка", "Сервис фонарика недоступен", "OK");
                return;
            }

            if (!_flashlightService.IsSupported)
            {
                await DisplayAlert("Фонарик", "Фонарик не поддерживается", "OK");
                return;
            }

            try
            {
                await _flashlightService.ToggleAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Ошибка",
                    $"Не удалось включить фонарик\n{ex.Message}",
                    "OK");
            }
        }


        private async void CloseScanner(object? sender, EventArgs e)
        {
            IsCameraEnabled = false;
            await Navigation.PopModalAsync();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // ♻️ сбрасываем состояние
            _isProcessing = false;
            IsCameraEnabled = true;
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();

            IsCameraEnabled = false;

            if (_flashlightService != null)
            {
                try
                {
                    await _flashlightService.TurnOffAsync();
                }
                catch
                {
                    // ignored
                }
            }
        }

    }
}
#endif
