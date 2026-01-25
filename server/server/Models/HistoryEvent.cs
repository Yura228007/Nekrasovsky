using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    [Table("HistoryEvent")]
    public class HistoryEvent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [ForeignKey(nameof(RelatedUser))]
        public int? RelatedUserId { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Action { get; set; } = string.Empty;

        [Column(TypeName = "varchar(50)")]
        public string? EntityType { get; set; }

        public int? EntityId { get; set; }

        [Required]
        [Column(TypeName = "text")]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "timestamp with time zone")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? WarehouseId { get; set; }
        public int? MaterialId { get; set; }
        public int? ProductId { get; set; }

        [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual User User { get; set; } = null!;

        [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual User? RelatedUser { get; set; }

        [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual Warehouse? Warehouse { get; set; }

        [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual Material? Material { get; set; }

        [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual Product? Product { get; set; }
    }
}
