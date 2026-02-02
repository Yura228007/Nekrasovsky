using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using SkiaSharp;
using ZXing;
using ZXing.Common;

namespace NekrasovskyAPP.Services
{
    public class QrCodeService
    {
        private const int DefaultSize = 512;
        private const float PdfPageWidth = 595; // A4 width in points (210mm)
        private const float PdfPageHeight = 842; // A4 height in points (297mm)
        private const float QrCodeSizeInPdf = 200; // Size of QR code in PDF points (for single QR)
        private const float Margin = 50; // Margin from page edges
        private const int QrCodesPerRow = 2; // Number of QR codes per row
        private const int QrCodesPerColumn = 3; // Number of QR codes per column
        private const float Spacing = 20; // Spacing between QR codes
        private const float TextHeight = 20; // Height reserved for text below QR code

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

        public byte[] GeneratePdf(string text, int qrSize = DefaultSize)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("QR-код не может быть пустым.", nameof(text));
            }

            using var stream = new MemoryStream();
            using var document = SKDocument.CreatePdf(stream);
            using var canvas = document.BeginPage(PdfPageWidth, PdfPageHeight);

            // Clear background
            canvas.Clear(SKColors.White);

            // Generate QR code image
            var qrImage = GenerateQrCodeImage(text, qrSize);

            // Calculate position to center QR code on page
            var x = (PdfPageWidth - QrCodeSizeInPdf) / 2;
            var y = (PdfPageHeight - QrCodeSizeInPdf) / 2;

            // Draw QR code with proper scaling
            var destRect = new SKRect(x, y, x + QrCodeSizeInPdf, y + QrCodeSizeInPdf);
            canvas.DrawImage(qrImage, destRect);

            // Draw text below QR code
            using var font = new SKFont(SKTypeface.Default, 12);
            using var paint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true
            };
            var textY = y + QrCodeSizeInPdf + 20;
            canvas.DrawText(text, PdfPageWidth / 2, textY, SKTextAlign.Center, font, paint);

            document.EndPage();
            document.Close();

            return stream.ToArray();
        }

        public byte[] GeneratePdfWithMultipleQrCodes(IEnumerable<string> texts, int qrSize = DefaultSize)
        {
            if (texts == null)
            {
                throw new ArgumentNullException(nameof(texts));
            }

            using var stream = new MemoryStream();
            using var document = SKDocument.CreatePdf(stream);

            var textList = new List<string>(texts);
            if (textList.Count == 0)
            {
                throw new ArgumentException("Список текстов для QR-кодов не может быть пустым.", nameof(texts));
            }

            var qrCodesPerPage = QrCodesPerRow * QrCodesPerColumn;
            var totalPages = (int)Math.Ceiling((double)textList.Count / qrCodesPerPage);

            for (int pageIndex = 0; pageIndex < totalPages; pageIndex++)
            {
                using var canvas = document.BeginPage(PdfPageWidth, PdfPageHeight);
                canvas.Clear(SKColors.White);

                var startIndex = pageIndex * qrCodesPerPage;
                var endIndex = Math.Min(startIndex + qrCodesPerPage, textList.Count);

                for (int i = startIndex; i < endIndex; i++)
                {
                    var text = textList[i];
                    var positionInPage = i - startIndex;
                    var row = positionInPage / QrCodesPerRow;
                    var col = positionInPage % QrCodesPerRow;

                    // Calculate position and size
                    var availableWidth = PdfPageWidth - 2 * Margin;
                    var availableHeight = PdfPageHeight - 2 * Margin;
                    var cellWidth = (availableWidth - (QrCodesPerRow - 1) * Spacing) / QrCodesPerRow;
                    var cellHeight = (availableHeight - (QrCodesPerColumn - 1) * Spacing) / QrCodesPerColumn;
                    
                    // QR code size should fit in cell with space for text
                    var qrSizeInPdf = Math.Min(cellWidth, cellHeight - TextHeight);
                    
                    // Center QR code in cell
                    var x = Margin + col * (cellWidth + Spacing) + (cellWidth - qrSizeInPdf) / 2;
                    var y = Margin + row * (cellHeight + Spacing) + (cellHeight - qrSizeInPdf - TextHeight) / 2;

                    // Generate QR code image
                    var qrImage = GenerateQrCodeImage(text, qrSize);

                    // Draw QR code with scaling
                    var destRect = new SKRect(x, y, x + qrSizeInPdf, y + qrSizeInPdf);
                    canvas.DrawImage(qrImage, destRect);

                    // Draw text below QR code
                    using var font = new SKFont(SKTypeface.Default, 10);
                    using var paint = new SKPaint
                    {
                        Color = SKColors.Black,
                        IsAntialias = true
                    };
                    var textY = y + qrSizeInPdf + 15;
                    var textX = x + qrSizeInPdf / 2;
                    
                    // Truncate text if too long
                    var displayText = text.Length > 20 ? text.Substring(0, 20) + "..." : text;
                    canvas.DrawText(displayText, textX, textY, SKTextAlign.Center, font, paint);
                }

                document.EndPage();
            }

            document.Close();
            return stream.ToArray();
        }

        private SKImage GenerateQrCodeImage(string text, int size)
        {
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

            return SKImage.FromBitmap(bitmap);
        }
    }
}
