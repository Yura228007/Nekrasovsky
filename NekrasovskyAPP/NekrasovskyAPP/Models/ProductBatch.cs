using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NekrasovskyAPP.Models;

[Table("ProductBatch")]
public class ProductBatch
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    public int WarehouseId { get; set; }

    [Required]
    public int Quantity { get; set; } = 0;

    public string? MeasuringUnit { get; set; }

    public int? CreatedByUserId { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? BatchNumber { get; set; }

    public int? ProductOutputId { get; set; }

    public string? Note { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    // Navigation properties (not mapped, filled from API)
    [NotMapped]
    public Product? Product { get; set; }

    [NotMapped]
    public Warehouse? Warehouse { get; set; }

    [NotMapped]
    public User? CreatedByUser { get; set; }

    // Display properties
    [NotMapped]
    public string DisplayName => $"{Product?.Name ?? "Продукт"} - {BatchNumber ?? $"Партия #{Id}"}";

    [NotMapped]
    public string CreatedByDisplay => CreatedByUser != null ? $"{CreatedByUser.Surname} {CreatedByUser.Name}" : (CreatedByUserId.HasValue ? $"Пользователь #{CreatedByUserId}" : "—");

    [NotMapped]
    public string WarehouseDisplay => Warehouse?.Name ?? $"Склад #{WarehouseId}";

    [NotMapped]
    public string QuantityDisplay => $"{Quantity} {MeasuringUnit ?? "ед."}";

    [NotMapped]
    public string ProductCode => Product?.Code ?? "";
}
