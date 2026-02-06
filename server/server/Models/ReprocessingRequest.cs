using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace server.Models
{
    public class ReprocessingCreateRequest
    {
        [Required]
        public int WarehouseId { get; set; }

        [Required]
        public List<ReprocessingSource> Sources { get; set; } = new();

        [Required]
        public List<ReprocessingOutput> Outputs { get; set; } = new();

        /// <summary>
        /// Количество невозвратного брака, которое отправляется на склад "Утиль"
        /// </summary>
        public double DefectQuantity { get; set; }

        /// <summary>
        /// ID склада утиля для невозвратного брака
        /// </summary>
        public int? DefectWarehouseId { get; set; }

        /// <summary>
        /// Количество на переработку, которое отправляется на склад "Утиль"
        /// </summary>
        public double RecyclingQuantity { get; set; }

        /// <summary>
        /// ID склада утиля для переработки
        /// </summary>
        public int? RecyclingWarehouseId { get; set; }

        /// <summary>
        /// Примечание к переработке (сохраняется в партии)
        /// </summary>
        public string? Note { get; set; }
    }

    public class ReprocessingSource
    {
        public int MaterialId { get; set; }
        public double Quantity { get; set; }
        public string? MeasuringType { get; set; }
    }

    public class ReprocessingOutput
    {
        public int? MaterialId { get; set; }
        public int? ProductId { get; set; }
        public double Quantity { get; set; }
        public string? MeasuringType { get; set; }
        
        // Для создания нового материала
        public string? NewMaterialCode { get; set; }
        public string? NewMaterialName { get; set; }
        public string? NewMaterialDescription { get; set; }

        /// <summary>
        /// Тип результата: Normal (нормальный) или Eco (ЭКО)
        /// </summary>
        public ReprocessingOutputType OutputType { get; set; } = ReprocessingOutputType.Normal;
    }

    public enum ReprocessingOutputType
    {
        Normal = 0,  // Нормальный результат
        Eco = 1      // ЭКО продукция
    }
}
