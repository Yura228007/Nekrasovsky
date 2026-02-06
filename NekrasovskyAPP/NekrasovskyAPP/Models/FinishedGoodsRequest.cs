using System.Text.Json.Serialization;

namespace NekrasovskyAPP.Models
{
    public class FinishedGoodsRequest
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("fromUserId")]
        public int FromUserId { get; set; }

        [JsonPropertyName("approvedByUserId")]
        public int? ApprovedByUserId { get; set; }

        [JsonPropertyName("fromWarehouseId")]
        public int FromWarehouseId { get; set; }

        [JsonPropertyName("toWarehouseId")]
        public int ToWarehouseId { get; set; }

        [JsonPropertyName("productId")]
        public int ProductId { get; set; }

        [JsonPropertyName("quantity")]
        public double Quantity { get; set; }

        [JsonPropertyName("measuringUnit")]
        public string? MeasuringUnit { get; set; }

        [JsonPropertyName("requestType")]
        public FinishedGoodsRequestType RequestType { get; set; }

        [JsonPropertyName("status")]
        public FinishedGoodsRequestStatus Status { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("processedAt")]
        public DateTime? ProcessedAt { get; set; }

        [JsonPropertyName("productOutputId")]
        public int? ProductOutputId { get; set; }

        // Navigation properties (populated from server)
        [JsonPropertyName("fromUser")]
        public virtual User? FromUser { get; set; }

        [JsonPropertyName("approvedByUser")]
        public virtual User? ApprovedByUser { get; set; }

        [JsonPropertyName("fromWarehouse")]
        public virtual Warehouse? FromWarehouse { get; set; }

        [JsonPropertyName("toWarehouse")]
        public virtual Warehouse? ToWarehouse { get; set; }

        [JsonPropertyName("product")]
        public virtual Product? Product { get; set; }

        // Computed properties for display
        public string ProductName => Product?.Name ?? $"Продукт #{ProductId}";
        public string FromUserName => FromUser != null ? $"{FromUser.Name} {FromUser.Surname}" : $"Пользователь #{FromUserId}";
        public string FromWarehouseName => FromWarehouse?.Name ?? "—";
        public string ToWarehouseName => ToWarehouse?.Name ?? "—";
        public string RequestTypeDisplay => RequestType == FinishedGoodsRequestType.Normal ? "Нормальная" : "ЭКО";
        public string CreatedAtDisplay => CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
    }

    public enum FinishedGoodsRequestType
    {
        Normal = 0,  // Нормальная продукция
        Eco = 1      // ЭКО продукция
    }

    public enum FinishedGoodsRequestStatus
    {
        Pending = 0,    // Ожидает подтверждения
        Approved = 1,   // Подтвержден (ответственность передана)
        Rejected = 2    // Отклонен (ответственность остается у создателя)
    }
}
