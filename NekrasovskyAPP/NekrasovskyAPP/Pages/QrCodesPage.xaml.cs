using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Storage;
using NekrasovskyAPP.Services;

namespace NekrasovskyAPP.Pages
{
    public partial class QrCodesPage : ContentPage
    {
        private readonly QrCodeService _qrCodeService;
        private bool _isBusy;

        public QrCodesPage(QrCodeService qrCodeService)
        {
            InitializeComponent();
            _qrCodeService = qrCodeService;
            
#if !ANDROID && !IOS
            // Скрываем кнопку сканирования на платформах, где сканирование недоступно
            ScanQrButton.IsVisible = false;
#endif
        }

        private async void OnGenerateTextClicked(object sender, EventArgs e)
        {
            await RunWithBusyAsync(async () =>
            {
                var text = TextInput.Text?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(text))
                {
                    await DisplayAlert("Ошибка", "Введите текст для QR-кода.", "OK");
                    return;
                }

                byte[] fileBytes;
                string fileName;
                string extension;

                if (PdfFormatRadio.IsChecked == true)
                {
                    fileBytes = _qrCodeService.GeneratePdf(text);
                    extension = "pdf";
                }
                else
                {
                    fileBytes = _qrCodeService.GeneratePng(text);
                    extension = "png";
                }

                using var stream = new MemoryStream(fileBytes);
                fileName = $"qr_{DateTime.Now:yyyyMMdd_HHmmss}.{extension}";
                var result = await FileSaver.Default.SaveAsync(fileName, stream, CancellationToken.None);

                if (result.IsSuccessful)
                {
                    SetStatus($"Файл сохранен: {result.FilePath}");
                }
                else
                {
                    SetStatus($"Сохранение отменено или не удалось: {result.Exception?.Message}", isError: true);
                }
            });
        }

        private async void OnGenerateBatchClicked(object sender, EventArgs e)
        {
            await RunWithBusyAsync(async () =>
            {
                if (!int.TryParse(BatchCountInput.Text?.Trim(), out var count) || count <= 0)
                {
                    await DisplayAlert("Ошибка", "Введите корректное количество QR-кодов.", "OK");
                    return;
                }

                if (SinglePdfFormatRadio.IsChecked == true)
                {
                    // Генерация одного PDF с множеством QR-кодов
                    var tokens = new List<string>();
                    for (var i = 1; i <= count; i++)
                    {
                        tokens.Add(Guid.NewGuid().ToString("N"));
                    }

                    var pdfBytes = _qrCodeService.GeneratePdfWithMultipleQrCodes(tokens);
                    using var stream = new MemoryStream(pdfBytes);
                    var fileName = $"qr_batch_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                    var result = await FileSaver.Default.SaveAsync(fileName, stream, CancellationToken.None);

                    if (result.IsSuccessful)
                    {
                        SetStatus($"PDF файл сохранен: {result.FilePath}");
                    }
                    else
                    {
                        SetStatus($"Сохранение отменено или не удалось: {result.Exception?.Message}", isError: true);
                    }
                }
                else if (ZipPdfFormatRadio.IsChecked == true)
                {
                    // Генерация ZIP архива с множеством PDF файлов
                    using var zipStream = new MemoryStream();
                    using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                    {
                        for (var i = 1; i <= count; i++)
                        {
                            var token = Guid.NewGuid().ToString("N");
                            var entryName = $"qr_{i:D3}_{token}.pdf";

                            var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                            await using var entryStream = entry.Open();

                            var pdfBytes = _qrCodeService.GeneratePdf(token);
                            await entryStream.WriteAsync(pdfBytes, 0, pdfBytes.Length);
                        }
                    }

                    zipStream.Position = 0;
                    var fileName = $"qr_batch_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
                    var result = await FileSaver.Default.SaveAsync(fileName, zipStream, CancellationToken.None);

                    if (result.IsSuccessful)
                    {
                        SetStatus($"ZIP архив сохранен: {result.FilePath}");
                    }
                    else
                    {
                        SetStatus($"Сохранение отменено или не удалось: {result.Exception?.Message}", isError: true);
                    }
                }
                else
                {
                    // Генерация ZIP архива с множеством PNG файлов
                    using var zipStream = new MemoryStream();
                    using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                    {
                        for (var i = 1; i <= count; i++)
                        {
                            var token = Guid.NewGuid().ToString("N");
                            var entryName = $"qr_{i:D3}_{token}.png";

                            var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                            await using var entryStream = entry.Open();

                            var pngBytes = _qrCodeService.GeneratePng(token);
                            await entryStream.WriteAsync(pngBytes, 0, pngBytes.Length);
                        }
                    }

                    zipStream.Position = 0;
                    var fileName = $"qr_batch_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
                    var result = await FileSaver.Default.SaveAsync(fileName, zipStream, CancellationToken.None);

                    if (result.IsSuccessful)
                    {
                        SetStatus($"ZIP архив сохранен: {result.FilePath}");
                    }
                    else
                    {
                        SetStatus($"Сохранение отменено или не удалось: {result.Exception?.Message}", isError: true);
                    }
                }
            });
        }

        private async Task RunWithBusyAsync(Func<Task> action)
        {
            if (_isBusy)
            {
                return;
            }

            _isBusy = true;
            BusyIndicator.IsRunning = true;
            BusyIndicator.IsVisible = true;
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                SetStatus($"Ошибка: {ex.Message}", isError: true);
            }
            finally
            {
                _isBusy = false;
                BusyIndicator.IsRunning = false;
                BusyIndicator.IsVisible = false;
            }
        }

        private void SetStatus(string message, bool isError = false)
        {
            StatusLabel.Text = message;
            if (Application.Current?.Resources == null)
            {
                return;
            }

            var colorKey = isError ? "Error" : "Gray500";
            if (Application.Current.Resources.TryGetValue(colorKey, out var colorValue) &&
                colorValue is Color color)
            {
                StatusLabel.TextColor = color;
            }
        }

#if ANDROID || IOS
        private async void OnScanQrCodeClicked(object sender, EventArgs e)
        {
            try
            {
                var scannerPage = new BarcodeScannerPage();
                
                void OnBarcodeScanned(object? s, string barcodeValue)
                {
                    scannerPage.BarcodeScanned -= OnBarcodeScanned;
                    
                    if (!string.IsNullOrWhiteSpace(barcodeValue))
                    {
                        TextInput.Text = barcodeValue;
                        SetStatus("QR-код отсканирован и вставлен в поле");
                    }
                }

                scannerPage.BarcodeScanned += OnBarcodeScanned;
                await Navigation.PushModalAsync(scannerPage);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось открыть сканер: {ex.Message}", "OK");
            }
        }
#else
        private async void OnScanQrCodeClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Недоступно", "Сканирование QR-кодов доступно только на Android и iOS устройствах.", "OK");
        }
#endif
    }
}
