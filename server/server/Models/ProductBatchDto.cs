namespace server.Models;

public class ProductBatchDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public double Quantity { get; set; }
    public string? MeasuringUnit { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? BatchNumber { get; set; }
    public string? Note { get; set; }
    public bool IsActive { get; set; }

    // Navigation properties
    public Product? Product { get; set; }
    public Warehouse? Warehouse { get; set; }
    public User? CreatedByUser { get; set; }

    // Responsibility information from ResponsibilityFilling
    public int? ResponsibleUserId { get; set; }
    public string? ResponsibleUserName { get; set; }
    public double? ResponsibleQuantity { get; set; }
    public double? UnassignedQuantity { get; set; }
}
