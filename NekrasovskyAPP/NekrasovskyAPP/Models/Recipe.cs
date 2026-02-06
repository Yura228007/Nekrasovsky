using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NekrasovskyAPP.Models
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
        [Column(TypeName = "double precision")]
        public double Quantity { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string? MeasuringType { get; set; }

        // 🔗 Навигационные свойства
        public virtual Product Product { get; set; } = null!;
        public virtual Material Material { get; set; } = null!;
    }
}
