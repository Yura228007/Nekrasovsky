using server.Models;

namespace server.Services
{
    public interface IWorkReportService
    {
        Task<IEnumerable<WorkReport>> GetAllWorkReportsAsync();
        Task<WorkReport?> GetWorkReportByIdAsync(int id);
        Task<IEnumerable<WorkReport>> GetWorkReportsByUserAsync(int userId);
        Task<IEnumerable<WorkReport>> GetWorkReportsByDateAsync(DateTime date);
        Task<IEnumerable<WorkReport>> GetWorkReportsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<WorkReport>> GetActiveWorkReportsAsync(int userId);
        Task<IEnumerable<PartRequest>> GetPartRequestsForWorkReportAsync(int reportId);
        Task<WorkReport> CreateWorkReportAsync(WorkReport report);
        Task<WorkReport> UpdateWorkReportAsync(int id, WorkReport updatedReport);
        Task<WorkReport> StartWorkAsync(int userId, DateTime? startTime = null);
        Task<WorkReport> FinishWorkAsync(int reportId, DateTime? finishTime = null, string? note = null);
        Task<bool> DeleteWorkReportAsync(int id);
    }
}

