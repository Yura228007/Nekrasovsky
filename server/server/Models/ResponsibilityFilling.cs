using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models;

/// <summary>
/// Ответственность за конкретное количество материала/продукта на конкретном складе.
/// Связывает: кто (UserId), за сколько (Quantity), чего (MaterialId или ProductId), где (WarehouseId).
/// </summary>
[Table("ResponsibilityFilling")]
public class ResponsibilityFilling
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

    [ForeignKey(nameof(Material))]
    public int? MaterialId { get; set; }

    [ForeignKey(nameof(Product))]
    public int? ProductId { get; set; }

    [Required]
    [Column(TypeName = "integer")]
    public int Quantity { get; set; }

    [Column(TypeName = "varchar(20)")]
    public string? MeasuringUnit { get; set; }

    [Required]
    [Column(TypeName = "timestamp with time zone")]
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "timestamp with time zone")]
    public DateTime? ReleasedAt { get; set; }

    [Required]
    [Column(TypeName = "boolean")]
    public bool IsActive { get; set; } = true;

    [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual User User { get; set; } = null!;

    [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual Warehouse Warehouse { get; set; } = null!;

    [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual Material? Material { get; set; }

    [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual Product? Product { get; set; }
}
