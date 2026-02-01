using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;
using SkiaSharp;

namespace server.Services
{
    public class ShiftReportService : IShiftReportService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ShiftReportService> _logger;
        private readonly string _reportsDirectory;

        // PDF constants
        private const float PdfPageWidth = 595; // A4 width in points
        private const float PdfPageHeight = 842; // A4 height in points
        private const float Margin = 40;
        private const float LineHeight = 20;
        private const float TableRowHeight = 25;

        public ShiftReportService(AppDbContext context, ILogger<ShiftReportService> logger, IWebHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            _reportsDirectory = Path.Combine(env.ContentRootPath, "reports");

            if (!Directory.Exists(_reportsDirectory))
            {
                Directory.CreateDirectory(_reportsDirectory);
            }
        }

        public async Task<ShiftReport> GenerateReportAsync(int workReportId)
        {
            var workReport = await _context.WorkReports
                .Include(wr => wr.User)
                    .ThenInclude(u => u!.Role)
                .FirstOrDefaultAsync(wr => wr.Id == workReportId);

            if (workReport == null)
            {
                throw new KeyNotFoundException($"WorkReport with ID {workReportId} not found");
            }

            if (workReport.FinishWork == null)
            {
                throw new InvalidOperationException("Cannot generate report for an active shift");
            }

            // Check if report already exists
            var existingReport = await _context.Set<ShiftReport>()
                .FirstOrDefaultAsync(sr => sr.WorkReportId == workReportId);

            if (existingReport != null)
            {
                return existingReport;
            }

            var user = workReport.User!;
            var shiftStart = workReport.StartWork;
            var shiftEnd = workReport.FinishWork.Value;

            // Gather data for the report
            var productOutputs = await GetProductOutputsForShift(workReport.UserId, shiftStart, shiftEnd);
            var partRequests = await GetPartRequestsForShift(workReport.UserId, shiftStart, shiftEnd);
            var responsibilities = await GetResponsibilitiesForShift(workReport.UserId);

            // Generate PDF
            var pdfBytes = GeneratePdf(user, workReport, productOutputs, partRequests, responsibilities);

            // Save to file
            var fileName = $"report_{user.Surname}_{user.Name}_{shiftStart:yyyy-MM-dd_HH-mm}.pdf";
            var filePath = Path.Combine(_reportsDirectory, fileName);
            await File.WriteAllBytesAsync(filePath, pdfBytes);

            // Create summary
            var summary = GenerateSummary(productOutputs, partRequests, responsibilities);

            // Save to database
            var report = new ShiftReport
            {
                WorkReportId = workReportId,
                UserId = workReport.UserId,
                FileName = fileName,
                FilePath = filePath,
                FileSize = pdfBytes.Length,
                CreatedAt = DateTime.UtcNow,
                ShiftStart = shiftStart,
                ShiftEnd = shiftEnd,
                Summary = summary
            };

            _context.Set<ShiftReport>().Add(report);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Shift report generated for user {UserId}, WorkReport {WorkReportId}",
                workReport.UserId, workReportId);

