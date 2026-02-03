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
        private readonly IResponsibilityShiftSnapshotService _snapshotService;
        private readonly IResponsibilityFillingService _fillingService;

        public ShiftReportService(AppDbContext context, ILogger<ShiftReportService> logger, IWebHostEnvironment env,
            IResponsibilityShiftSnapshotService snapshotService, IResponsibilityFillingService fillingService)
        {
            _context = context;
            _logger = logger;
            _reportsDirectory = Path.Combine(env.ContentRootPath, "reports");
            _snapshotService = snapshotService;
            _fillingService = fillingService;
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
            var reprocessings = await GetReprocessingsForShift(workReport.UserId, shiftStart, shiftEnd);
            var responsibilitySnapshot = await _snapshotService.GetByWorkReportIdAsync(workReportId);
            var responsibilityEnd = await _fillingService.GetResponsibilityFillingsByUserAsync(workReport.UserId);

            // Generate Excel (stored in DB, not on disk)
            var excelBytes = GenerateExcel(user, workReport, productOutputs, partRequests, responsibilities, reprocessings,
                responsibilitySnapshot, responsibilityEnd);

            var fileName = $"report_{user.Surname}_{user.Name}_{shiftStart:yyyy-MM-dd_HH-mm}.xlsx";

            // Create summary
            var summary = GenerateSummary(productOutputs, partRequests, responsibilities);

            // Save to database (file content in DB, no server folder)
            var report = new ShiftReport
            {
                WorkReportId = workReportId,
                UserId = workReport.UserId,
                FileName = fileName,
                FilePath = null,
                FileContent = excelBytes,
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

            if (report.FileContent != null && report.FileContent.Length > 0)
            {
                return report.FileContent;
            }

            // Legacy: read from disk if stored in server folder
            if (!string.IsNullOrEmpty(report.FilePath) && File.Exists(report.FilePath))
            {
                return await File.ReadAllBytesAsync(report.FilePath);
            }

            throw new FileNotFoundException($"Report file not found for ShiftReport ID {reportId}");
        }

        public async Task<bool> DeleteReportAsync(int id)
        {
            var report = await _context.Set<ShiftReport>().FindAsync(id);
            if (report == null)
            {
                return false;
            }

            // Delete file from disk only if it was stored there (legacy)
            if (!string.IsNullOrEmpty(report.FilePath) && File.Exists(report.FilePath))
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

        private async Task<List<Reprocessing>> GetReprocessingsForShift(int userId, DateTime shiftStart, DateTime shiftEnd)
        {
            return await _context.Reprocessings
                .Include(r => r.Warehouse)
                .Include(r => r.SourceMaterial)
                .Include(r => r.Sources)
                    .ThenInclude(s => s.Material)
                .Include(r => r.Items)
                    .ThenInclude(i => i.Material)
                .Include(r => r.Items)
                    .ThenInclude(i => i.Product)
                .Where(r => r.UserId == userId && r.CreatedAt >= shiftStart && r.CreatedAt <= shiftEnd)
                .OrderBy(r => r.CreatedAt)
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
            List<ProductOutput> outputs, List<PartRequest> requests, List<Responsibility> responsibilities,
            List<Reprocessing> reprocessings, ResponsibilityShiftSnapshot? responsibilitySnapshot,
            List<ResponsibilityFilling> responsibilityEnd)
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

            // Sheet: Reprocessing (переработка) — что произвели, что использовали, сколько вышло
            if (reprocessings.Count > 0)
            {
                CreateReprocessingSheet(workbook, user, reprocessings);
            }

            // Sheet: Ответственность на начало и конец смены (начальный и конечный остаток за человеком)
            CreateResponsibilityStartEndSheet(workbook, user, responsibilitySnapshot, responsibilityEnd);

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

        private void CreateReprocessingSheet(XLWorkbook workbook, User user, List<Reprocessing> reprocessings)
        {
            var ws = workbook.Worksheets.Add("Переработка");

            int row = 1;

            ws.Cell(row, 1).Value = $"ПЕРЕРАБОТКА ЗА СМЕНУ — {user.Surname} {user.Name}";
            ws.Range(row, 1, row, 7).Merge();
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontSize = 14;
            row += 2;

            var headers = new[] { "Дата", "Время", "Склад", "Использовано (исходное)", "Получено", "Кол-во", "Ед.изм." };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(row, i + 1).Value = headers[i];
                ws.Cell(row, i + 1).Style.Font.Bold = true;
                ws.Cell(row, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                ws.Cell(row, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }
            row++;

            foreach (var r in reprocessings)
            {
                var sourceText = string.Join("; ", r.Sources.Select(s => $"{s.Material?.Name ?? $"#{s.MaterialId}"}: {s.Quantity} {s.MeasuringType ?? "шт"}"));
                if (string.IsNullOrEmpty(sourceText))
                    sourceText = $"{r.SourceMaterial?.Name ?? $"#{r.SourceMaterialId}"}: {r.SourceQuantity}";

                foreach (var item in r.Items)
                {
                    var outputName = item.MaterialId.HasValue
                        ? (item.Material?.Name ?? $"Материал #{item.MaterialId}")
                        : (item.Product?.Name ?? $"Продукт #{item.ProductId}");
                    ws.Cell(row, 1).Value = r.CreatedAt.ToString("dd.MM.yyyy");
                    ws.Cell(row, 2).Value = r.CreatedAt.ToString("HH:mm");
                    ws.Cell(row, 3).Value = r.Warehouse?.Name ?? "-";
                    ws.Cell(row, 4).Value = sourceText;
                    ws.Cell(row, 5).Value = outputName;
                    ws.Cell(row, 6).Value = item.Quantity;
                    ws.Cell(row, 7).Value = item.MeasuringType ?? "шт";
                    for (int i = 1; i <= 7; i++)
                        ws.Cell(row, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    row++;
                }
            }

            row += 2;
            ws.Cell(row, 1).Value = $"Всего операций переработки: {reprocessings.Count}";
            ws.Cell(row, 1).Style.Font.Bold = true;

            ws.Columns().AdjustToContents();
        }

        private void CreateResponsibilityStartEndSheet(XLWorkbook workbook, User user,
            ResponsibilityShiftSnapshot? snapshot, List<ResponsibilityFilling> endFillings)
        {
            // Excel: sheet name max 31 chars. "Ответственность на начало и конец" = 33
            var ws = workbook.Worksheets.Add("Ответств. на начало и конец");

            int row = 1;

            ws.Cell(row, 1).Value = $"ОТВЕТСТВЕННОСТЬ НА НАЧАЛО И КОНЕЦ СМЕНЫ — {user.Surname} {user.Name}";
            ws.Range(row, 1, row, 5).Merge();
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontSize = 14;
            row += 2;

            // На начало смены
            ws.Cell(row, 1).Value = "На начало смены";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontSize = 12;
            row++;

            var startHeaders = new[] { "Тип", "Наименование", "Склад", "Количество", "Ед.изм." };
            for (int i = 0; i < startHeaders.Length; i++)
            {
                ws.Cell(row, i + 1).Value = startHeaders[i];
                ws.Cell(row, i + 1).Style.Font.Bold = true;
                ws.Cell(row, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                ws.Cell(row, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }
            row++;

            if (snapshot?.Items != null && snapshot.Items.Count > 0)
            {
                foreach (var item in snapshot.Items.OrderBy(i => i.MaterialId.HasValue ? 0 : 1).ThenBy(i => i.Material?.Name ?? i.Product?.Name ?? ""))
                {
                    var typeName = item.MaterialId.HasValue ? "Материал" : "Продукт";
                    var itemName = item.Material?.Name ?? item.Product?.Name ?? "-";
                    ws.Cell(row, 1).Value = typeName;
                    ws.Cell(row, 2).Value = itemName;
                    ws.Cell(row, 3).Value = item.Warehouse?.Name ?? "-";
                    ws.Cell(row, 4).Value = item.Quantity;
                    ws.Cell(row, 5).Value = item.MeasuringUnit ?? "-";
                    for (int i = 1; i <= 5; i++) ws.Cell(row, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    row++;
                }
            }
            else
            {
                ws.Cell(row, 1).Value = "Снимок не был сделан или отсутствует";
                ws.Cell(row, 1).Style.Font.Italic = true;
                ws.Range(row, 1, row, 5).Merge();
                row++;
            }

            row += 2;

            // На конец смены (агрегируем endFillings по WarehouseId + MaterialId + ProductId)
            ws.Cell(row, 1).Value = "На конец смены";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontSize = 12;
            row++;

            for (int i = 0; i < startHeaders.Length; i++)
            {
                ws.Cell(row, i + 1).Value = startHeaders[i];
                ws.Cell(row, i + 1).Style.Font.Bold = true;
                ws.Cell(row, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                ws.Cell(row, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }
            row++;

            var endAggregated = endFillings
                .Where(rf => rf.IsActive)
                .GroupBy(rf => new { rf.WarehouseId, rf.MaterialId, rf.ProductId })
                .Select(g => new
                {
                    g.Key.WarehouseId,
                    g.Key.MaterialId,
                    g.Key.ProductId,
                    Quantity = g.Sum(rf => rf.Quantity),
                    MeasuringUnit = g.First().MeasuringUnit,
                    Material = g.First().Material,
                    Product = g.First().Product,
                    Warehouse = g.First().Warehouse
                })
                .OrderBy(x => x.MaterialId.HasValue ? 0 : 1)
                .ThenBy(x => x.Material?.Name ?? x.Product?.Name ?? "")
                .ToList();

            if (endAggregated.Count > 0)
            {
                foreach (var a in endAggregated)
                {
                    var typeName = a.MaterialId.HasValue ? "Материал" : "Продукт";
                    var itemName = a.Material?.Name ?? a.Product?.Name ?? "-";
                    ws.Cell(row, 1).Value = typeName;
                    ws.Cell(row, 2).Value = itemName;
                    ws.Cell(row, 3).Value = a.Warehouse?.Name ?? "-";
                    ws.Cell(row, 4).Value = a.Quantity;
                    ws.Cell(row, 5).Value = a.MeasuringUnit ?? "-";
                    for (int i = 1; i <= 5; i++) ws.Cell(row, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    row++;
                }
            }
            else
            {
                ws.Cell(row, 1).Value = "Нет активной ответственности на конец смены";
                ws.Cell(row, 1).Style.Font.Italic = true;
                ws.Range(row, 1, row, 5).Merge();
                row++;
            }

            ws.Columns().AdjustToContents();
        }
    }
}
