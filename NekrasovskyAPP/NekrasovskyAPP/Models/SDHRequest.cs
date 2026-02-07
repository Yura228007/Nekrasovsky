namespace NekrasovskyAPP.Models;

public class SDHRequest
{
    public int Id { get; set; }
    public int FromUserId { get; set; }
    public int? ApprovedByUserId { get; set; }
    public int FromWarehouseId { get; set; }
    public int ToWarehouseId { get; set; }
    public int? MaterialId { get; set; }
    public int? ProductId { get; set; }
    public double Quantity { get; set; }
    public string? MeasuringUnit { get; set; }
    public SDHRequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }

    // Display properties
    public string? MaterialName { get; set; }
    public string? ProductName { get; set; }
    public string? FromUserName { get; set; }
    public string? FromWarehouseName { get; set; }
    public string? ToWarehouseName { get; set; }

    public string ItemName => MaterialId.HasValue ? MaterialName ?? "Неизвестный материал" : ProductName ?? "Неизвестный продукт";
}

public enum SDHRequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}