            return report;
        }

        public async Task<ShiftReport?> GetReportByIdAsync(int id)
        {
            return await _context.Set<ShiftReport>()
                .Include(sr => sr.User)
                .Include(sr => sr.WorkReport)
                .FirstOrDefaultAsync(sr => sr.Id == id);
        }

        public async Task<ShiftReport?> GetReportByWorkReportIdAsync(int workReportId)
        {
            return await _context.Set<ShiftReport>()
                .Include(sr => sr.User)
                .Include(sr => sr.WorkReport)
                .FirstOrDefaultAsync(sr => sr.WorkReportId == workReportId);
        }

        public async Task<IEnumerable<ShiftReport>> GetReportsByUserAsync(int userId)
        {
            return await _context.Set<ShiftReport>()
                .Include(sr => sr.User)
                .Include(sr => sr.WorkReport)
                .Where(sr => sr.UserId == userId)
                .OrderByDescending(sr => sr.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ShiftReport>> GetAllReportsAsync()
        {
            return await _context.Set<ShiftReport>()
                .Include(sr => sr.User)
                .Include(sr => sr.WorkReport)
                .OrderByDescending(sr => sr.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ShiftReport>> GetReportsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Set<ShiftReport>()
                .Include(sr => sr.User)
                .Include(sr => sr.WorkReport)
                .Where(sr => sr.ShiftStart >= startDate && sr.ShiftEnd <= endDate)
                .OrderByDescending(sr => sr.CreatedAt)
                .ToListAsync();
        }

        public async Task<byte[]> GetReportFileAsync(int reportId)
        {
            var report = await GetReportByIdAsync(reportId);
            if (report == null)
            {
                throw new KeyNotFoundException($"ShiftReport with ID {reportId} not found");
            }

            if (!File.Exists(report.FilePath))
            {
                throw new FileNotFoundException($"Report file not found: {report.FilePath}");
            }

            return await File.ReadAllBytesAsync(report.FilePath);
        }

        public async Task<bool> DeleteReportAsync(int id)
        {
            var report = await _context.Set<ShiftReport>().FindAsync(id);
            if (report == null)
            {
                return false;
            }

            // Delete file if exists
            if (File.Exists(report.FilePath))
            {
                File.Delete(report.FilePath);
            }

            _context.Set<ShiftReport>().Remove(report);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CanUserDownloadReportAsync(int requestingUserId, int reportId)
        {
            var requestingUser = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == requestingUserId);

            if (requestingUser == null)
            {
                return false;
            }

            var report = await GetReportByIdAsync(reportId);
            if (report == null)
            {
                return false;
            }

            // Owner and Admin can download any report
            var roleCode = requestingUser.Role?.Code ?? string.Empty;
            if (roleCode == "Owner" || roleCode == "Admin")
            {
                return true;
            }

            // Users can download their own reports
            return report.UserId == requestingUserId;
        }

        private async Task<List<ProductOutput>> GetProductOutputsForShift(int userId, DateTime start, DateTime end)
        {
            return await _context.ProductOutputs
                .Include(po => po.Product)
                .Include(po => po.Warehouse)
                .Include(po => po.Machine)
                .Where(po => po.UserId == userId && po.CreatedAt >= start && po.CreatedAt <= end)
                .OrderBy(po => po.CreatedAt)
                .ToListAsync();
        }

        private async Task<List<PartRequest>> GetPartRequestsForShift(int userId, DateTime start, DateTime end)
        {
            return await _context.PartRequests
                .Include(pr => pr.Material)
                .Include(pr => pr.FromWarehouse)
                .Include(pr => pr.ToWarehouse)
                .Where(pr => pr.FromUserId == userId && pr.CreatedAt >= start && pr.CreatedAt <= end)
                .OrderBy(pr => pr.CreatedAt)
                .ToListAsync();
        }

        private async Task<List<Responsibility>> GetResponsibilitiesForShift(int userId)
        {
            return await _context.Responsibilities
                .Include(r => r.Material)
                .Include(r => r.Product)
                .Where(r => r.UserId == userId && r.IsActive)
                .ToListAsync();
        }

        private string GenerateSummary(List<ProductOutput> outputs, List<PartRequest> requests, List<Responsibility> responsibilities)
        {
            var totalProduced = outputs.Sum(o => o.ProducedQuantity);
            var totalDefects = outputs.Sum(o => o.DefectQuantity);
            var totalEco = outputs.Sum(o => o.EcoQuantity);
            var approvedRequests = requests.Count(r => r.Status == PartRequestStatus.Approved);

            return $"Произведено: {totalProduced}, Брак: {totalDefects}, Эко: {totalEco}, " +
                   $"Заявок: {requests.Count} (одобрено: {approvedRequests}), " +
                   $"Ответственностей: {responsibilities.Count}";
        }

        private byte[] GeneratePdf(User user, WorkReport workReport,
            List<ProductOutput> outputs, List<PartRequest> requests, List<Responsibility> responsibilities)
        {
            using var stream = new MemoryStream();
            using var document = SKDocument.CreatePdf(stream);

            var roleName = user.Role?.Name ?? "Сотрудник";
            var shiftType = workReport.StartWork.Hour >= 8 && workReport.StartWork.Hour < 20 ? "День" : "Ночь";
            var duration = workReport.FinishWork!.Value - workReport.StartWork;

            // Page 1: Header and Production Output
            DrawPage1(document, user, workReport, roleName, shiftType, duration, outputs);

            // Page 2: Part Requests (if any)
            if (requests.Count > 0)
            {
                DrawPage2(document, user, requests);
            }

            // Page 3: Responsibilities (if any)
            if (responsibilities.Count > 0)
            {
                DrawPage3(document, user, responsibilities);
            }

            document.Close();
            return stream.ToArray();
        }

        private void DrawPage1(SKDocument document, User user, WorkReport workReport,
            string roleName, string shiftType, TimeSpan duration, List<ProductOutput> outputs)
        {
            using var canvas = document.BeginPage(PdfPageWidth, PdfPageHeight);
            canvas.Clear(SKColors.White);

            float y = Margin;

            // Title
            using var titlePaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 18,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };
            canvas.DrawText("ОТЧЁТ О СМЕНЕ", PdfPageWidth / 2 - 80, y, titlePaint);
            y += LineHeight * 2;

            // Header info
            using var textPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 12,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal)
            };

            using var boldPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 12,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };

            // Draw header table
            DrawHeaderTable(canvas, ref y, user, workReport, roleName, shiftType, duration, textPaint, boldPaint);

            y += LineHeight;

            // Production section
            if (outputs.Count > 0)
            {
                canvas.DrawText("ПРОИЗВОДСТВО", Margin, y, boldPaint);
                y += LineHeight;

                DrawProductionTable(canvas, ref y, outputs, textPaint, boldPaint);
            }
            else
            {
                canvas.DrawText("Производство: нет данных", Margin, y, textPaint);
                y += LineHeight;
            }

            // Totals
            y += LineHeight;
            var totalProduced = outputs.Sum(o => o.ProducedQuantity);
            var totalDefects = outputs.Sum(o => o.DefectQuantity);
            var totalEco = outputs.Sum(o => o.EcoQuantity);

            canvas.DrawText($"ИТОГО: Выпущено: {totalProduced}, Брак: {totalDefects}, Эко: {totalEco}",
                Margin, y, boldPaint);

            // Footer with signature line
            y = PdfPageHeight - Margin - LineHeight * 3;
            canvas.DrawText("Подпись работника: _____________________", Margin, y, textPaint);
            y += LineHeight * 1.5f;
            canvas.DrawText("Подпись старшего: _____________________", Margin, y, textPaint);
            y += LineHeight * 1.5f;
            canvas.DrawText($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}", Margin, y, textPaint);

            document.EndPage();
        }

        private void DrawHeaderTable(SKCanvas canvas, ref float y, User user, WorkReport workReport,
            string roleName, string shiftType, TimeSpan duration, SKPaint textPaint, SKPaint boldPaint)
        {
            var tableWidth = PdfPageWidth - 2 * Margin;
            var colWidth = tableWidth / 2;

            using var linePaint = new SKPaint
            {
                Color = SKColors.Black,
                StrokeWidth = 1,
                IsStroke = true,
                IsAntialias = true
            };

            // Row 1
            DrawTableRow(canvas, ref y, Margin, tableWidth,
                new[] { ("Дата (день/ночь)", $"{workReport.Date:dd.MM.yyyy} ({shiftType})"),
                        ("Фамилия", $"{user.Surname} {user.Name}") },
                textPaint, boldPaint, linePaint);

            // Row 2
            DrawTableRow(canvas, ref y, Margin, tableWidth,
                new[] { ("Должность", roleName),
                        ("Смена", $"{workReport.StartWork:HH:mm} - {workReport.FinishWork:HH:mm}") },
                textPaint, boldPaint, linePaint);

            // Row 3
            DrawTableRow(canvas, ref y, Margin, tableWidth,
                new[] { ("Продолжительность", $"{duration.Hours}ч {duration.Minutes}мин"),
                        ("Особые отметки", workReport.Note ?? "-") },
                textPaint, boldPaint, linePaint);
        }

        private void DrawTableRow(SKCanvas canvas, ref float y, float x, float width,
            (string label, string value)[] cells, SKPaint textPaint, SKPaint boldPaint, SKPaint linePaint)
        {
            var cellWidth = width / cells.Length;
            var rowHeight = TableRowHeight;

            // Draw cells
            for (int i = 0; i < cells.Length; i++)
            {
                var cellX = x + i * cellWidth;

                // Draw cell border
                canvas.DrawRect(cellX, y, cellWidth, rowHeight, linePaint);

                // Draw label
                canvas.DrawText(cells[i].label + ":", cellX + 5, y + 12, boldPaint);

                // Draw value
                canvas.DrawText(cells[i].value, cellX + 5, y + 22, textPaint);
            }

            y += rowHeight;
        }

        private void DrawProductionTable(SKCanvas canvas, ref float y, List<ProductOutput> outputs,
            SKPaint textPaint, SKPaint boldPaint)
        {
            var columns = new[] { "Продукт", "Выпущено", "Брак", "Эко", "Ед.изм." };
            var colWidths = new[] { 200f, 80f, 80f, 80f, 75f };

            using var linePaint = new SKPaint
            {
                Color = SKColors.Black,
                StrokeWidth = 1,
                IsStroke = true,
                IsAntialias = true
            };

            // Header row
            float x = Margin;
            for (int i = 0; i < columns.Length; i++)
            {
                canvas.DrawRect(x, y, colWidths[i], TableRowHeight, linePaint);
                canvas.DrawText(columns[i], x + 5, y + 16, boldPaint);
                x += colWidths[i];
            }
            y += TableRowHeight;

            // Data rows
            foreach (var output in outputs)
            {
                if (y > PdfPageHeight - Margin - 100)
                {
                    break; // Prevent overflow
                }

                x = Margin;
                var values = new[]
                {
                    output.Product?.Name ?? $"Продукт #{output.ProductId}",
                    output.ProducedQuantity.ToString(),
                    output.DefectQuantity.ToString(),
                    output.EcoQuantity.ToString(),
                    output.MeasuringUnit ?? "шт"
                };

                for (int i = 0; i < values.Length; i++)
                {
                    canvas.DrawRect(x, y, colWidths[i], TableRowHeight, linePaint);
                    var displayText = values[i].Length > 25 ? values[i].Substring(0, 22) + "..." : values[i];
                    canvas.DrawText(displayText, x + 5, y + 16, textPaint);
                    x += colWidths[i];
                }
                y += TableRowHeight;
            }
        }

        private void DrawPage2(SKDocument document, User user, List<PartRequest> requests)
        {
            using var canvas = document.BeginPage(PdfPageWidth, PdfPageHeight);
            canvas.Clear(SKColors.White);

            float y = Margin;

            using var titlePaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 16,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };

            using var textPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 11,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal)
            };

            using var boldPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 11,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };

            using var linePaint = new SKPaint
            {
                Color = SKColors.Black,
                StrokeWidth = 1,
                IsStroke = true,
                IsAntialias = true
            };

            canvas.DrawText($"ЗАЯВКИ НА ПЕРЕМЕЩЕНИЕ - {user.Surname} {user.Name}", Margin, y, titlePaint);
            y += LineHeight * 2;

            var columns = new[] { "Материал", "Кол-во", "Откуда", "Куда", "Статус" };
            var colWidths = new[] { 150f, 60f, 100f, 100f, 105f };

            // Header
            float x = Margin;
            for (int i = 0; i < columns.Length; i++)
            {
                canvas.DrawRect(x, y, colWidths[i], TableRowHeight, linePaint);
                canvas.DrawText(columns[i], x + 3, y + 16, boldPaint);
                x += colWidths[i];
            }
            y += TableRowHeight;

            // Data
            foreach (var request in requests)
            {
                if (y > PdfPageHeight - Margin - 50)
                {
                    break;
                }

                x = Margin;
                var statusText = request.Status switch
                {
                    PartRequestStatus.Pending => "Ожидает",
                    PartRequestStatus.Approved => "Одобрено",
                    PartRequestStatus.Rejected => "Отклонено",
                    _ => "-"
                };

                var values = new[]
                {
                    request.Material?.Name ?? $"#{request.MaterialId}",
                    $"{request.Quantity} {request.MeasuringType ?? "шт"}",
                    request.FromWarehouse?.Name ?? "-",
                    request.ToWarehouse?.Name ?? "-",
                    statusText
                };

                for (int i = 0; i < values.Length; i++)
                {
                    canvas.DrawRect(x, y, colWidths[i], TableRowHeight, linePaint);
                    var displayText = values[i].Length > 20 ? values[i].Substring(0, 17) + "..." : values[i];
                    canvas.DrawText(displayText, x + 3, y + 16, textPaint);
                    x += colWidths[i];
                }
                y += TableRowHeight;
            }

            // Summary
            y += LineHeight;
            var approved = requests.Count(r => r.Status == PartRequestStatus.Approved);
            var pending = requests.Count(r => r.Status == PartRequestStatus.Pending);
            var rejected = requests.Count(r => r.Status == PartRequestStatus.Rejected);
            canvas.DrawText($"Всего заявок: {requests.Count} (одобрено: {approved}, ожидает: {pending}, отклонено: {rejected})",
                Margin, y, boldPaint);

            document.EndPage();
        }

        private void DrawPage3(SKDocument document, User user, List<Responsibility> responsibilities)
        {
            using var canvas = document.BeginPage(PdfPageWidth, PdfPageHeight);
            canvas.Clear(SKColors.White);

            float y = Margin;

            using var titlePaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 16,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };

            using var textPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 11,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal)
            };

            using var boldPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 11,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };

            using var linePaint = new SKPaint
            {
                Color = SKColors.Black,
                StrokeWidth = 1,
                IsStroke = true,
                IsAntialias = true
            };

            canvas.DrawText($"ОТВЕТСТВЕННОСТИ - {user.Surname} {user.Name}", Margin, y, titlePaint);
            y += LineHeight * 2;

            var columns = new[] { "Тип", "Наименование", "Количество", "Ед.изм.", "Назначено" };
            var colWidths = new[] { 80f, 180f, 80f, 70f, 105f };

            // Header
            float x = Margin;
            for (int i = 0; i < columns.Length; i++)
            {
                canvas.DrawRect(x, y, colWidths[i], TableRowHeight, linePaint);
                canvas.DrawText(columns[i], x + 3, y + 16, boldPaint);
                x += colWidths[i];
            }
            y += TableRowHeight;

            // Data
            foreach (var resp in responsibilities)
            {
                if (y > PdfPageHeight - Margin - 50)
                {
                    break;
                }

                x = Margin;
                var typeName = resp.MaterialId.HasValue ? "Материал" : "Продукт";
                var itemName = resp.Material?.Name ?? resp.Product?.Name ?? "-";
                var quantity = resp.Quantity?.ToString() ?? "Весь";

                var values = new[]
                {
                    typeName,
                    itemName,
                    quantity,
                    resp.MeasuringUnit ?? "-",
                    resp.AssignedAt.ToString("dd.MM.yyyy HH:mm")
                };

                for (int i = 0; i < values.Length; i++)
                {
                    canvas.DrawRect(x, y, colWidths[i], TableRowHeight, linePaint);
                    var displayText = values[i].Length > 22 ? values[i].Substring(0, 19) + "..." : values[i];
                    canvas.DrawText(displayText, x + 3, y + 16, textPaint);
                    x += colWidths[i];
                }
                y += TableRowHeight;
            }

            // Summary
            y += LineHeight;
            var materialCount = responsibilities.Count(r => r.MaterialId.HasValue);
            var productCount = responsibilities.Count(r => r.ProductId.HasValue);
            canvas.DrawText($"Всего: {responsibilities.Count} (материалов: {materialCount}, продуктов: {productCount})",
                Margin, y, boldPaint);

            document.EndPage();
        }
    }
}
