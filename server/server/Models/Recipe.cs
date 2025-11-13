using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server.Models
{
    [Table("Recipe")]
    public class Recipe
    {
        [Required]
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }

        [Required]
        [ForeignKey(nameof(Material))]
        public int MaterialId { get; set; }

        [Required]
        [Column(TypeName = "integer")]
        public int Quantity { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string? MeasuringType { get; set; }

        // ?? ????????????? ????????
        public virtual Product Product { get; set; } = null!;
        public virtual Material Material { get; set; } = null!;
    }
}
