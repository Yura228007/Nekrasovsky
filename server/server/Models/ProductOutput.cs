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

        [ForeignKey(nameof(Machine))]
        public int? MachineId { get; set; }

        /// <summary>
        /// Количество произведённой продукции
        /// </summary>
        [Required]
        [Column(TypeName = "integer")]
        public int ProducedQuantity { get; set; } = 0;

        /// <summary>
        /// Количество брака
        /// </summary>
        [Column(TypeName = "integer")]
        public int DefectQuantity { get; set; } = 0;

        /// <summary>
        /// Количество эко-продукции (переработка/вторсырьё)
        /// </summary>
        [Column(TypeName = "integer")]
        public int EcoQuantity { get; set; } = 0;

        [Column(TypeName = "varchar(50)")]
        public string? MeasuringUnit { get; set; }

        [Column(TypeName = "text")]
        public string? Note { get; set; }

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
        public virtual Machine? Machine { get; set; }
    }
}
