using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;

namespace server.Services
{
    public interface IMachineService
    {
        Task<IEnumerable<Machine>> GetAllAsync();
        Task<IEnumerable<Machine>> GetActiveAsync();
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
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Machine>> GetActiveAsync()
        {
            return await _context.Machines
                .Where(m => m.IsActive)
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<Machine?> GetByIdAsync(int id)
        {
            return await _context.Machines.FindAsync(id);
        }

        public async Task<Machine> CreateAsync(Machine machine)
        {
            // Check for duplicate name
            var exists = await _context.Machines.AnyAsync(m => m.Name == machine.Name);
            if (exists)
            {
                throw new InvalidOperationException($"Станок с названием '{machine.Name}' уже существует");
            }

            _context.Machines.Add(machine);
            await _context.SaveChangesAsync();

            return machine;
        }

        public async Task<Machine> UpdateAsync(int id, Machine updated)
        {
            var existing = await _context.Machines.FindAsync(id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Machine with ID {id} not found");
            }

            // Check for duplicate name (excluding self)
            var duplicateExists = await _context.Machines.AnyAsync(m => m.Name == updated.Name && m.Id != id);
            if (duplicateExists)
            {
                throw new InvalidOperationException($"Станок с названием '{updated.Name}' уже существует");
            }

            existing.Name = updated.Name;
            existing.Type = updated.Type;
            existing.IsActive = updated.IsActive;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _context.Machines.FindAsync(id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Machine with ID {id} not found");
            }

            // Machine is no longer used in ProductOutput

            _context.Machines.Remove(existing);
            await _context.SaveChangesAsync();
        }
    }
}
