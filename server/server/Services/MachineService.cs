using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;

namespace server.Services
{
    public interface IMachineService
    {
        Task<IEnumerable<Machine>> GetAllAsync();
        Task<IEnumerable<Machine>> GetActiveAsync();
        Task<IEnumerable<Machine>> GetByWarehouseAsync(int warehouseId);
        Task<Machine?> GetByIdAsync(int id);
        Task<Machine> CreateAsync(Machine machine);
        Task<Machine> UpdateAsync(int id, Machine updated);
        Task DeleteAsync(int id);
    }

    public class MachineService : IMachineService
    {
        private readonly AppDbContext _context;

        public MachineService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Machine>> GetAllAsync()
        {
            return await _context.Machines
                .Include(m => m.Warehouse)
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Machine>> GetActiveAsync()
        {
            return await _context.Machines
                .Include(m => m.Warehouse)
                .Where(m => m.IsActive)
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Machine>> GetByWarehouseAsync(int warehouseId)
        {
            return await _context.Machines
                .Include(m => m.Warehouse)
                .Where(m => m.WarehouseId == warehouseId && m.IsActive)
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<Machine?> GetByIdAsync(int id)
        {
            return await _context.Machines
                .Include(m => m.Warehouse)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Machine> CreateAsync(Machine machine)
        {
            // Check for duplicate code
            if (!string.IsNullOrWhiteSpace(machine.Code))
            {
                var exists = await _context.Machines.AnyAsync(m => m.Code == machine.Code);
                if (exists)
                {
                    throw new InvalidOperationException($"Станок с кодом '{machine.Code}' уже существует");
                }
            }

            machine.CreatedAt = DateTime.UtcNow;
            _context.Machines.Add(machine);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(machine.Id) ?? machine;
        }

        public async Task<Machine> UpdateAsync(int id, Machine updated)
        {
            var existing = await _context.Machines.FindAsync(id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Machine with ID {id} not found");
            }

            // Check for duplicate code (excluding self)
            if (!string.IsNullOrWhiteSpace(updated.Code))
            {
                var duplicateExists = await _context.Machines.AnyAsync(m => m.Code == updated.Code && m.Id != id);
                if (duplicateExists)
                {
                    throw new InvalidOperationException($"Станок с кодом '{updated.Code}' уже существует");
                }
            }

            existing.Name = updated.Name;
            existing.Code = updated.Code;
            existing.Type = updated.Type;
            existing.Description = updated.Description;
            existing.IsActive = updated.IsActive;
            existing.WarehouseId = updated.WarehouseId;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id) ?? existing;
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _context.Machines.FindAsync(id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Machine with ID {id} not found");
            }

            // Check if machine is used in any product outputs
            var hasOutputs = await _context.ProductOutputs.AnyAsync(po => po.MachineId == id);
            if (hasOutputs)
            {
                throw new InvalidOperationException("Невозможно удалить станок, так как он используется в записях выпуска продукции");
            }

            _context.Machines.Remove(existing);
            await _context.SaveChangesAsync();
        }
    }
}
