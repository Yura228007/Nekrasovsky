using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaterialsController : ControllerBase
    {
        private readonly IMaterialService _materialService;

        public MaterialsController(IMaterialService materialService)
        {
            _materialService = materialService;
        }

        // GET: api/materials
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Material>>> GetAll()
        {
            var materials = await _materialService.GetAllMaterialsAsync();
            return Ok(materials);
        }

        // GET: api/materials/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Material>> GetById(int id)
        {
            var material = await _materialService.GetMaterialByIdAsync(id);
            if (material == null)
                return NotFound($"Material with ID {id} not found");

            return Ok(material);
        }

        // GET: api/materials/search?name=&code=
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Material>>> Search([FromQuery] string? name, [FromQuery] string? code)
        {
            var materials = await _materialService.SearchMaterialsAsync(name, code);
            return Ok(materials);
        }

        // GET: api/materials/unit/{unit}
        [HttpGet("unit/{unit}")]
        public async Task<ActionResult<IEnumerable<Material>>> GetByMeasuringUnit(string unit)
        {
            var materials = await _materialService.GetMaterialsByMeasuringUnitAsync(unit);
            return Ok(materials);
        }

        // POST: api/materials/add
        [HttpPost("add")]
        public async Task<IActionResult> AddMaterial([FromBody] Material material)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdMaterial = await _materialService.CreateMaterialAsync(material);
            return Ok(new { message = "Material created successfully", material = createdMaterial });
        }

        // POST: api/materials/edit/{id}
        [HttpPost("edit/{id}")]
        public async Task<IActionResult> EditMaterial(int id, [FromBody] Material updated)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var material = await _materialService.UpdateMaterialAsync(id, updated);
                return Ok(new { message = "Material updated successfully", material });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/materials/delete/{id}
        [HttpPost("delete/{id}")]
        public async Task<IActionResult> DeleteMaterial(int id)
        {
            var deleted = await _materialService.DeleteMaterialAsync(id);
            if (!deleted)
                return NotFound($"Material with ID {id} not found");

            return Ok(new { message = "Material deleted successfully" });
        }

        // GET: api/materials/{id}/movements
        [HttpGet("{id}/movements")]
        public async Task<ActionResult<IEnumerable<AccessibleMovement>>> GetMovements(int id)
        {
            var movements = await _materialService.GetMaterialMovementsAsync(id);
            return Ok(movements);
        }

        // GET: api/materials/{id}/usage
        [HttpGet("{id}/usage")]
        public async Task<ActionResult<object>> GetUsage(int id)
        {
            var usage = await _materialService.GetMaterialUsageAsync(id);
            return Ok(usage);
        }
    }
}

