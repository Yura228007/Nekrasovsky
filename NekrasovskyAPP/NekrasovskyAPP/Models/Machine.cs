using System.Text.Json.Serialization;

namespace NekrasovskyAPP.Models
{
    public class Machine
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; } = true;

        [JsonPropertyName("warehouseId")]
        public int? WarehouseId { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("warehouse")]
        public virtual Warehouse? Warehouse { get; set; }

        // Computed properties for display
        public string DisplayName => string.IsNullOrEmpty(Code) ? Name : $"{Code} - {Name}";
        public string WarehouseName => Warehouse?.Name ?? "—";
        public string TypeDisplay => Type ?? "—";
    }
}
