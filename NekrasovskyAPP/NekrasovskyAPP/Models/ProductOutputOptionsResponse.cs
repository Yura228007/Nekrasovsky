using System.Text.Json.Serialization;

namespace NekrasovskyAPP.Models
{
    public class ProductOutputOptionsResponse
    {
        [JsonPropertyName("hasSendToSale")]
        public bool HasSendToSale { get; set; }

        [JsonPropertyName("options")]
        public List<ProductOutputOption> Options { get; set; } = new();
    }

    public class ProductOutputOption
    {
        [JsonPropertyName("productId")]
        public int ProductId { get; set; }

        [JsonPropertyName("productName")]
        public string ProductName { get; set; } = string.Empty;

        [JsonPropertyName("productBatchId")]
        public int? ProductBatchId { get; set; }

        [JsonPropertyName("batchNumber")]
        public string? BatchNumber { get; set; }

        [JsonPropertyName("warehouseId")]
        public int? WarehouseId { get; set; }

        [JsonPropertyName("warehouseName")]
        public string? WarehouseName { get; set; }

        [JsonPropertyName("targetWarehouseId")]
        public int? TargetWarehouseId { get; set; }

        [JsonPropertyName("targetWarehouseName")]
        public string? TargetWarehouseName { get; set; }

        [JsonPropertyName("maxQuantity")]
        public int? MaxQuantity { get; set; }

        [JsonPropertyName("measuringUnit")]
        public string? MeasuringUnit { get; set; }
    }
}
