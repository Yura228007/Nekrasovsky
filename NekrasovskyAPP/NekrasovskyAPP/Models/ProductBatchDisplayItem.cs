using NekrasovskyAPP.Models;

namespace NekrasovskyAPP.Models;

public class ProductBatchDisplayItem
{
    public ProductBatch Batch { get; set; } = null!;
    
    /// <summary>
    /// Ответственное лицо из ResponsibilityFilling (null если неответственная часть)
    /// </summary>
    public ResponsibilityFilling? Responsibility { get; set; }
    
    /// <summary>
    /// Неответственное количество (если нет ответственного лица)
    /// </summary>
    public double? UnassignedQuantity { get; set; }
    
    /// <summary>
    /// Единица измерения неответственной части
    /// </summary>
    public string? UnassignedMeasuringUnit { get; set; }
    
    // Свойства для удобного доступа к данным партии
    public int Id => Batch.Id;
    public string DisplayName => Batch.DisplayName;
    public string? ProductCode => Batch.ProductCode;
    public string WarehouseDisplay => Batch.WarehouseDisplay;
    public string QuantityDisplay => Batch.QuantityDisplay;
    
    /// <summary>
    /// Свойство для отображения ответственности
    /// </summary>
    public string ResponsibilityDisplay
    {
        get
        {
            if (Responsibility == null)
            {
                if (UnassignedQuantity.HasValue && UnassignedQuantity > 0)
                {
                    var unassignedUnit = UnassignedMeasuringUnit ?? Batch.MeasuringUnit ?? "ед.";
                    return $"— (неответственное: {UnassignedQuantity} {unassignedUnit})";
                }
                return "—";
            }
            
            var responsibleUnit = !string.IsNullOrWhiteSpace(Responsibility.MeasuringUnit)
                ? Responsibility.MeasuringUnit
                : (Batch.MeasuringUnit ?? "ед.");
            
            // ResponsibilityFilling не содержит User, поэтому используем UserId
            // Имя пользователя будет получено отдельно при необходимости
            var userName = $"Пользователь #{Responsibility.UserId}";
            
            return userName + (Responsibility.Quantity > 0 ? $" ({Responsibility.Quantity} {responsibleUnit})" : "");
        }
    }
    
    public bool HasResponsibility => Responsibility != null;
}
