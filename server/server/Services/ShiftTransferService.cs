using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Data;

namespace server.Services
{
    public class ShiftTransferService : IShiftTransferService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ShiftTransferService> _logger;
        private readonly IResponsibilityService _responsibilityService;
        private readonly IResponsibilityFillingService _responsibilityFillingService;
        private readonly IWorkReportService _workReportService;

        public ShiftTransferService(
            AppDbContext context,
            ILogger<ShiftTransferService> logger,
            IResponsibilityService responsibilityService,
            IResponsibilityFillingService responsibilityFillingService,
            IWorkReportService workReportService)
        {
            _context = context;
            _logger = logger;
            _responsibilityService = responsibilityService;
            _responsibilityFillingService = responsibilityFillingService;
            _workReportService = workReportService;
        }

        public async Task<IEnumerable<ShiftTransfer>> GetAllShiftTransfersAsync()
        {
            return await _context.ShiftTransfers.ToListAsync();
        }

        public async Task<ShiftTransfer?> GetShiftTransferByIdAsync(int id)
        {
            return await _context.ShiftTransfers.FindAsync(id);
        }

        public async Task<IEnumerable<ShiftTransfer>> GetShiftTransfersByUserAsync(int userId, bool sent = true)
        {
            if (sent)
            {
                return await _context.ShiftTransfers
                    .Where(st => st.FromUserId == userId)
                    .ToListAsync();
            }
            else
            {
                return await _context.ShiftTransfers
                    .Where(st => st.ToUserId == userId)
                    .ToListAsync();
            }
        }

        public async Task<IEnumerable<ShiftTransfer>> GetShiftTransfersByDateAsync(DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            return await _context.ShiftTransfers
                .Where(st => st.TransferDate >= startOfDay && st.TransferDate < endOfDay)
                .ToListAsync();
        }

        public async Task<IEnumerable<ShiftTransfer>> GetPendingShiftTransfersAsync(int userId)
        {
            return await _context.ShiftTransfers
                .Where(st => st.ToUserId == userId && !st.IsConfirmed)
                .ToListAsync();
        }

        public async Task<ShiftTransfer> CreateShiftTransferAsync(ShiftTransfer transfer)
        {
            // Check if users exist
            if (!await _context.Users.AnyAsync(u => u.Id == transfer.FromUserId))
            {
                throw new KeyNotFoundException($"FromUser with ID {transfer.FromUserId} not found");
            }

            if (!await _context.Users.AnyAsync(u => u.Id == transfer.ToUserId))
            {
                throw new KeyNotFoundException($"ToUser with ID {transfer.ToUserId} not found");
            }

            // Проверка остатков перед передачей смены
            // Получаем все склады и проверяем их заполненность
            var warehouses = await _context.Warehouses.ToListAsync();
            var missingStock = new List<string>();

            foreach (var warehouse in warehouses)
            {
                var fillings = await _context.FillingWarehouses
                    .Where(fw => fw.WarehouseId == warehouse.Id)
                    .ToListAsync();

                var materialFillings = fillings.Where(f => f.MaterialId.HasValue).ToList();

                if (materialFillings.Count == 0)
                {
                    missingStock.Add($"Склад '{warehouse.Name}' (ID: {warehouse.Id}) не имеет информации об остатках");
                }
                else
                {
                    // Проверяем наличие материалов с нулевым количеством (можно расширить логику)
                    var zeroQuantity = materialFillings.Where(f => f.Quantity <= 0).ToList();
                    if (zeroQuantity.Any())
                    {
                        var materialIds = zeroQuantity.Select(f => f.MaterialId!.Value).ToList();
                        var materials = await _context.Materials
                            .Where(m => materialIds.Contains(m.Id))
                            .ToListAsync();
                        
                        var materialNames = string.Join(", ", materials.Select(m => m.Name));
                        missingStock.Add($"На складе '{warehouse.Name}' закончились материалы: {materialNames}");
                    }
                }
            }

            if (missingStock.Any())
            {
                _logger.LogWarning("ShiftTransfer created with missing stock information: {MissingStock}",
                    string.Join("; ", missingStock));
            }

            transfer.IsConfirmed = false;

            _context.ShiftTransfers.Add(transfer);
            await _context.SaveChangesAsync();

            _logger.LogInformation("ShiftTransfer created with ID: {TransferId}, FromUser: {FromUserId}, ToUser: {ToUserId}", 
                transfer.Id, transfer.FromUserId, transfer.ToUserId);
            return transfer;
        }

        public async Task<ShiftTransfer> UpdateShiftTransferAsync(int id, ShiftTransfer updatedTransfer)
        {
            var transfer = await _context.ShiftTransfers.FindAsync(id);
            if (transfer == null)
            {
                throw new KeyNotFoundException($"ShiftTransfer with ID {id} not found");
            }

            transfer.TransferDate = updatedTransfer.TransferDate;
            // IsConfirmed should be updated via ConfirmShiftTransferAsync

            await _context.SaveChangesAsync();

            _logger.LogInformation("ShiftTransfer updated with ID: {TransferId}", transfer.Id);
            return transfer;
        }

        public async Task<ShiftTransfer> ConfirmShiftTransferAsync(int id)
        {
            var transfer = await _context.ShiftTransfers.FindAsync(id);
            if (transfer == null)
            {
                throw new KeyNotFoundException($"ShiftTransfer with ID {id} not found");
            }

            transfer.IsConfirmed = true;
            await _context.SaveChangesAsync();

            // Передаём все остатки по складам (ResponsibilityFilling) — материалы, продукты, партии
            var fillingTransferred = await _responsibilityFillingService.TransferAllResponsibilityFillingAsync(transfer.FromUserId, transfer.ToUserId);
            _logger.LogInformation("ShiftTransfer {TransferId}: transferred {Count} responsibility filling groups from {FromUserId} to {ToUserId}", id, fillingTransferred, transfer.FromUserId, transfer.ToUserId);

            // Передаём старую модель Responsibility (без склада) для совместимости
            await _responsibilityService.TransferAllAsync(transfer.FromUserId, transfer.ToUserId);

            var activeReport = await _context.WorkReports
                .FirstOrDefaultAsync(wr => wr.UserId == transfer.FromUserId && wr.FinishWork == null);

            if (activeReport != null)
            {
                activeReport.FinishWork = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            // У принимающего смену автоматически начинается новая смена (WorkReport)
            var receiverHasActiveShift = await _context.WorkReports
                .AnyAsync(wr => wr.UserId == transfer.ToUserId && wr.FinishWork == null);
            if (!receiverHasActiveShift)
            {
                var newReport = await _workReportService.StartWorkAsync(transfer.ToUserId);
                _logger.LogInformation("ShiftTransfer {TransferId}: started new shift for receiver ToUserId={ToUserId}, WorkReportId={WorkReportId}", id, transfer.ToUserId, newReport.Id);
            }
            else
            {
                _logger.LogWarning("ShiftTransfer {TransferId}: receiver ToUserId={ToUserId} already has active shift, new shift not started", id, transfer.ToUserId);
            }

            _logger.LogInformation("ShiftTransfer {TransferId} confirmed", id);
            return transfer;
        }

        public async Task<bool> DeleteShiftTransferAsync(int id)
        {
            var transfer = await _context.ShiftTransfers.FindAsync(id);
            if (transfer == null)
            {
                return false;
            }

            _context.ShiftTransfers.Remove(transfer);
            await _context.SaveChangesAsync();

            _logger.LogInformation("ShiftTransfer deleted with ID: {TransferId}", id);
            return true;
        }
    }
}

