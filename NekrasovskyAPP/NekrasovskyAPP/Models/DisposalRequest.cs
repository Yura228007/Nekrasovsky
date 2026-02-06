namespace NekrasovskyAPP.Models;

public class DisposalRequest
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

    // Навигационные свойства (для отображения) - заполняются с сервера
    public string? FromUserName { get; set; }
    public string? MaterialName { get; set; }
    public string? ProductName { get; set; }
    public string? FromWarehouseName { get; set; }
    public string? ToWarehouseName { get; set; }
    
    // Вычисляемое свойство для отображения
    public string ItemName => MaterialName ?? ProductName ?? "Неизвестно";
}

public enum DisposalRequestType
{
    Defect = 0,      // Невозвратный брак
    Recycling = 1   // Производство
}

public enum DisposalRequestStatus
{
    Pending = 0,    // Ожидает подтверждения
    Approved = 1,   // Подтвержден
    Rejected = 2    // Отклонен
}
