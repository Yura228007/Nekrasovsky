using System.Collections.Generic;

namespace NekrasovskyAPP.Models
{
    public class ReprocessingCreateRequest
    {
        public int WarehouseId { get; set; }
        public int SourceMaterialId { get; set; }
        public int SourceQuantity { get; set; }
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
