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
    public double Quantity { get; set; } = 0;

    public string? MeasuringUnit { get; set; }

    public int? CreatedByUserId { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? BatchNumber { get; set; }

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

    // Responsibility information from ResponsibilityFilling
    [NotMapped]
    public int? ResponsibleUserId { get; set; }
    
    [NotMapped]
    public string? ResponsibleUserName { get; set; }
    
    [NotMapped]
    public double? ResponsibleQuantity { get; set; }
    
    [NotMapped]
    public double? UnassignedQuantity { get; set; }

    // Display properties
    [NotMapped]
    public string DisplayName => $"{Product?.Name ?? "Продукт"} - {BatchNumber ?? $"Партия #{Id}"}";

    [NotMapped]
    public string CreatedByDisplay => CreatedByUser != null ? $"{CreatedByUser.Surname} {CreatedByUser.Name}" : (CreatedByUserId.HasValue ? $"Пользователь #{CreatedByUserId}" : "—");
    
    [NotMapped]
    public string ResponsibleDisplay => !string.IsNullOrWhiteSpace(ResponsibleUserName) 
        ? (ResponsibleQuantity.HasValue 
            ? $"{ResponsibleUserName} ({ResponsibleQuantity} {MeasuringUnit ?? "ед."})" 
            : ResponsibleUserName)
        : (UnassignedQuantity.HasValue && UnassignedQuantity > 0 
            ? $"— (неответственное: {UnassignedQuantity} {MeasuringUnit ?? "ед."})" 
            : "—");

    [NotMapped]
    public string WarehouseDisplay => Warehouse?.Name ?? $"Склад #{WarehouseId}";

    [NotMapped]
    public string QuantityDisplay => $"{Quantity} {MeasuringUnit ?? "ед."}";

    [NotMapped]
    public string ProductCode => Product?.Code ?? "";
    
    [NotMapped]
    public string UnassignedQuantityDisplay => UnassignedQuantity.HasValue && UnassignedQuantity > 0
        ? $"Неответственное: {UnassignedQuantity} {MeasuringUnit ?? "ед."}"
        : QuantityDisplay;
}
