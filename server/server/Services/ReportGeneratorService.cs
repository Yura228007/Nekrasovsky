using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;
using ClosedXML.Excel;

namespace server.Services
{
    public interface IReportGeneratorService
    {
        Task<byte[]> GeneratePartRequestReportAsync(DateTime startDate, DateTime endDate, int? userId = null);
        Task<byte[]> GenerateReprocessingReportAsync(DateTime startDate, DateTime endDate, int? userId = null);
    }

    public class ReportGeneratorService : IReportGeneratorService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ReportGeneratorService> _logger;

        public ReportGeneratorService(AppDbContext context, ILogger<ReportGeneratorService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Генерирует отчёт по заявкам на перемещение (отправленные и принятые)
        /// </summary>
        public async Task<byte[]> GeneratePartRequestReportAsync(DateTime startDate, DateTime endDate, int? userId = null)
        {
            var query = _context.PartRequests
                .Include(pr => pr.Material)
                .Include(pr => pr.FromUser)
                .Include(pr => pr.ToUser)
                .Include(pr => pr.FromWarehouse)
                .Include(pr => pr.ToWarehouse)
                .Where(pr => pr.CreatedAt >= startDate && pr.CreatedAt <= endDate);

            if (userId.HasValue)
            {
                query = query.Where(pr => pr.FromUserId == userId.Value || pr.ToUserId == userId.Value);
            }

            var requests = await query
                .OrderBy(pr => pr.CreatedAt)
                .ToListAsync();

            using var workbook = new XLWorkbook();

            // Sheet 1: Sent requests (Отправленные заявки)
            CreateSentRequestsSheet(workbook, requests, userId, startDate, endDate);

            // Sheet 2: Received requests (Принятые заявки)
            CreateReceivedRequestsSheet(workbook, requests, userId, startDate, endDate);

            // Sheet 3: Summary by material (Сводка по материалам)
            CreateMaterialSummarySheet(workbook, requests, startDate, endDate);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        /// <summary>
        /// Генерирует отчёт по переработке
        /// </summary>
        public async Task<byte[]> GenerateReprocessingReportAsync(DateTime startDate, DateTime endDate, int? userId = null)
        {
            var query = _context.Reprocessings
                .Include(r => r.User)
                .Include(r => r.Warehouse)
                .Include(r => r.SourceMaterial)
                .Include(r => r.Sources)
                    .ThenInclude(s => s.Material)
                .Include(r => r.Items)
                    .ThenInclude(i => i.Material)
                .Include(r => r.Items)
                    .ThenInclude(i => i.Product)
                .Where(r => r.CreatedAt >= startDate && r.CreatedAt <= endDate);

            if (userId.HasValue)
            {
                query = query.Where(r => r.UserId == userId.Value);
            }

            var reprocessings = await query
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();

            using var workbook = new XLWorkbook();

            // Sheet 1: Reprocessing operations (Операции переработки)
            CreateReprocessingOperationsSheet(workbook, reprocessings, startDate, endDate);

            // Sheet 2: Source materials summary (Сводка по исходным материалам)
            CreateSourceMaterialsSummarySheet(workbook, reprocessings, startDate, endDate);

            // Sheet 3: Output products summary (Сводка по выходу продукции)
            CreateOutputSummarySheet(workbook, reprocessings, startDate, endDate);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        #region PartRequest Report Sheets

        private void CreateSentRequestsSheet(XLWorkbook workbook, List<PartRequest> requests, int? userId, DateTime startDate, DateTime endDate)
        {
            var ws = workbook.Worksheets.Add("Отправленные заявки");
            int row = 1;

            // Title
            ws.Cell(row, 1).Value = "ОТПРАВЛЕННЫЕ ЗАЯВКИ НА ПЕРЕМЕЩЕНИЕ";
            ws.Range(row, 1, row, 8).Merge();
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontSize = 16;
            ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            // Period info
            ws.Cell(row, 1).Value = $"Период: {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}";
            row += 2;

            var sentRequests = userId.HasValue
                ? requests.Where(r => r.FromUserId == userId.Value).ToList()
                : requests;

            if (sentRequests.Count > 0)
            {
                // Header row
                var headers = new[] { "Дата", "Материал", "Кол-во", "Ед.изм.", "Откуда", "Куда", "Кому", "Статус" };
                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cell(row, i + 1).Value = headers[i];
                    ws.Cell(row, i + 1).Style.Font.Bold = true;
                    ws.Cell(row, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    ws.Cell(row, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
                row++;

                foreach (var request in sentRequests)
                {
                    ws.Cell(row, 1).Value = request.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
                    ws.Cell(row, 2).Value = request.Material?.Name ?? "-";
                    ws.Cell(row, 3).Value = request.Quantity;
                    ws.Cell(row, 4).Value = request.MeasuringType ?? "шт";
                    ws.Cell(row, 5).Value = request.FromWarehouse?.Name ?? "-";
                    ws.Cell(row, 6).Value = request.ToWarehouse?.Name ?? "-";
                    ws.Cell(row, 7).Value = $"{request.ToUser?.Name} {request.ToUser?.Surname}";
                    ws.Cell(row, 8).Value = GetStatusText(request.Status);

                    // Color-code status
                    ws.Cell(row, 8).Style.Fill.BackgroundColor = GetStatusColor(request.Status);

                    for (int i = 1; i <= 8; i++)
                    {
                        ws.Cell(row, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                    row++;
                }

                // Summary
                row += 2;
                var approved = sentRequests.Count(r => r.Status == PartRequestStatus.Approved);
                var pending = sentRequests.Count(r => r.Status == PartRequestStatus.Pending);
                var rejected = sentRequests.Count(r => r.Status == PartRequestStatus.Rejected);

                ws.Cell(row, 1).Value = $"Всего: {sentRequests.Count}";
                ws.Cell(row, 1).Style.Font.Bold = true;
                row++;
                ws.Cell(row, 1).Value = $"Одобрено: {approved}, Ожидает: {pending}, Отклонено: {rejected}";
            }
            else
            {
                ws.Cell(row, 1).Value = "Нет отправленных заявок за указанный период";
                ws.Cell(row, 1).Style.Font.Italic = true;
            }

            ws.Columns().AdjustToContents();
        }

        private void CreateReceivedRequestsSheet(XLWorkbook workbook, List<PartRequest> requests, int? userId, DateTime startDate, DateTime endDate)
        {
            var ws = workbook.Worksheets.Add("Принятые заявки");
            int row = 1;

            // Title
            ws.Cell(row, 1).Value = "ПРИНЯТЫЕ ЗАЯВКИ НА ПЕРЕМЕЩЕНИЕ";
            ws.Range(row, 1, row, 7).Merge();
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontSize = 16;
            ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            // Period info
            ws.Cell(row, 1).Value = $"Период: {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}";
            row += 2;

            var receivedRequests = userId.HasValue
                ? requests.Where(r => r.ToUserId == userId.Value && r.Status == PartRequestStatus.Approved).ToList()
                : requests.Where(r => r.Status == PartRequestStatus.Approved).ToList();

            if (receivedRequests.Count > 0)
            {
                // Header row
                var headers = new[] { "Дата", "Материал", "Кол-во", "Ед.изм.", "От кого", "На склад", "Статус" };
                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cell(row, i + 1).Value = headers[i];
                    ws.Cell(row, i + 1).Style.Font.Bold = true;
                    ws.Cell(row, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    ws.Cell(row, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
                row++;

                foreach (var request in receivedRequests)
                {
                    ws.Cell(row, 1).Value = request.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
                    ws.Cell(row, 2).Value = request.Material?.Name ?? "-";
                    ws.Cell(row, 3).Value = request.Quantity;
                    ws.Cell(row, 4).Value = request.MeasuringType ?? "шт";
                    ws.Cell(row, 5).Value = $"{request.FromUser?.Surname} {request.FromUser?.Name}";
                    ws.Cell(row, 6).Value = request.ToWarehouse?.Name ?? "-";
                    ws.Cell(row, 7).Value = "Принято";
                    ws.Cell(row, 7).Style.Fill.BackgroundColor = XLColor.LightGreen;

                    for (int i = 1; i <= 7; i++)
                    {
                        ws.Cell(row, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                    row++;
                }

                // Summary
                row += 2;
                var totalQuantity = receivedRequests.Sum(r => r.Quantity);
                ws.Cell(row, 1).Value = $"Всего принято: {receivedRequests.Count} заявок, {totalQuantity} единиц";
                ws.Cell(row, 1).Style.Font.Bold = true;
            }
            else
            {
                ws.Cell(row, 1).Value = "Нет принятых заявок за указанный период";
                ws.Cell(row, 1).Style.Font.Italic = true;
            }

            ws.Columns().AdjustToContents();
        }

        private void CreateMaterialSummarySheet(XLWorkbook workbook, List<PartRequest> requests, DateTime startDate, DateTime endDate)
        {
            var ws = workbook.Worksheets.Add("Сводка по материалам");
            int row = 1;

            // Title
            ws.Cell(row, 1).Value = "СВОДКА ПО МАТЕРИАЛАМ";
            ws.Range(row, 1, row, 5).Merge();
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontSize = 16;
            ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            ws.Cell(row, 1).Value = $"Период: {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}";
            row += 2;

            var approvedRequests = requests.Where(r => r.Status == PartRequestStatus.Approved).ToList();

            if (approvedRequests.Count > 0)
            {
                var materialSummary = approvedRequests
                    .GroupBy(r => new { r.MaterialId, MaterialName = r.Material?.Name ?? "-", r.MeasuringType })
                    .Select(g => new
                    {
                        g.Key.MaterialName,
                        g.Key.MeasuringType,
                        TotalQuantity = g.Sum(r => r.Quantity),
                        RequestCount = g.Count()
                    })
                    .OrderByDescending(x => x.TotalQuantity)
                    .ToList();

                // Header row
                var headers = new[] { "Материал", "Кол-во", "Ед.изм.", "Заявок" };
                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cell(row, i + 1).Value = headers[i];
                    ws.Cell(row, i + 1).Style.Font.Bold = true;
                    ws.Cell(row, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    ws.Cell(row, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
                row++;

                foreach (var item in materialSummary)
                {
                    ws.Cell(row, 1).Value = item.MaterialName;
                    ws.Cell(row, 2).Value = item.TotalQuantity;
                    ws.Cell(row, 3).Value = item.MeasuringType ?? "шт";
                    ws.Cell(row, 4).Value = item.RequestCount;

                    for (int i = 1; i <= 4; i++)
                    {
                        ws.Cell(row, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                    row++;
                }

                row += 2;
                ws.Cell(row, 1).Value = $"Итого материалов: {materialSummary.Count}";
                ws.Cell(row, 1).Style.Font.Bold = true;
            }
            else
            {
                ws.Cell(row, 1).Value = "Нет одобренных заявок за указанный период";
                ws.Cell(row, 1).Style.Font.Italic = true;
            }

            ws.Columns().AdjustToContents();
        }

        #endregion

        #region Reprocessing Report Sheets

        private void CreateReprocessingOperationsSheet(XLWorkbook workbook, List<Reprocessing> reprocessings, DateTime startDate, DateTime endDate)
        {
            var ws = workbook.Worksheets.Add("Операции переработки");
            int row = 1;

            // Title
            ws.Cell(row, 1).Value = "ОТЧЁТ ПО ПЕРЕРАБОТКЕ";
            ws.Range(row, 1, row, 6).Merge();
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontSize = 16;
            ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            ws.Cell(row, 1).Value = $"Период: {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}";
            row += 2;

            if (reprocessings.Count > 0)
            {
                foreach (var reprocessing in reprocessings)
                {
                    // Operation header
                    ws.Cell(row, 1).Value = $"Переработка #{reprocessing.Id}";
                    ws.Cell(row, 1).Style.Font.Bold = true;
                    ws.Cell(row, 1).Style.Font.FontSize = 12;
                    row++;

                    ws.Cell(row, 1).Value = "Дата:";
                    ws.Cell(row, 2).Value = reprocessing.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
                    row++;

                    ws.Cell(row, 1).Value = "Сотрудник:";
                    ws.Cell(row, 2).Value = $"{reprocessing.User?.Surname} {reprocessing.User?.Name}";
                    row++;

                    ws.Cell(row, 1).Value = "Склад:";
                    ws.Cell(row, 2).Value = reprocessing.Warehouse?.Name ?? "-";
                    row++;

                    // Source materials
                    row++;
                    ws.Cell(row, 1).Value = "Исходные материалы:";
                    ws.Cell(row, 1).Style.Font.Bold = true;
                    row++;

                    var sourceHeaders = new[] { "Материал", "Кол-во", "Ед.изм." };
                    for (int i = 0; i < sourceHeaders.Length; i++)
                    {
                        ws.Cell(row, i + 1).Value = sourceHeaders[i];
                        ws.Cell(row, i + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
                        ws.Cell(row, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                    row++;

                    foreach (var source in reprocessing.Sources)
                    {
                        ws.Cell(row, 1).Value = source.Material?.Name ?? "-";
                        ws.Cell(row, 2).Value = source.Quantity;
                        ws.Cell(row, 3).Value = source.MeasuringType ?? "шт";

                        for (int i = 1; i <= 3; i++)
                        {
                            ws.Cell(row, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }
                        row++;
                    }

                    // Output products/materials
                    row++;
                    ws.Cell(row, 1).Value = "Выход:";
                    ws.Cell(row, 1).Style.Font.Bold = true;
                    row++;

                    var outputHeaders = new[] { "Тип", "Наименование", "Кол-во", "Ед.изм." };
                    for (int i = 0; i < outputHeaders.Length; i++)
                    {
                        ws.Cell(row, i + 1).Value = outputHeaders[i];
                        ws.Cell(row, i + 1).Style.Fill.BackgroundColor = XLColor.LightGreen;
                        ws.Cell(row, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                    row++;

                    foreach (var item in reprocessing.Items)
                    {
                        var itemType = item.MaterialId.HasValue ? "Материал" : "Продукт";
                        var itemName = item.Material?.Name ?? item.Product?.Name ?? "-";

                        ws.Cell(row, 1).Value = itemType;
                        ws.Cell(row, 2).Value = itemName;
                        ws.Cell(row, 3).Value = item.Quantity;
                        ws.Cell(row, 4).Value = item.MeasuringType ?? "шт";

                        for (int i = 1; i <= 4; i++)
                        {
                            ws.Cell(row, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }
                        row++;
                    }

                    row += 2;
                }

                // Total summary
                ws.Cell(row, 1).Value = $"Всего операций переработки: {reprocessings.Count}";
                ws.Cell(row, 1).Style.Font.Bold = true;
            }
            else
            {
                ws.Cell(row, 1).Value = "Нет операций переработки за указанный период";
                ws.Cell(row, 1).Style.Font.Italic = true;
            }

            ws.Columns().AdjustToContents();
        }

        private void CreateSourceMaterialsSummarySheet(XLWorkbook workbook, List<Reprocessing> reprocessings, DateTime startDate, DateTime endDate)
        {
            var ws = workbook.Worksheets.Add("Сводка исходных материалов");
            int row = 1;

            // Title
            ws.Cell(row, 1).Value = "СВОДКА ПО ИСХОДНЫМ МАТЕРИАЛАМ";
            ws.Range(row, 1, row, 4).Merge();
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontSize = 16;
            ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            ws.Cell(row, 1).Value = $"Период: {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}";
            row += 2;

            var allSources = reprocessings.SelectMany(r => r.Sources).ToList();

            if (allSources.Count > 0)
            {
                var materialSummary = allSources
                    .GroupBy(s => new { s.MaterialId, MaterialName = s.Material?.Name ?? "-", s.MeasuringType })
                    .Select(g => new
                    {
                        g.Key.MaterialName,
                        g.Key.MeasuringType,
                        TotalQuantity = g.Sum(s => s.Quantity),
                        OperationsCount = g.Select(s => s.ReprocessingId).Distinct().Count()
                    })
                    .OrderByDescending(x => x.TotalQuantity)
                    .ToList();

                // Header row
                var headers = new[] { "Материал", "Кол-во", "Ед.изм.", "Операций" };
                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cell(row, i + 1).Value = headers[i];
                    ws.Cell(row, i + 1).Style.Font.Bold = true;
                    ws.Cell(row, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    ws.Cell(row, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
                row++;

                foreach (var item in materialSummary)
                {
                    ws.Cell(row, 1).Value = item.MaterialName;
                    ws.Cell(row, 2).Value = item.TotalQuantity;
                    ws.Cell(row, 3).Value = item.MeasuringType ?? "шт";
                    ws.Cell(row, 4).Value = item.OperationsCount;

                    for (int i = 1; i <= 4; i++)
                    {
                        ws.Cell(row, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                    row++;
                }

                row += 2;
                ws.Cell(row, 1).Value = $"Всего использовано видов материалов: {materialSummary.Count}";
                ws.Cell(row, 1).Style.Font.Bold = true;
            }
            else
            {
                ws.Cell(row, 1).Value = "Нет данных за указанный период";
                ws.Cell(row, 1).Style.Font.Italic = true;
            }

            ws.Columns().AdjustToContents();
        }

        private void CreateOutputSummarySheet(XLWorkbook workbook, List<Reprocessing> reprocessings, DateTime startDate, DateTime endDate)
        {
            var ws = workbook.Worksheets.Add("Сводка по выходу");
            int row = 1;

            // Title
            ws.Cell(row, 1).Value = "СВОДКА ПО ВЫХОДУ ПРОДУКЦИИ";
            ws.Range(row, 1, row, 5).Merge();
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontSize = 16;
            ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            ws.Cell(row, 1).Value = $"Период: {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}";
            row += 2;

            var allItems = reprocessings.SelectMany(r => r.Items).ToList();

            if (allItems.Count > 0)
            {
                // Products summary
                var productItems = allItems.Where(i => i.ProductId.HasValue).ToList();
                if (productItems.Count > 0)
                {
                    ws.Cell(row, 1).Value = "Продукция:";
                    ws.Cell(row, 1).Style.Font.Bold = true;
                    row++;

                    var productSummary = productItems
                        .GroupBy(i => new { i.ProductId, ProductName = i.Product?.Name ?? "-", ProductCode = i.Product?.Code, i.MeasuringType })
                        .Select(g => new
                        {
                            g.Key.ProductCode,
                            g.Key.ProductName,
                            g.Key.MeasuringType,
                            TotalQuantity = g.Sum(i => i.Quantity)
                        })
                        .OrderByDescending(x => x.TotalQuantity)
                        .ToList();

                    var headers = new[] { "Артикул", "Продукт", "Кол-во", "Ед.изм." };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        ws.Cell(row, i + 1).Value = headers[i];
                        ws.Cell(row, i + 1).Style.Font.Bold = true;
                        ws.Cell(row, i + 1).Style.Fill.BackgroundColor = XLColor.LightGreen;
                        ws.Cell(row, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                    row++;

                    foreach (var item in productSummary)
                    {
                        ws.Cell(row, 1).Value = item.ProductCode ?? "-";
                        ws.Cell(row, 2).Value = item.ProductName;
                        ws.Cell(row, 3).Value = item.TotalQuantity;
                        ws.Cell(row, 4).Value = item.MeasuringType ?? "шт";

                        for (int i = 1; i <= 4; i++)
                        {
                            ws.Cell(row, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }
                        row++;
                    }
                    row += 2;
                }

                // Materials summary
                var materialItems = allItems.Where(i => i.MaterialId.HasValue).ToList();
                if (materialItems.Count > 0)
                {
                    ws.Cell(row, 1).Value = "Материалы:";
                    ws.Cell(row, 1).Style.Font.Bold = true;
                    row++;

                    var materialSummary = materialItems
                        .GroupBy(i => new { i.MaterialId, MaterialName = i.Material?.Name ?? "-", i.MeasuringType })
                        .Select(g => new
                        {
                            g.Key.MaterialName,
                            g.Key.MeasuringType,
                            TotalQuantity = g.Sum(i => i.Quantity)
                        })
                        .OrderByDescending(x => x.TotalQuantity)
                        .ToList();

                    var headers = new[] { "Материал", "Кол-во", "Ед.изм." };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        ws.Cell(row, i + 1).Value = headers[i];
                        ws.Cell(row, i + 1).Style.Font.Bold = true;
                        ws.Cell(row, i + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
                        ws.Cell(row, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                    row++;

                    foreach (var item in materialSummary)
                    {
                        ws.Cell(row, 1).Value = item.MaterialName;
                        ws.Cell(row, 2).Value = item.TotalQuantity;
                        ws.Cell(row, 3).Value = item.MeasuringType ?? "шт";

                        for (int i = 1; i <= 3; i++)
                        {
                            ws.Cell(row, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }
                        row++;
                    }
                }
            }
            else
            {
                ws.Cell(row, 1).Value = "Нет данных за указанный период";
                ws.Cell(row, 1).Style.Font.Italic = true;
            }

            ws.Columns().AdjustToContents();
        }

        #endregion

        #region Helpers

        private static string GetStatusText(PartRequestStatus status)
        {
            return status switch
            {
                PartRequestStatus.Pending => "Ожидает",
                PartRequestStatus.Approved => "Одобрено",
                PartRequestStatus.Rejected => "Отклонено",
                _ => "-"
            };
        }

        private static XLColor GetStatusColor(PartRequestStatus status)
        {
            return status switch
            {
                PartRequestStatus.Approved => XLColor.LightGreen,
                PartRequestStatus.Rejected => XLColor.LightCoral,
                _ => XLColor.LightYellow
            };
        }

        #endregion
    }
}
