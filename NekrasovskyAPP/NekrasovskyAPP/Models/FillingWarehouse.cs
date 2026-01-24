using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NekrasovskyAPP.Models
{
    [Table("FillingWarehouse")]
    public class FillingWarehouse
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(Warehouse))]
        public int WarehouseId { get; set; }

        [ForeignKey(nameof(Material))]
        public int? MaterialId { get; set; }

        [ForeignKey(nameof(Product))]
        public int? ProductId { get; set; }

        [Required]
        [Column(TypeName = "integer")]
        public int Quantity { get; set; } = 0;

        [Column(TypeName = "varchar(20)")]
        public string? MeasuringType { get; set; }

        // 🔗 Навигационные свойства
        public virtual Warehouse Warehouse { get; set; } = null!;
        public virtual Material? Material { get; set; }
        public virtual Product? Product { get; set; }
    }
}
