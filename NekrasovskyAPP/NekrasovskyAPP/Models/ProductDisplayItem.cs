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
        
        // Список складов с количеством для этого ответственного (или для неответственной части)
        public List<WarehouseStockInfo> WarehouseStocks { get; set; } = new();
        
        // Свойства для удобного доступа к данным продукта
        public int Id => Product.Id;
        public string Name => Product.Name;
        public string? Code => Product.Code;
        public string MeasuringUnit => Product.MeasuringUnit;
        public bool IsActive => Product.IsActive;
        public string? Description => Product.Description;
        
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
                        : Product.MeasuringUnit;
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
                
                return string.Join("\n", WarehouseStocks.Select(ws => $"{ws.WarehouseName} - {ws.Quantity} {ws.MeasuringUnit}"));
            }
        }
        
        public bool HasWarehouseStocks => HasResponsibility && WarehouseStocks.Count > 0;
    }
}