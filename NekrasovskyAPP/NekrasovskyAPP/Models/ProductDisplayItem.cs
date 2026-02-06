using System.Linq;

namespace NekrasovskyAPP.Models
{
    /// <summary>
    /// Обертка для отображения продукта с информацией об ответственном лице
    /// Позволяет отображать несколько карточек для одного продукта, если у него несколько ответственных
    /// </summary>
    public class ProductDisplayItem
    {
        public Product Product { get; set; } = null!;
        public ResponsibilityAssignment? Responsibility { get; set; }

        /// <summary>
        /// Количество неответственной части (только для карточки с Responsibility == null).
        /// При назначении ответственности передаётся это количество, а не null.
        /// </summary>
        public double? UnassignedQuantity { get; set; }

        /// <summary>
        /// Единица измерения неответственной части.
        /// </summary>
        public string? UnassignedMeasuringUnit { get; set; }
        
        // Список складов с количеством для этого ответственного (или для неответственной части)
        public List<WarehouseStockInfo> WarehouseStocks { get; set; } = new();
        
        // Свойства для удобного доступа к данным продукта
        public int Id => Product.Id;
        public string Name => Product.Name;
        public string? Code => Product.Code;
        public string MeasuringUnit => Product.MeasuringUnit;
        public bool IsActive => Product.IsActive;
        public string? Description => Product.Description;
        public string StockSummary => Product.StockSummary;
        
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
                        : (!string.IsNullOrWhiteSpace(Product.MeasuringUnit) ? Product.MeasuringUnit : "ед.");
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
    }
}