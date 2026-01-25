using System.ComponentModel.DataAnnotations;

namespace server.Models
{
    public class ReprocessingCreateRequest
    {
        [Required]
        public int WarehouseId { get; set; }

        [Required]
        public int SourceMaterialId { get; set; }

        [Required]
        public int SourceQuantity { get; set; }

        [Required]
        public List<ReprocessingOutput> Outputs { get; set; } = new();
    }

    public class ReprocessingOutput
    {
        public int? MaterialId { get; set; }
        public int? ProductId { get; set; }
        public int Quantity { get; set; }
        public string? MeasuringType { get; set; }
    }
}
