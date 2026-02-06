namespace server.Models
{
    public class ResponsibilityAssignment
    {
        public int ItemId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public double? Quantity { get; set; }
        public string? MeasuringUnit { get; set; }
        /// <summary>Склад, за который назначена ответственность (из ResponsibilityFilling).</summary>
        public int? WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        /// <summary>ID ResponsibilityFilling для работы с конкретной карточкой.</summary>
        public int? ResponsibilityFillingId { get; set; }
    }
}
