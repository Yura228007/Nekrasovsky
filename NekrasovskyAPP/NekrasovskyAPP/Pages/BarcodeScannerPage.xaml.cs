#if ANDROID || IOS
using System.Windows.Input;
using BarcodeScanning;

namespace NekrasovskyAPP.Pages
{
    public partial class BarcodeScannerPage : ContentPage
    {
        private bool _isCameraEnabled;
        private bool _isProcessing; // 🔒 защита от повторного сканирования

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

            // ✅ В BarcodeScanning.Native.Maui корректное свойство — Value
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

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert(
                    "Штрих-код найден",
                    value,
                    "OK");

                await Navigation.PopModalAsync();
            });
        }

        private async void ToggleFlashlight(object? sender, EventArgs e)
        {
            // Заглушка — чтобы код был стабильным
            await DisplayAlert(
                "Фонарик",
                "Управление фонариком пока не реализовано",
                "OK");
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

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            IsCameraEnabled = false;
        }
    }
}
#endif
