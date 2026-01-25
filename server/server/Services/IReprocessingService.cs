using server.Models;

namespace server.Services
{
    public interface IReprocessingService
    {
        Task<Reprocessing> CreateReprocessingAsync(ReprocessingCreateRequest request, int userId);
    }
}
