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
        public string Type { get; set; } = "???";

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("warehouseId")]
        public int? WarehouseId { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; } = true;

        // Computed properties for display
        public string TypeDisplay => Type ?? "—";
        public string DisplayName => string.IsNullOrWhiteSpace(Code) ? Name : $"{Name} ({Code})";
    }
}
