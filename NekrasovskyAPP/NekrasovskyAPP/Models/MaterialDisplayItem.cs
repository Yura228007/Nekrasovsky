using System.Linq;

namespace NekrasovskyAPP.Models
{
    /// <summary>
    /// Информация о количестве материала на конкретном складе
    /// </summary>
    public class WarehouseStockInfo
    {
        public string WarehouseName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string MeasuringUnit { get; set; } = string.Empty;
    }

    /// <summary>
    /// Обертка для отображения материала с информацией об ответственном лице
    /// Позволяет отображать несколько карточек для одного материала, если у него несколько ответственных
    /// </summary>
    public class MaterialDisplayItem
    {
        public Material Material { get; set; } = null!;
        public ResponsibilityAssignment? Responsibility { get; set; }

        /// <summary>
        /// Количество неответственной части (только для карточки с Responsibility == null).
        /// При назначении ответственности передаётся это количество, а не null.
        /// </summary>
        public int? UnassignedQuantity { get; set; }

        /// <summary>
        /// Единица измерения неответственной части.
        /// </summary>
        public string? UnassignedMeasuringUnit { get; set; }
        
        // Список складов с количеством для этого ответственного (или для неответственной части)
        public List<WarehouseStockInfo> WarehouseStocks { get; set; } = new();
        
        // Свойства для удобного доступа к данным материала
        public int Id => Material.Id;
        public string Name => Material.Name;
        public string? Code => Material.Code;
        public string MeasuringUnit => Material.MeasuringUnit;
        public bool IsActive => Material.IsActive;
        public string? Description => Material.Description;
        public string StockSummary => Material.StockSummary;
        
        // Свойство для отображения ответственности
        public string ResponsibilityDisplay
        {
            get
            {
                if (Responsibility == null)
                {
                    return "Ответственный: — (неответственная часть)";
                }
                
                if (Responsibility.Quantity.HasValue)
                {
                    var unit = !string.IsNullOrWhiteSpace(Responsibility.MeasuringUnit)
                        ? Responsibility.MeasuringUnit
                        : (!string.IsNullOrWhiteSpace(Material.MeasuringUnit) ? Material.MeasuringUnit : "ед.");
                    return $"Ответственный: {Responsibility.UserName} ({Responsibility.Quantity} {unit})";
                }

                return $"Ответственный: {Responsibility.UserName}";
            }
        }
        
        public bool HasResponsibility => Responsibility != null;
        
        // Свойство для отображения складов (только если есть ответственный)
        public string WarehouseStocksDisplay
        {
            get
            {
                if (!HasResponsibility || WarehouseStocks.Count == 0)
                {
                    return string.Empty;
                }
                
                return string.Join("\n", WarehouseStocks.Select(ws => $"{ws.WarehouseName} - {ws.Quantity} {(!string.IsNullOrWhiteSpace(ws.MeasuringUnit) ? ws.MeasuringUnit : "ед.")}"));
            }
        }
        
        public bool HasWarehouseStocks => HasResponsibility && WarehouseStocks.Count > 0;

        /// <summary>Остатки: либо по складам (WarehouseStocksDisplay), либо общая подпись (StockSummary).</summary>
        public string DisplayStocks => !string.IsNullOrWhiteSpace(WarehouseStocksDisplay) ? WarehouseStocksDisplay : StockSummary;

        /// <summary>Единица измерения для отображения в карточке.</summary>
        public string MeasuringUnitDisplay => !string.IsNullOrWhiteSpace(Material.MeasuringUnit) ? Material.MeasuringUnit : "ед.";
    }
}