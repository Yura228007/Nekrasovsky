using server.Models;

namespace server.Services
{
    public interface IShiftTransferService
    {
        Task<IEnumerable<ShiftTransfer>> GetAllShiftTransfersAsync();
        Task<ShiftTransfer?> GetShiftTransferByIdAsync(int id);
        Task<IEnumerable<ShiftTransfer>> GetShiftTransfersByUserAsync(int userId, bool sent = true);
        Task<IEnumerable<ShiftTransfer>> GetShiftTransfersByDateAsync(DateTime date);
        Task<IEnumerable<ShiftTransfer>> GetPendingShiftTransfersAsync(int userId);
        Task<ShiftTransfer> CreateShiftTransferAsync(ShiftTransfer transfer);
        Task<ShiftTransfer> UpdateShiftTransferAsync(int id, ShiftTransfer updatedTransfer);
        Task<ShiftTransfer> ConfirmShiftTransferAsync(int id);
        Task<bool> DeleteShiftTransferAsync(int id);
    }
}

