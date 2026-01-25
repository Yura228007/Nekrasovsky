using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    [Table("Reprocessing")]
    public class Reprocessing
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [Required]
        [ForeignKey(nameof(Warehouse))]
        public int WarehouseId { get; set; }

        [Required]
        [ForeignKey(nameof(SourceMaterial))]
        public int SourceMaterialId { get; set; }

        [Required]
        [Column(TypeName = "integer")]
        public int SourceQuantity { get; set; }

        [Required]
        [Column(TypeName = "timestamp with time zone")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual User User { get; set; } = null!;

        [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual Warehouse Warehouse { get; set; } = null!;

        [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual Material SourceMaterial { get; set; } = null!;

        public virtual ICollection<ReprocessingItem> Items { get; set; } = new List<ReprocessingItem>();
    }
}
