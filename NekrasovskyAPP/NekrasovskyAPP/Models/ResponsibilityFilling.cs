namespace NekrasovskyAPP.Models;

public class ResponsibilityFilling
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int WarehouseId { get; set; }
    public int? MaterialId { get; set; }
    public int? ProductId { get; set; }
    public int? ProductBatchId { get; set; }
    public double Quantity { get; set; }
    public string? MeasuringUnit { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? ReleasedAt { get; set; }
    public bool IsActive { get; set; }
}
