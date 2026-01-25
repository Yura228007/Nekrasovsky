using System.Collections.Generic;

namespace server.Models
{
    public class ResponsibilityStockItem
    {
        public string ItemType { get; set; } = string.Empty; // Material/Product
        public int ItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string MeasuringUnit { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public List<ResponsibilityWarehouseStock> Warehouses { get; set; } = new();
    }

    public class ResponsibilityWarehouseStock
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string MeasuringType { get; set; } = string.Empty;
    }
}
