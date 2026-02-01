using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;
using ClosedXML.Excel;

namespace server.Services
{
    public class ShiftReportService : IShiftReportService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ShiftReportService> _logger;
        private readonly string _reportsDirectory;

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

            // Generate Excel
            var excelBytes = GenerateExcel(user, workReport, productOutputs, partRequests, responsibilities);

            // Save to file
            var fileName = $"report_{user.Surname}_{user.Name}_{shiftStart:yyyy-MM-dd_HH-mm}.xlsx";
            var filePath = Path.Combine(_reportsDirectory, fileName);
            await File.WriteAllBytesAsync(filePath, excelBytes);

            // Create summary
            var summary = GenerateSummary(productOutputs, partRequests, responsibilities);

            // Save to database
            var report = new ShiftReport
            {
                WorkReportId = workReportId,
                UserId = workReport.UserId,
                FileName = fileName,
                FilePath = filePath,
                FileSize = excelBytes.Length,
                CreatedAt = DateTime.UtcNow.AddHours(4), // Assuming UTC+4 timezone
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

        private byte[] GenerateExcel(User user, WorkReport workReport,
            List<ProductOutput> outputs, List<PartRequest> requests, List<Responsibility> responsibilities)
        {
            using var workbook = new XLWorkbook();

            var roleName = user.Role?.Name ?? "Сотрудник";
            var shiftType = workReport.StartWork.Hour >= 8 && workReport.StartWork.Hour < 20 ? "День" : "Ночь";
            var duration = workReport.FinishWork!.Value - workReport.StartWork;

            // Sheet 1: Main report with header and production
            CreateMainSheet(workbook, user, workReport, roleName, shiftType, duration, outputs);

            // Sheet 2: Part Requests (if any)
            if (requests.Count > 0)
            {
                CreatePartRequestsSheet(workbook, user, requests);
            }

            // Sheet 3: Responsibilities (if any)
            if (responsibilities.Count > 0)
            {
                CreateResponsibilitiesSheet(workbook, user, responsibilities);
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private void CreateMainSheet(XLWorkbook workbook, User user, WorkReport workReport,
            string roleName, string shiftType, TimeSpan duration, List<ProductOutput> outputs)
        {
            var ws = workbook.Worksheets.Add("Отчёт о смене");

            int row = 1;

            // Title
            ws.Cell(row, 1).Value = "ОТЧЁТ О СМЕНЕ";
            ws.Range(row, 1, row, 5).Merge();
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontSize = 16;
            ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            // Header info table
            var headerData = new[]
            {
                ("Дата:", $"{workReport.Date:dd.MM.yyyy} ({shiftType})"),
                ("Фамилия:", $"{user.Surname} {user.Name}"),
                ("Должность:", roleName),
                ("Смена:", $"{workReport.StartWork:HH:mm} - {workReport.FinishWork:HH:mm}"),
                ("Продолжительность:", $"{duration.Hours}ч {duration.Minutes}мин"),
                ("Особые отметки:", workReport.Note ?? "-")
            };

            foreach (var (label, value) in headerData)
            {
                ws.Cell(row, 1).Value = label;
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 2).Value = value;
                ws.Range(row, 2, row, 3).Merge();
                row++;
            }

            row += 2;

            // Production section
            ws.Cell(row, 1).Value = "ПРОИЗВОДСТВО";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontSize = 14;
            row++;

            if (outputs.Count > 0)
            {
                // Header row
                var headers = new[] { "Артикул", "Продукт", "Станок", "Выпущено", "Брак", "Эко", "Ед.изм." };
                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cell(row, i + 1).Value = headers[i];
                    ws.Cell(row, i + 1).Style.Font.Bold = true;
                    ws.Cell(row, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    ws.Cell(row, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
                row++;

                // Data rows
                foreach (var output in outputs)
                {
                    ws.Cell(row, 1).Value = output.Product?.Code ?? "-";
                    ws.Cell(row, 2).Value = output.Product?.Name ?? $"Продукт #{output.ProductId}";
                    ws.Cell(row, 3).Value = output.Machine?.Name ?? "-";
                    ws.Cell(row, 4).Value = output.ProducedQuantity;
                    ws.Cell(row, 5).Value = output.DefectQuantity;
                    ws.Cell(row, 6).Value = output.EcoQuantity;
                    ws.Cell(row, 7).Value = output.MeasuringUnit ?? "шт";

                    for (int i = 1; i <= 7; i++)
                    {
                        ws.Cell(row, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                    row++;
                }

                // Totals row
                row++;
                ws.Cell(row, 1).Value = "ИТОГО:";
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 4).Value = outputs.Sum(o => o.ProducedQuantity);
                ws.Cell(row, 4).Style.Font.Bold = true;
                ws.Cell(row, 5).Value = outputs.Sum(o => o.DefectQuantity);
                ws.Cell(row, 5).Style.Font.Bold = true;
                ws.Cell(row, 6).Value = outputs.Sum(o => o.EcoQuantity);
                ws.Cell(row, 6).Style.Font.Bold = true;
            }
            else
            {
                ws.Cell(row, 1).Value = "Нет данных о производстве";
                ws.Cell(row, 1).Style.Font.Italic = true;
            }

            row += 3;

            // Signature lines
            ws.Cell(row, 1).Value = "Подпись работника:";
            ws.Cell(row, 2).Value = "_____________________";
            row++;
            ws.Cell(row, 1).Value = "Подпись старшего:";
            ws.Cell(row, 2).Value = "_____________________";
            row += 2;
            ws.Cell(row, 1).Value = $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}";
            ws.Cell(row, 1).Style.Font.Italic = true;

            // Auto-fit columns
            ws.Columns().AdjustToContents();
        }

        private void CreatePartRequestsSheet(XLWorkbook workbook, User user, List<PartRequest> requests)
        {
            var ws = workbook.Worksheets.Add("Заявки на перемещение");

            int row = 1;

            // Title
            ws.Cell(row, 1).Value = $"ЗАЯВКИ НА ПЕРЕМЕЩЕНИЕ - {user.Surname} {user.Name}";
            ws.Range(row, 1, row, 5).Merge();
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontSize = 14;
            row += 2;

            // Header row
            var headers = new[] { "Материал", "Количество", "Откуда", "Куда", "Статус" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(row, i + 1).Value = headers[i];
                ws.Cell(row, i + 1).Style.Font.Bold = true;
                ws.Cell(row, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                ws.Cell(row, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }
            row++;

            // Data rows
            foreach (var request in requests)
            {
                var statusText = request.Status switch
                {
                    PartRequestStatus.Pending => "Ожидает",
                    PartRequestStatus.Approved => "Одобрено",
                    PartRequestStatus.Rejected => "Отклонено",
                    _ => "-"
                };

                ws.Cell(row, 1).Value = request.Material?.Name ?? $"#{request.MaterialId}";
                ws.Cell(row, 2).Value = $"{request.Quantity} {request.MeasuringType ?? "шт"}";
                ws.Cell(row, 3).Value = request.FromWarehouse?.Name ?? "-";
                ws.Cell(row, 4).Value = request.ToWarehouse?.Name ?? "-";
                ws.Cell(row, 5).Value = statusText;

                // Color-code status
                var statusColor = request.Status switch
                {
                    PartRequestStatus.Approved => XLColor.LightGreen,
                    PartRequestStatus.Rejected => XLColor.LightCoral,
                    _ => XLColor.LightYellow
                };
                ws.Cell(row, 5).Style.Fill.BackgroundColor = statusColor;

                for (int i = 1; i <= 5; i++)
                {
                    ws.Cell(row, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
                row++;
            }

            // Summary
            row += 2;
            var approved = requests.Count(r => r.Status == PartRequestStatus.Approved);
            var pending = requests.Count(r => r.Status == PartRequestStatus.Pending);
            var rejected = requests.Count(r => r.Status == PartRequestStatus.Rejected);

            ws.Cell(row, 1).Value = $"Всего заявок: {requests.Count}";
            ws.Cell(row, 1).Style.Font.Bold = true;
            row++;
            ws.Cell(row, 1).Value = $"Одобрено: {approved}, Ожидает: {pending}, Отклонено: {rejected}";

            ws.Columns().AdjustToContents();
        }

        private void CreateResponsibilitiesSheet(XLWorkbook workbook, User user, List<Responsibility> responsibilities)
        {
            var ws = workbook.Worksheets.Add("Ответственности");

            int row = 1;

            // Title
            ws.Cell(row, 1).Value = $"ОТВЕТСТВЕННОСТИ - {user.Surname} {user.Name}";
            ws.Range(row, 1, row, 5).Merge();
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontSize = 14;
            row += 2;

            // Header row
            var headers = new[] { "Тип", "Наименование", "Количество", "Ед.изм.", "Назначено" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(row, i + 1).Value = headers[i];
                ws.Cell(row, i + 1).Style.Font.Bold = true;
                ws.Cell(row, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                ws.Cell(row, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }
            row++;

            // Data rows
            foreach (var resp in responsibilities)
            {
                var typeName = resp.MaterialId.HasValue ? "Материал" : "Продукт";
                var itemName = resp.Material?.Name ?? resp.Product?.Name ?? "-";
                var quantity = resp.Quantity?.ToString() ?? "Весь";

                ws.Cell(row, 1).Value = typeName;
                ws.Cell(row, 2).Value = itemName;
                ws.Cell(row, 3).Value = quantity;
                ws.Cell(row, 4).Value = resp.MeasuringUnit ?? "-";
                ws.Cell(row, 5).Value = resp.AssignedAt.ToString("dd.MM.yyyy HH:mm");

                for (int i = 1; i <= 5; i++)
                {
                    ws.Cell(row, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
                row++;
            }

            // Summary
            row += 2;
            var materialCount = responsibilities.Count(r => r.MaterialId.HasValue);
            var productCount = responsibilities.Count(r => r.ProductId.HasValue);

            ws.Cell(row, 1).Value = $"Всего: {responsibilities.Count} (материалов: {materialCount}, продуктов: {productCount})";
            ws.Cell(row, 1).Style.Font.Bold = true;

            ws.Columns().AdjustToContents();
        }
    }
}
