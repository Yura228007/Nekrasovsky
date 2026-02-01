using server.Models;

namespace server.Services
{
    public interface IShiftReportService
    {
        Task<ShiftReport> GenerateReportAsync(int workReportId);
        Task<ShiftReport?> GetReportByIdAsync(int id);
        Task<ShiftReport?> GetReportByWorkReportIdAsync(int workReportId);
        Task<IEnumerable<ShiftReport>> GetReportsByUserAsync(int userId);
        Task<IEnumerable<ShiftReport>> GetAllReportsAsync();
        Task<IEnumerable<ShiftReport>> GetReportsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<byte[]> GetReportFileAsync(int reportId);
        Task<bool> DeleteReportAsync(int id);
        Task<bool> CanUserDownloadReportAsync(int requestingUserId, int reportId);
    }
}
