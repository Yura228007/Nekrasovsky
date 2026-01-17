#if ANDROID || IOS
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using BarcodeScanning;

namespace NekrasovskyAPP.Pages
{
    public partial class BarcodeScannerPage : ContentPage
    {
        private bool _isCameraEnabled = true;
        public string ScannedBarcode { get; private set; } = string.Empty;
        public event EventHandler<string>? BarcodeScanned;

        public bool IsCameraEnabled
        {
            get => _isCameraEnabled;
            set
            {
                _isCameraEnabled = value;
                OnPropertyChanged();
            }
        }

        public ICommand DetectionFinishedCommand { get; }

        public BarcodeScannerPage()
        {
            InitializeComponent();
            DetectionFinishedCommand = new Command<IReadOnlySet<BarcodeResult>?>(OnDetectionFinished);
            BindingContext = this;
        }

        private void OnDetectionFinished(IReadOnlySet<BarcodeResult>? results)
        {
            if (results != null && results.Count > 0)
            {
                // Взять первый результат
                var barcode = results.First();
                
                // Получить значение штрих-кода - используем Value, Text, DisplayValue или RawValue
                // В зависимости от версии библиотеки свойства могут отличаться
                string barcodeValue = string.Empty;
                
                // Пробуем разные возможные свойства
                if (barcode != null)
                {
                    // Пробуем Value (самое вероятное свойство)
                    var valueProp = barcode.GetType().GetProperty("Value");
                    if (valueProp != null)
                    {
                        barcodeValue = valueProp.GetValue(barcode)?.ToString() ?? string.Empty;
                    }
                    
                    // Если не получилось, пробуем Text
                    if (string.IsNullOrEmpty(barcodeValue))
                    {
                        var textProp = barcode.GetType().GetProperty("Text");
                        if (textProp != null)
                        {
                            barcodeValue = textProp.GetValue(barcode)?.ToString() ?? string.Empty;
                        }
                    }
                    
                    // Если не получилось, пробуем DisplayValue
                    if (string.IsNullOrEmpty(barcodeValue))
                    {
                        var displayValueProp = barcode.GetType().GetProperty("DisplayValue");
                        if (displayValueProp != null)
                        {
                            barcodeValue = displayValueProp.GetValue(barcode)?.ToString() ?? string.Empty;
                        }
                    }
                    
                    // Если все еще пусто, пробуем RawValue
                    if (string.IsNullOrEmpty(barcodeValue))
                    {
                        var rawValueProp = barcode.GetType().GetProperty("RawValue");
                        if (rawValueProp != null)
                        {
                            barcodeValue = rawValueProp.GetValue(barcode)?.ToString() ?? string.Empty;
                        }
                    }
                    
                    // Если все еще пусто, пробуем ToString
                    if (string.IsNullOrEmpty(barcodeValue))
                    {
                        barcodeValue = barcode.ToString() ?? string.Empty;
                    }
                }
                
                if (!string.IsNullOrEmpty(barcodeValue))
                {
                    // Обработать результат сканирования
                    ScannedBarcode = barcodeValue;

                    // Вызвать событие для уведомления подписчиков
                    BarcodeScanned?.Invoke(this, barcodeValue);

                    // Отключить камеру после успешного распознавания
                    IsCameraEnabled = false;

                    // Показать результат и закрыть страницу
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        // Получить формат штрих-кода (если доступен)
                        string formatString = "Неизвестный";
                        try
                        {
                            if (barcode != null)
                            {
                                var barcodeType = barcode.GetType();
                                
                                // Пробуем разные возможные свойства для формата (Symbology - самое вероятное)
                                var formatProp = barcodeType.GetProperty("Symbology")
                                    ?? barcodeType.GetProperty("BarcodeFormat") 
                                    ?? barcodeType.GetProperty("Format") 
                                    ?? barcodeType.GetProperty("Type");
                                
                                if (formatProp != null)
                                {
                                    var format = formatProp.GetValue(barcode);
                                    if (format != null)
                                    {
                                        formatString = format.ToString() ?? "Неизвестный";
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // Если свойство недоступно, используем "Неизвестный"
                        }
                        
                        await DisplayAlert(
                            "Штрих-код найден",
                            $"Найден штрих-код: {barcodeValue}\nТип: {formatString}",
                            "OK");

                        // Закрыть страницу и вернуть результат
                        await Navigation.PopModalAsync();
                    });
                }
            }
        }

        private void ToggleFlashlight(object? sender, EventArgs e)
        {
            FlashlightToggle.IsToggled = !FlashlightToggle.IsToggled;
        }

        private void OnFlashlightToggled(object? sender, ToggledEventArgs e)
        {
            // Примечание: BarcodeScanning.Native.Maui может не поддерживать Torch напрямую через CameraView
            // Это зависит от версии библиотеки и платформы
            // Если необходимо, можно добавить платформо-специфичный код для управления фонариком
        }

        private async void CloseScanner(object? sender, EventArgs e)
        {
            IsCameraEnabled = false;
            await Navigation.PopModalAsync();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            // Включить камеру при появлении страницы
            IsCameraEnabled = true;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            
            // Отключить камеру при скрытии страницы
            IsCameraEnabled = false;
        }
    }
}
#endif

