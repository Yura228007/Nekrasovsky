using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Data;

namespace server.Services
{
    public class MaterialService : IMaterialService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MaterialService> _logger;
        private readonly IResponsibilityService _responsibilityService;

        public MaterialService(AppDbContext context, ILogger<MaterialService> logger, IResponsibilityService responsibilityService)
        {
            _context = context;
            _logger = logger;
            _responsibilityService = responsibilityService;
        }

        public async Task<IEnumerable<Material>> GetAllMaterialsAsync()
        {
            return await _context.Materials.ToListAsync();
        }

        public async Task<Material?> GetMaterialByIdAsync(int id)
        {
            return await _context.Materials.FindAsync(id);
        }

        public async Task<IEnumerable<Material>> SearchMaterialsAsync(string? name, string? code, bool? isActive = null, string? sortBy = null)
        {
            var query = _context.Materials.AsQueryable();

            // Если указаны и name, и code - используем OR логику
            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(code))
            {
                query = query.Where(m =>
                    EF.Functions.ILike(m.Name, $"%{name}%") ||
                    (m.Code != null && EF.Functions.ILike(m.Code, $"%{code}%")));
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(name))
                    query = query.Where(m => EF.Functions.ILike(m.Name, $"%{name}%"));

                if (!string.IsNullOrWhiteSpace(code))
                    query = query.Where(m => m.Code != null && EF.Functions.ILike(m.Code, $"%{code}%"));
            }

            if (isActive.HasValue)
                query = query.Where(m => m.IsActive == isActive.Value);

            // Сортировка
            query = sortBy?.ToLower() switch
            {
                "name" => query.OrderBy(m => m.Name),
                "name_desc" => query.OrderByDescending(m => m.Name),
                "code" => query.OrderBy(m => m.Code),
                "code_desc" => query.OrderByDescending(m => m.Code),
                _ => query.OrderBy(m => m.Name) // По умолчанию
            };

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Material>> GetMaterialsByMeasuringUnitAsync(string unit)
        {
            return await _context.Materials
                .Where(m => m.MeasuringUnit == unit)
                .ToListAsync();
        }

        public async Task<Material> CreateMaterialAsync(Material material, int userId, int? quantity = null, string? measuringUnit = null)
        {
            // Проверка уникальности артикула
            if (!string.IsNullOrWhiteSpace(material.Code))
            {
                var existingMaterial = await _context.Materials
                    .FirstOrDefaultAsync(m => m.Code == material.Code);
                if (existingMaterial != null)
                {
                    throw new InvalidOperationException($"Материал с артикулом '{material.Code}' уже существует");
                }
            }

            _context.Materials.Add(material);
            await _context.SaveChangesAsync();

            // Если единица измерения не указана, берем из материала
            if (string.IsNullOrWhiteSpace(measuringUnit))
            {
                measuringUnit = material.MeasuringUnit;
            }

            await _responsibilityService.AssignMaterialAsync(material.Id, userId, quantity, measuringUnit);

            _logger.LogInformation("Material created with ID: {MaterialId}, Name: {Name}, Quantity: {Quantity}", material.Id, material.Name, quantity);
            return material;
        }

        public async Task<Material> UpdateMaterialAsync(int id, Material updatedMaterial)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null)
            {
                throw new KeyNotFoundException($"Material with ID {id} not found");
            }

            // Проверка уникальности артикула (исключая текущий материал)
            if (!string.IsNullOrWhiteSpace(updatedMaterial.Code))
            {
                var existingMaterial = await _context.Materials
                    .FirstOrDefaultAsync(m => m.Code == updatedMaterial.Code && m.Id != id);
                if (existingMaterial != null)
                {
                    throw new InvalidOperationException($"Материал с артикулом '{updatedMaterial.Code}' уже существует");
                }
            }

            material.Name = updatedMaterial.Name;
            material.Description = updatedMaterial.Description;
            material.Code = updatedMaterial.Code;
            material.MeasuringUnit = updatedMaterial.MeasuringUnit;
            material.IsActive = updatedMaterial.IsActive;

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

