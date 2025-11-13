using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Data;

namespace server.Services
{
    public class MaterialService : IMaterialService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MaterialService> _logger;

        public MaterialService(AppDbContext context, ILogger<MaterialService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Material>> GetAllMaterialsAsync()
        {
            return await _context.Materials.ToListAsync();
        }

        public async Task<Material?> GetMaterialByIdAsync(int id)
        {
            return await _context.Materials.FindAsync(id);
        }

        public async Task<IEnumerable<Material>> SearchMaterialsAsync(string? name, string? code)
        {
            var query = _context.Materials.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(m => EF.Functions.ILike(m.Name, $"%{name}%"));

            if (!string.IsNullOrWhiteSpace(code))
                query = query.Where(m => m.Code != null && EF.Functions.ILike(m.Code, $"%{code}%"));

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Material>> GetMaterialsByMeasuringUnitAsync(string unit)
        {
            return await _context.Materials
                .Where(m => m.MeasuringUnit == unit)
                .ToListAsync();
        }

        public async Task<Material> CreateMaterialAsync(Material material)
        {
            _context.Materials.Add(material);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Material created with ID: {MaterialId}, Name: {Name}", material.Id, material.Name);
            return material;
        }

        public async Task<Material> UpdateMaterialAsync(int id, Material updatedMaterial)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null)
            {
                throw new KeyNotFoundException($"Material with ID {id} not found");
            }

            material.Name = updatedMaterial.Name;
            material.Description = updatedMaterial.Description;
            material.Code = updatedMaterial.Code;
            material.MeasuringUnit = updatedMaterial.MeasuringUnit;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Material updated with ID: {MaterialId}", material.Id);
            return material;
        }

        public async Task<bool> DeleteMaterialAsync(int id)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null)
            {
                return false;
            }

            _context.Materials.Remove(material);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Material deleted with ID: {MaterialId}", id);
            return true;
        }

        public async Task<IEnumerable<AccessibleMovement>> GetMaterialMovementsAsync(int materialId)
        {
            return await _context.AccessibleMovements
                .Where(am => am.MaterialId == materialId)
                .ToListAsync();
        }

        public async Task<object> GetMaterialUsageAsync(int materialId)
        {
            var recipes = await _context.Recipes
                .Where(r => r.MaterialId == materialId)
                .ToListAsync();

            var fillings = await _context.FillingWarehouses
                .Where(fw => fw.MaterialId == materialId)
                .ToListAsync();

            var movements = await _context.AccessibleMovements
                .Where(am => am.MaterialId == materialId)
                .ToListAsync();

            return new
            {
                UsedInRecipes = recipes.Count,
                UsedInWarehouses = fillings.Count,
                UsedInMovements = movements.Count,
                Recipes = recipes,
                Fillings = fillings,
                Movements = movements
            };
        }
    }
}

