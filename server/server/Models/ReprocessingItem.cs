using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    [Table("ReprocessingItem")]
    public class ReprocessingItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(Reprocessing))]
        public int ReprocessingId { get; set; }

        [ForeignKey(nameof(Material))]
        public int? MaterialId { get; set; }

        [ForeignKey(nameof(Product))]
        public int? ProductId { get; set; }

        [Required]
        [Column(TypeName = "integer")]
        public double Quantity { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string? MeasuringType { get; set; }

        [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual Reprocessing Reprocessing { get; set; } = null!;

        [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual Material? Material { get; set; }

        [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual Product? Product { get; set; }
    }
}
