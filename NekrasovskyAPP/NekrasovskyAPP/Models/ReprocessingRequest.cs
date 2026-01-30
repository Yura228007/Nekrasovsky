using System.Collections.Generic;

namespace NekrasovskyAPP.Models
{
    public class ReprocessingCreateRequest
    {
        public int WarehouseId { get; set; }
        public List<ReprocessingSource> Sources { get; set; } = new();
        public List<ReprocessingOutput> Outputs { get; set; } = new();
    }

    public class ReprocessingSource
    {
        public int MaterialId { get; set; }
        public int Quantity { get; set; }
        public string? MeasuringType { get; set; }
    }

    public class ReprocessingOutput
    {
        public int? MaterialId { get; set; }
        public int? ProductId { get; set; }
        public int Quantity { get; set; }
        public string? MeasuringType { get; set; }
    }
}
