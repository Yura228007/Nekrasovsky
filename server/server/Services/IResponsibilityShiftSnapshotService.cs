using server.Models;

namespace server.Services;

public interface IResponsibilityShiftSnapshotService
{
    /// <summary>
    /// Создать снимок ответственности пользователя на начало смены (вызывать при StartWork).
    /// </summary>
    Task<ResponsibilityShiftSnapshot> CreateSnapshotAsync(int workReportId, int userId);

    /// <summary>
    /// Получить снимок на начало смены по Id отчёта о смене (WorkReportId).
    /// </summary>
    Task<ResponsibilityShiftSnapshot?> GetByWorkReportIdAsync(int workReportId);
}
