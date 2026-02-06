using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    [Table("ProductOutput")]
    public class ProductOutput
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [ForeignKey(nameof(WorkReport))]
        public int? WorkReportId { get; set; }

        [Required]
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }

        /// <summary>Склад готовой продукции (куда перемещается выпуск).</summary>
        [ForeignKey(nameof(Warehouse))]
        public int? WarehouseId { get; set; }

        /// <summary>Партия, из которой выпускается продукция (обязательно при выпуске без права «Отправка на реализацию»).</summary>
        [ForeignKey(nameof(ProductBatch))]
        public int? ProductBatchId { get; set; }


        /// <summary>
        /// Количество произведённой продукции
        /// </summary>
        [Required]
        [Column(TypeName = "double precision")]
        public double ProducedQuantity { get; set; } = 0;

        /// <summary>
        /// Количество брака
        /// </summary>
        [Column(TypeName = "double precision")]
        public double DefectQuantity { get; set; } = 0;

        /// <summary>
        /// Количество эко-продукции (переработка/вторсырьё)
        /// </summary>
        [Column(TypeName = "double precision")]
        public double EcoQuantity { get; set; } = 0;

        /// <summary>
        /// Количество на перемотку (отправляется на склад "Перемотка")
        /// </summary>
        [Column(TypeName = "double precision")]
        public double RewindQuantity { get; set; } = 0;

        /// <summary>
        /// Склад готовой продукции для нормальной продукции (если отличается от WarehouseId)
        /// </summary>
        [ForeignKey(nameof(NormalWarehouse))]
        public int? NormalWarehouseId { get; set; }

        /// <summary>
        /// Склад готовой продукции для ЭКО продукции
        /// </summary>
        [ForeignKey(nameof(EcoWarehouse))]
        public int? EcoWarehouseId { get; set; }

        /// <summary>
        /// Склад утиля для брака
        /// </summary>
        [ForeignKey(nameof(DefectWarehouse))]
        public int? DefectWarehouseId { get; set; }

        /// <summary>
        /// Склад для перемотки
        /// </summary>
        [ForeignKey(nameof(RewindWarehouse))]
        public int? RewindWarehouseId { get; set; }

        /// <summary>
        /// Пользователь, которому отправляется перемотка
        /// </summary>
        [ForeignKey(nameof(RewindToUser))]
        public int? RewindToUserId { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string? MeasuringUnit { get; set; }

        [Required]
        [Column(TypeName = "timestamp with time zone")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства
        // Note: these navigation properties are intentionally nullable.
        // With [ApiController], non-nullable reference properties are treated as required during model binding,
        // which would cause 400 responses for POST/PUT where the client sends only *Id fields.
        public virtual User? User { get; set; }
        public virtual WorkReport? WorkReport { get; set; }
        public virtual Product? Product { get; set; }
        public virtual Warehouse? Warehouse { get; set; }
        public virtual ProductBatch? ProductBatch { get; set; }
        public virtual Warehouse? NormalWarehouse { get; set; }
        public virtual Warehouse? EcoWarehouse { get; set; }
        public virtual Warehouse? DefectWarehouse { get; set; }
        public virtual Warehouse? RewindWarehouse { get; set; }
        public virtual User? RewindToUser { get; set; }
    }
}
