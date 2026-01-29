using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    [Table("Machine")]
    public class Machine
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Код/номер станка (например, "Л-1", "Л-2")
        /// </summary>
        [Column(TypeName = "varchar(50)")]
        public string? Code { get; set; }

        /// <summary>
        /// Тип станка/линии (экструзия, ТПА, покраска и т.д.)
        /// </summary>
        [Column(TypeName = "varchar(100)")]
        public string? Type { get; set; }

        /// <summary>
        /// Описание или дополнительная информация
        /// </summary>
        [Column(TypeName = "text")]
        public string? Description { get; set; }

        /// <summary>
        /// Активен ли станок
        /// </summary>
        [Required]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Связь со складом/цехом, где находится станок
        /// </summary>
        [ForeignKey(nameof(Warehouse))]
        public int? WarehouseId { get; set; }

        [Column(TypeName = "timestamp with time zone")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства
        public virtual Warehouse? Warehouse { get; set; }
    }
}
