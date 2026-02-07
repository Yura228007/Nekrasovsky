namespace server.Models;

public class PartRequestDto
{
    public int Id { get; set; }
    public int FromUserId { get; set; }
    public int ToUserId { get; set; }
    public int FromWarehouseId { get; set; }
    public int ToWarehouseId { get; set; }
    public int? MaterialId { get; set; }
    public int? ProductId { get; set; }
    public double Quantity { get; set; }
    public string? MeasuringType { get; set; }
    public DateTime CreatedAt { get; set; }
    public PartRequestStatus Status { get; set; }
    
    // Названия для отображения
    public string? MaterialName { get; set; }
    public string? ProductName { get; set; }
    
    // Имена пользователей для отображения
    public string? FromUserName { get; set; }
    public string? ToUserName { get; set; }
    
    // Названия складов для отображения
    public string? FromWarehouseName { get; set; }
    public string? ToWarehouseName { get; set; }
    
    // Вычисляемое свойство для удобства
    public string ItemName => MaterialName ?? ProductName ?? (MaterialId.HasValue ? $"Материал #{MaterialId}" : ProductId.HasValue ? $"Продукт #{ProductId}" : "—");
}
