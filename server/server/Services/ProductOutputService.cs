using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;

namespace server.Services
{
    public interface IProductOutputService
    {
        Task<IEnumerable<ProductOutput>> GetAllAsync();
        Task<IEnumerable<ProductOutput>> GetByUserAsync(int userId);
        Task<IEnumerable<ProductOutput>> GetByWorkReportAsync(int workReportId);
        Task<ProductOutput?> GetByIdAsync(int id);
        Task<ProductOutput> CreateAsync(ProductOutput productOutput);
        Task<ProductOutput> UpdateAsync(int id, ProductOutput updated);
        Task DeleteAsync(int id);
    }

    public class ProductOutputService : IProductOutputService
    {
        private readonly AppDbContext _context;

        public ProductOutputService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductOutput>> GetAllAsync()
        {
            return await _context.ProductOutputs
                .Include(po => po.User)
                .Include(po => po.Product)
                .Include(po => po.Warehouse)
                .Include(po => po.WorkReport)
                .Include(po => po.Machine)
                .OrderByDescending(po => po.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductOutput>> GetByUserAsync(int userId)
        {
            return await _context.ProductOutputs
                .Include(po => po.User)
                .Include(po => po.Product)
                .Include(po => po.Warehouse)
                .Include(po => po.WorkReport)
                .Include(po => po.Machine)
                .Where(po => po.UserId == userId)
                .OrderByDescending(po => po.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductOutput>> GetByWorkReportAsync(int workReportId)
        {
            return await _context.ProductOutputs
                .Include(po => po.User)
                .Include(po => po.Product)
                .Include(po => po.Warehouse)
                .Include(po => po.WorkReport)
                .Include(po => po.Machine)
                .Where(po => po.WorkReportId == workReportId)
                .OrderByDescending(po => po.CreatedAt)
                .ToListAsync();
        }

        public async Task<ProductOutput?> GetByIdAsync(int id)
        {
            return await _context.ProductOutputs
                .Include(po => po.User)
                .Include(po => po.Product)
                .Include(po => po.Warehouse)
                .Include(po => po.WorkReport)
                .Include(po => po.Machine)
                .FirstOrDefaultAsync(po => po.Id == id);
        }

        public async Task<ProductOutput> CreateAsync(ProductOutput productOutput)
        {
            productOutput.CreatedAt = DateTime.UtcNow;
            _context.ProductOutputs.Add(productOutput);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            return await GetByIdAsync(productOutput.Id) ?? productOutput;
        }

        public async Task<ProductOutput> UpdateAsync(int id, ProductOutput updated)
        {
            var existing = await _context.ProductOutputs.FindAsync(id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"ProductOutput with ID {id} not found");
            }

            existing.ProductId = updated.ProductId;
            existing.WarehouseId = updated.WarehouseId;
            existing.MachineId = updated.MachineId;
            existing.ProducedQuantity = updated.ProducedQuantity;
            existing.DefectQuantity = updated.DefectQuantity;
            existing.EcoQuantity = updated.EcoQuantity;
            existing.MeasuringUnit = updated.MeasuringUnit;
            existing.Note = updated.Note;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id) ?? existing;
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _context.ProductOutputs.FindAsync(id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"ProductOutput with ID {id} not found");
            }

            _context.ProductOutputs.Remove(existing);
            await _context.SaveChangesAsync();
        }
    }
}
