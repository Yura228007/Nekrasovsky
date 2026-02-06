using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NekrasovskyAPP.Models
{
    [Table("PartRequest")]
    public class PartRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(FromUser))]
        public int FromUserId { get; set; }

        [Required]
        [ForeignKey(nameof(ToUser))]
        public int ToUserId { get; set; }

        [Required]
        [ForeignKey(nameof(FromWarehouse))]
        public int FromWarehouseId { get; set; }

        [Required]
        [ForeignKey(nameof(ToWarehouse))]
        public int ToWarehouseId { get; set; }

        [ForeignKey(nameof(Material))]
        public int? MaterialId { get; set; }

        [ForeignKey(nameof(Product))]
        public int? ProductId { get; set; }

        [Required]
        [Column(TypeName = "integer")]
        public double Quantity { get; set; } = 0;

        [Column(TypeName = "varchar(20)")]
        public string? MeasuringType { get; set; }

        [Required]
        [Column(TypeName = "timestamp with time zone")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "integer")]
        public PartRequestStatus Status { get; set; } = PartRequestStatus.Pending;

        // 🔗 Навигационные свойства
        public virtual User FromUser { get; set; } = null!;
        public virtual User ToUser { get; set; } = null!;
        public virtual Warehouse FromWarehouse { get; set; } = null!;
        public virtual Warehouse ToWarehouse { get; set; } = null!;
        public virtual Material? Material { get; set; }
        public virtual Product? Product { get; set; }

        /// <summary>Текст статуса для отображения в списке.</summary>
        [NotMapped]
        public string StatusDisplay => Status switch
        {
            PartRequestStatus.Pending => "Ожидает",
            PartRequestStatus.Approved => "Одобрено",
            PartRequestStatus.Rejected => "Отклонено",
            _ => "—"
        };

        /// <summary>Название материала или продукта для отображения.</summary>
        [NotMapped]
        public string ItemName => Material?.Name ?? Product?.Name ?? (MaterialId.HasValue ? $"Материал #{MaterialId}" : ProductId.HasValue ? $"Продукт #{ProductId}" : "—");
    }

    public enum PartRequestStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }
}