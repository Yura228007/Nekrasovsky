using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Data;

namespace server.Services
{
    public class PartRequestService : IPartRequestService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PartRequestService> _logger;

        public PartRequestService(AppDbContext context, ILogger<PartRequestService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<PartRequest>> GetAllPartRequestsAsync()
        {
            return await _context.PartRequests.ToListAsync();
        }

        public async Task<PartRequest?> GetPartRequestByIdAsync(int id)
        {
            return await _context.PartRequests.FindAsync(id);
        }

        public async Task<IEnumerable<PartRequest>> GetPartRequestsByStatusAsync(PartRequestStatus status)
        {
            return await _context.PartRequests
                .Where(pr => pr.Status == status)
                .ToListAsync();
        }

        public async Task<IEnumerable<PartRequest>> GetPartRequestsByUserAsync(int userId, bool sent = true)
        {
            if (sent)
            {
                return await _context.PartRequests
                    .Where(pr => pr.FromUserId == userId)
                    .ToListAsync();
            }
            else
            {
                return await _context.PartRequests
                    .Where(pr => pr.ToUserId == userId)
                    .ToListAsync();
            }
        }

        public async Task<IEnumerable<PartRequest>> GetPartRequestsByWarehouseAsync(int warehouseId, bool from = true)
        {
            if (from)
            {
                return await _context.PartRequests
                    .Where(pr => pr.FromWarehouseId == warehouseId)
                    .ToListAsync();
            }
            else
            {
                return await _context.PartRequests
                    .Where(pr => pr.ToWarehouseId == warehouseId)
                    .ToListAsync();
            }
        }

        public async Task<IEnumerable<PartRequest>> GetPartRequestsByMaterialAsync(int materialId)
        {
            return await _context.PartRequests
                .Where(pr => pr.MaterialId == materialId)
                .ToListAsync();
        }

        public async Task<PartRequest> CreatePartRequestAsync(PartRequest request)
        {
            // Validate related entities exist
            if (!await _context.Users.AnyAsync(u => u.Id == request.FromUserId))
                throw new KeyNotFoundException($"FromUser with ID {request.FromUserId} not found");

            if (!await _context.Users.AnyAsync(u => u.Id == request.ToUserId))
                throw new KeyNotFoundException($"ToUser with ID {request.ToUserId} not found");

            if (!await _context.Warehouses.AnyAsync(w => w.Id == request.FromWarehouseId))
                throw new KeyNotFoundException($"FromWarehouse with ID {request.FromWarehouseId} not found");

            if (!await _context.Warehouses.AnyAsync(w => w.Id == request.ToWarehouseId))
                throw new KeyNotFoundException($"ToWarehouse with ID {request.ToWarehouseId} not found");

            if (!await _context.Materials.AnyAsync(m => m.Id == request.MaterialId))
                throw new KeyNotFoundException($"Material with ID {request.MaterialId} not found");

            request.CreatedAt = DateTime.UtcNow;
            request.Status = PartRequestStatus.Pending;

            _context.PartRequests.Add(request);
            await _context.SaveChangesAsync();

            _logger.LogInformation("PartRequest created with ID: {RequestId}", request.Id);
            return request;
        }

        public async Task<PartRequest> UpdatePartRequestAsync(int id, PartRequest updatedRequest)
        {
            var request = await _context.PartRequests.FindAsync(id);
            if (request == null)
            {
                throw new KeyNotFoundException($"PartRequest with ID {id} not found");
            }

            request.Quantity = updatedRequest.Quantity;
            request.MeasuringType = updatedRequest.MeasuringType;
            // Status can be updated via Approve/Reject methods

            await _context.SaveChangesAsync();

            _logger.LogInformation("PartRequest updated with ID: {RequestId}", request.Id);
            return request;
        }

        public async Task<PartRequest> ApprovePartRequestAsync(int id)
        {
            var request = await _context.PartRequests.FindAsync(id);
            if (request == null)
            {
                throw new KeyNotFoundException($"PartRequest with ID {id} not found");
            }

            request.Status = PartRequestStatus.Approved;
            await _context.SaveChangesAsync();

            _logger.LogInformation("PartRequest {RequestId} approved", id);
            return request;
        }

        public async Task<PartRequest> RejectPartRequestAsync(int id, string? reason)
        {
            var request = await _context.PartRequests.FindAsync(id);
            if (request == null)
            {
                throw new KeyNotFoundException($"PartRequest with ID {id} not found");
            }

            request.Status = PartRequestStatus.Rejected;
            await _context.SaveChangesAsync();

            _logger.LogInformation("PartRequest {RequestId} rejected. Reason: {Reason}", id, reason ?? "No reason provided");
            return request;
        }

        public async Task<bool> DeletePartRequestAsync(int id)
        {
            var request = await _context.PartRequests.FindAsync(id);
            if (request == null)
            {
                return false;
            }

            _context.PartRequests.Remove(request);
            await _context.SaveChangesAsync();

            _logger.LogInformation("PartRequest deleted with ID: {RequestId}", id);
            return true;
        }
    }
}

