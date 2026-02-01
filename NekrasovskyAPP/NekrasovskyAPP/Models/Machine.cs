using System.Text.Json.Serialization;

namespace NekrasovskyAPP.Models
{
    public class Machine
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = "???";

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; } = true;

        // Computed properties for display
        public string TypeDisplay => Type ?? "—";
    }
}
