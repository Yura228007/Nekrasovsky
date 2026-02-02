using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NekrasovskyAPP.Models;

[Table("ProductMovementRequest")]
public class ProductMovementRequest
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int FromUserId { get; set; }

    [Required]
    public int ToUserId { get; set; }

    [Required]
    public int FromWarehouseId { get; set; }

    [Required]
    public int ToWarehouseId { get; set; }

    [Required]
    public int ProductBatchId { get; set; }

    [Required]
    public int Quantity { get; set; } = 0;

    public string? MeasuringType { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public ProductMovementStatus Status { get; set; } = ProductMovementStatus.Pending;

    public string? Note { get; set; }

    // Navigation properties (not mapped, filled from API)
    [NotMapped]
    public User? FromUser { get; set; }

    [NotMapped]
    public User? ToUser { get; set; }

    [NotMapped]
    public Warehouse? FromWarehouse { get; set; }

    [NotMapped]
    public Warehouse? ToWarehouse { get; set; }

    [NotMapped]
    public ProductBatch? ProductBatch { get; set; }

    // Display properties
    [NotMapped]
    public string FromUserDisplay => FromUser != null ? $"{FromUser.Surname} {FromUser.Name}" : $"Пользователь #{FromUserId}";

    [NotMapped]
    public string ToUserDisplay => ToUser != null ? $"{ToUser.Surname} {ToUser.Name}" : $"Пользователь #{ToUserId}";

    [NotMapped]
    public string FromWarehouseDisplay => FromWarehouse?.Name ?? $"Склад #{FromWarehouseId}";

    [NotMapped]
    public string ToWarehouseDisplay => ToWarehouse?.Name ?? $"Склад #{ToWarehouseId}";

    [NotMapped]
    public string StatusDisplay => Status switch
    {
        ProductMovementStatus.Pending => "Ожидает",
        ProductMovementStatus.Approved => "Одобрено",
        ProductMovementStatus.Rejected => "Отклонено",
        _ => "Неизвестно"
    };

    [NotMapped]
    public string QuantityDisplay => $"{Quantity} {MeasuringType ?? "ед."}";

    [NotMapped]
    public string ProductDisplay => ProductBatch?.Product?.Name ?? "Продукт";

    [NotMapped]
    public string? ProductCodeDisplay => ProductBatch?.Product?.Code;

    [NotMapped]
    public string BatchDisplay => ProductBatch?.BatchNumber ?? $"Партия #{ProductBatchId}";
}

public enum ProductMovementStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}
