using System.Text.Json.Serialization;

namespace NekrasovskyAPP.Models
{
    public class ProductOutput
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("userId")]
        public int UserId { get; set; }

        [JsonPropertyName("workReportId")]
        public int? WorkReportId { get; set; }

        [JsonPropertyName("productId")]
        public int ProductId { get; set; }

        [JsonPropertyName("warehouseId")]
        public int? WarehouseId { get; set; }

        [JsonPropertyName("productBatchId")]
        public int? ProductBatchId { get; set; }

        [JsonPropertyName("producedQuantity")]
        public int ProducedQuantity { get; set; }

        [JsonPropertyName("defectQuantity")]
        public int DefectQuantity { get; set; }

        [JsonPropertyName("ecoQuantity")]
        public int EcoQuantity { get; set; }

        [JsonPropertyName("rewindQuantity")]
        public int RewindQuantity { get; set; }

        [JsonPropertyName("normalWarehouseId")]
        public int? NormalWarehouseId { get; set; }

        [JsonPropertyName("ecoWarehouseId")]
        public int? EcoWarehouseId { get; set; }

        [JsonPropertyName("defectWarehouseId")]
        public int? DefectWarehouseId { get; set; }

        [JsonPropertyName("rewindWarehouseId")]
        public int? RewindWarehouseId { get; set; }

        [JsonPropertyName("rewindToUserId")]
        public int? RewindToUserId { get; set; }

        [JsonPropertyName("measuringUnit")]
        public string? MeasuringUnit { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("user")]
        public virtual User? User { get; set; }

        [JsonPropertyName("product")]
        public virtual Product? Product { get; set; }

        [JsonPropertyName("warehouse")]
        public virtual Warehouse? Warehouse { get; set; }

        // Computed properties for display
        public string ProductName => Product?.Name ?? $"Продукт #{ProductId}";
        public string UserName => User != null ? $"{User.Name} {User.Surname}" : $"Пользователь #{UserId}";
        public string WarehouseName => Warehouse?.Name ?? "—";
        public string CreatedAtDisplay => CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        public double TotalQuantity => ProducedQuantity + DefectQuantity + EcoQuantity + RewindQuantity;
    }
}
