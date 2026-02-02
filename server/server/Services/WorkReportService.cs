using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Data;

namespace server.Services
{
    public class WorkReportService : IWorkReportService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<WorkReportService> _logger;
        private readonly IResponsibilityShiftSnapshotService _snapshotService;

        public WorkReportService(AppDbContext context, ILogger<WorkReportService> logger,
            IResponsibilityShiftSnapshotService snapshotService)
        {
            _context = context;
            _logger = logger;
            _snapshotService = snapshotService;
        }

        public async Task<IEnumerable<WorkReport>> GetAllWorkReportsAsync()
        {
            return await _context.WorkReports
                .Include(wr => wr.User)
                .OrderByDescending(wr => wr.Date)
                .ThenByDescending(wr => wr.StartWork)
                .ToListAsync();
        }

        public async Task<WorkReport?> GetWorkReportByIdAsync(int id)
        {
            return await _context.WorkReports.FindAsync(id);
        }

        public async Task<IEnumerable<WorkReport>> GetWorkReportsByUserAsync(int userId)
        {
            return await _context.WorkReports
                .Include(wr => wr.User)
                .Where(wr => wr.UserId == userId)
                .OrderByDescending(wr => wr.Date)
                .ThenByDescending(wr => wr.StartWork)
                .ToListAsync();
        }

        public async Task<IEnumerable<WorkReport>> GetWorkReportsByDateAsync(DateTime date)
        {
            return await _context.WorkReports
                .Where(wr => wr.Date.Date == date.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<WorkReport>> GetWorkReportsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.WorkReports
                .Where(wr => wr.Date >= startDate && wr.Date <= endDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<WorkReport>> GetActiveWorkReportsAsync(int userId)
        {
            return await _context.WorkReports
                .Where(wr => wr.UserId == userId && wr.FinishWork == null)
                .ToListAsync();
        }

        public async Task<IEnumerable<PartRequest>> GetPartRequestsForWorkReportAsync(int reportId)
        {
            var report = await _context.WorkReports.FindAsync(reportId);
            if (report == null)
            {
                throw new KeyNotFoundException($"WorkReport with ID {reportId} not found");
            }

            // Получаем все запросы пользователя за дату отчета
            var startOfDay = report.Date.Date;
            var endOfDay = startOfDay.AddDays(1);

            return await _context.PartRequests
                .Where(pr => pr.FromUserId == report.UserId && 
                            pr.CreatedAt >= startOfDay && 
                            pr.CreatedAt < endOfDay)
                .OrderBy(pr => pr.CreatedAt)
                .ToListAsync();
        }

        public async Task<WorkReport> CreateWorkReportAsync(WorkReport report)
        {
            // Check if user exists
            if (!await _context.Users.AnyAsync(u => u.Id == report.UserId))
            {
                throw new KeyNotFoundException($"User with ID {report.UserId} not found");
            }

            _context.WorkReports.Add(report);
            await _context.SaveChangesAsync();

            _logger.LogInformation("WorkReport created with ID: {ReportId}, User: {UserId}, Date: {Date}", 
                report.Id, report.UserId, report.Date);
            return report;
        }

        public async Task<WorkReport> UpdateWorkReportAsync(int id, WorkReport updatedReport)
        {
            var report = await _context.WorkReports.FindAsync(id);
            if (report == null)
            {
                throw new KeyNotFoundException($"WorkReport with ID {id} not found");
            }

            report.Date = updatedReport.Date;
            report.StartWork = updatedReport.StartWork;
            report.FinishWork = updatedReport.FinishWork;
            report.Note = updatedReport.Note;

            await _context.SaveChangesAsync();

            _logger.LogInformation("WorkReport updated with ID: {ReportId}", report.Id);
            return report;
        }

        public async Task<WorkReport> StartWorkAsync(int userId, DateTime? startTime = null)
        {
            // Check if user exists
            if (!await _context.Users.AnyAsync(u => u.Id == userId))
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            var hasActiveShift = await _context.WorkReports
                .AnyAsync(wr => wr.UserId == userId && wr.FinishWork == null);

            if (hasActiveShift)
            {
                throw new InvalidOperationException("Work already started for this user");
            }

            var normalizedStart = startTime ?? DateTime.UtcNow;
            if (normalizedStart.Kind == DateTimeKind.Unspecified)
            {
                normalizedStart = DateTime.SpecifyKind(normalizedStart, DateTimeKind.Utc);
            }
            else if (normalizedStart.Kind == DateTimeKind.Local)
            {
                normalizedStart = normalizedStart.ToUniversalTime();
            }

            var report = new WorkReport
            {
                UserId = userId,
                // PostgreSQL date expects Unspecified kind
                Date = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Unspecified),
                StartWork = normalizedStart,
                FinishWork = null
            };

            _context.WorkReports.Add(report);
            await _context.SaveChangesAsync();

            await _snapshotService.CreateSnapshotAsync(report.Id, userId);

            _logger.LogInformation("Work started for User {UserId}, Report ID: {ReportId}", userId, report.Id);
            return report;
        }

        public async Task<WorkReport> FinishWorkAsync(int reportId, DateTime? finishTime = null, string? note = null)
        {
            var report = await _context.WorkReports.FindAsync(reportId);
            if (report == null)
            {
                throw new KeyNotFoundException($"WorkReport with ID {reportId} not found");
            }

            if (report.FinishWork != null)
            {
                throw new InvalidOperationException($"Work already finished for this report");
            }

            report.FinishWork = finishTime ?? DateTime.UtcNow;
            report.Note = note;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Work finished for Report {ReportId}, User {UserId}, Note: {Note}", reportId, report.UserId, note ?? "(empty)");
            return report;
        }

        public async Task<bool> DeleteWorkReportAsync(int id)
        {
            var report = await _context.WorkReports.FindAsync(id);
            if (report == null)
            {
                return false;
            }

            _context.WorkReports.Remove(report);
            await _context.SaveChangesAsync();

            _logger.LogInformation("WorkReport deleted with ID: {ReportId}", id);
            return true;
        }
    }
}

