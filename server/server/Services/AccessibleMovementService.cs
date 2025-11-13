using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Data;

namespace server.Services
{
    public class AccessibleMovementService : IAccessibleMovementService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AccessibleMovementService> _logger;

        public AccessibleMovementService(AppDbContext context, ILogger<AccessibleMovementService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<AccessibleMovement>> GetAllMovementsAsync()
        {
            return await _context.AccessibleMovements.ToListAsync();
        }

        public async Task<AccessibleMovement?> GetMovementAsync(int fromWarehouseId, int toWarehouseId, int materialId)
        {
            return await _context.AccessibleMovements
                .FirstOrDefaultAsync(am => am.FromWarehouseId == fromWarehouseId 
                    && am.ToWarehouseId == toWarehouseId 
                    && am.MaterialId == materialId);
        }

        public async Task<IEnumerable<AccessibleMovement>> GetMovementsFromWarehouseAsync(int warehouseId)
        {
            return await _context.AccessibleMovements
                .Where(am => am.FromWarehouseId == warehouseId)
                .ToListAsync();
        }

        public async Task<IEnumerable<AccessibleMovement>> GetMovementsToWarehouseAsync(int warehouseId)
        {
            return await _context.AccessibleMovements
                .Where(am => am.ToWarehouseId == warehouseId)
                .ToListAsync();
        }

        public async Task<IEnumerable<AccessibleMovement>> GetMovementsByMaterialAsync(int materialId)
        {
            return await _context.AccessibleMovements
                .Where(am => am.MaterialId == materialId)
                .ToListAsync();
        }

        public async Task<AccessibleMovement> CreateMovementAsync(AccessibleMovement movement)
        {
            // Check if warehouses exist
            if (!await _context.Warehouses.AnyAsync(w => w.Id == movement.FromWarehouseId))
            {
                throw new KeyNotFoundException($"FromWarehouse with ID {movement.FromWarehouseId} not found");
            }

            if (!await _context.Warehouses.AnyAsync(w => w.Id == movement.ToWarehouseId))
            {
                throw new KeyNotFoundException($"ToWarehouse with ID {movement.ToWarehouseId} not found");
            }

            // Check if material exists
            if (!await _context.Materials.AnyAsync(m => m.Id == movement.MaterialId))
            {
                throw new KeyNotFoundException($"Material with ID {movement.MaterialId} not found");
            }

            // Check if movement already exists
            if (await _context.AccessibleMovements.AnyAsync(am => 
                am.FromWarehouseId == movement.FromWarehouseId 
                && am.ToWarehouseId == movement.ToWarehouseId 
                && am.MaterialId == movement.MaterialId))
            {
                throw new InvalidOperationException($"This movement already exists");
            }

            _context.AccessibleMovements.Add(movement);
            await _context.SaveChangesAsync();

            _logger.LogInformation("AccessibleMovement created: FromWarehouse {FromId} -> ToWarehouse {ToId}, Material {MaterialId}", 
                movement.FromWarehouseId, movement.ToWarehouseId, movement.MaterialId);
            return movement;
        }

        public async Task<AccessibleMovement> UpdateMovementAsync(int fromWarehouseId, int toWarehouseId, int materialId, AccessibleMovement updatedMovement)
        {
            var movement = await GetMovementAsync(fromWarehouseId, toWarehouseId, materialId);
            if (movement == null)
            {
                throw new KeyNotFoundException($"Movement not found");
            }

            // If keys are being changed, we need to delete old and create new
            if (movement.FromWarehouseId != updatedMovement.FromWarehouseId 
                || movement.ToWarehouseId != updatedMovement.ToWarehouseId 
                || movement.MaterialId != updatedMovement.MaterialId)
            {
                _context.AccessibleMovements.Remove(movement);
                await _context.SaveChangesAsync();
                return await CreateMovementAsync(updatedMovement);
            }

            // No changes needed if keys are the same
            await _context.SaveChangesAsync();
            return movement;
        }

        public async Task<bool> DeleteMovementAsync(int fromWarehouseId, int toWarehouseId, int materialId)
        {
            var movement = await GetMovementAsync(fromWarehouseId, toWarehouseId, materialId);
            if (movement == null)
            {
                return false;
            }

            _context.AccessibleMovements.Remove(movement);
            await _context.SaveChangesAsync();

            _logger.LogInformation("AccessibleMovement deleted: FromWarehouse {FromId} -> ToWarehouse {ToId}, Material {MaterialId}", 
                fromWarehouseId, toWarehouseId, materialId);
            return true;
        }

        public async Task<bool> IsMovementAllowedAsync(int fromWarehouseId, int toWarehouseId, int materialId)
        {
            return await _context.AccessibleMovements
                .AnyAsync(am => am.FromWarehouseId == fromWarehouseId 
                    && am.ToWarehouseId == toWarehouseId 
                    && am.MaterialId == materialId);
        }
    }
}

