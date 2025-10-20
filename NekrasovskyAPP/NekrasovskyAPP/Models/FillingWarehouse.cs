using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PasswordManagerV1.Models;

namespace NekrasovskyAPP.Models
{
    [Table("FillingWarehouse")]
    public class FillingWarehouse
    {
        [Required]
        [ForeignKey(nameof(Warehouse))]
        public int WarehouseId { get; set; }

        [Required]
        [ForeignKey(nameof(Material))]
        public int MaterialId { get; set; }

        [Required]
        [Column(TypeName = "integer")]
        public int Quantity { get; set; } = 0;

        [Column(TypeName = "varchar(20)")]
        public string? MeasuringType { get; set; }

        // 🔗 Навигационные свойства
        public virtual Warehouse Warehouse { get; set; } = null!;
        public virtual Material Material { get; set; } = null!;
    }
}
