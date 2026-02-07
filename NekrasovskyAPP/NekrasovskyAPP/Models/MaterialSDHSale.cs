namespace NekrasovskyAPP.Models;

public class MaterialSDHSale
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int WarehouseId { get; set; }
    public int? MaterialId { get; set; }
    public int? ProductId { get; set; }
    public double Quantity { get; set; }
    public string? MeasuringUnit { get; set; }
    public DateTime SoldAt { get; set; }
    public string? Note { get; set; }
}
