using System;
using System.Runtime.InteropServices;
using SkiaSharp;
using ZXing;
using ZXing.Common;

namespace NekrasovskyAPP.Services
{
    public class QrCodeService
    {
        private const int DefaultSize = 512;

        public byte[] GeneratePng(string text, int size = DefaultSize)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("QR-код не может быть пустым.", nameof(text));
            }

            var writer = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Height = size,
                    Width = size,
                    Margin = 1,
                    PureBarcode = true
                }
            };

            var pixelData = writer.Write(text);

            using var bitmap = new SKBitmap(
                pixelData.Width,
                pixelData.Height,
                SKColorType.Bgra8888,
                SKAlphaType.Premul);

            var pixelsPtr = bitmap.GetPixels();
            Marshal.Copy(pixelData.Pixels, 0, pixelsPtr, pixelData.Pixels.Length);

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }
    }
}
