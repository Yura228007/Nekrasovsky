using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekrasovskyAPP.Models
{
    [Table("AccessibleMovement")]
    public class AccessibleMovement
    {
        [Required]
        [ForeignKey(nameof(FromWarehouse))]
        public int FromWarehouseId { get; set; }

        [Required]
        [ForeignKey(nameof(ToWarehouse))]
        public int ToWarehouseId { get; set; }

        [Required]
        [ForeignKey(nameof(Material))]
        public int MaterialId { get; set; }

        // 🔗 Навигационные свойства
        public virtual Warehouse FromWarehouse { get; set; } = null!;
        public virtual Warehouse ToWarehouse { get; set; } = null!;
        public virtual Material Material { get; set; } = null!;
    }
}
