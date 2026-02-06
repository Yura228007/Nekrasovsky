namespace server.Models;

/// <summary>
/// DTO для DisposalRequest с заполненными именами для отображения
/// </summary>
public class DisposalRequestDto
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
    public DisposalRequestType RequestType { get; set; }
    public DisposalRequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    
    // Для отображения
    public string? MaterialName { get; set; }
    public string? ProductName { get; set; }
    public string? FromUserName { get; set; }
    public string? FromWarehouseName { get; set; }
    public string? ToWarehouseName { get; set; }
}
