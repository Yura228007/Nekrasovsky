namespace server.Models;

/// <summary>
/// Ответ API: варианты выпуска для пользователя (только то, что под его ответственностью, с лимитом количества).
/// </summary>
public class ProductOutputOptionsResponse
{
    /// <summary>У пользователя есть право «Отправка на реализацию» — может выпускать без ограничения по ответственности.</summary>
    public bool HasSendToSale { get; set; }

    /// <summary>Доступные варианты: продукт + склад + макс. количество (null = без лимита).</summary>
    public List<ProductOutputOption> Options { get; set; } = new();
}

public class ProductOutputOption
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    /// <summary>Партия, из которой разрешён выпуск (null при HasSendToSale).</summary>
    public int? ProductBatchId { get; set; }
    public string? BatchNumber { get; set; }
    /// <summary>Склад, где находится партия (источник).</summary>
    public int? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    /// <summary>Склад готовой продукции (куда перемещается выпуск).</summary>
    public int? TargetWarehouseId { get; set; }
    public string? TargetWarehouseName { get; set; }
    /// <summary>Максимум, сколько можно выпустить по этой партии. null = без лимита.</summary>
    public double? MaxQuantity { get; set; }
    public string? MeasuringUnit { get; set; }
}
