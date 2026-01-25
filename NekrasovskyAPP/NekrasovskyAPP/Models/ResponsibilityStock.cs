using System.Collections.ObjectModel;

namespace NekrasovskyAPP.Models
{
    public class ResponsibilityStockItem
    {
        public string ItemType { get; set; } = string.Empty;
        public int ItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string MeasuringUnit { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public ObservableCollection<ResponsibilityWarehouseStock> Warehouses { get; set; } = new();
        public bool IsChecked { get; set; }
    }

    public class ResponsibilityWarehouseStock
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string MeasuringType { get; set; } = string.Empty;
    }
}
