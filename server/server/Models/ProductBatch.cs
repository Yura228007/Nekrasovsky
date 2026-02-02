using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models;

/// <summary>
/// Партия продукции - одна запись = один идентифицируемый объём продукции.
/// Привязана к конкретному человеку (кто изготовил/принял) и моменту (когда).
/// </summary>
[Table("ProductBatch")]
public class ProductBatch
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(Product))]
    public int ProductId { get; set; }

    [Required]
    [ForeignKey(nameof(Warehouse))]
    public int WarehouseId { get; set; }

    [Required]
    [Column(TypeName = "integer")]
    public int Quantity { get; set; } = 0;

    [Column(TypeName = "varchar(20)")]
    public string? MeasuringUnit { get; set; }

    /// <summary>
    /// Пользователь, ответственный за партию (создал/принял). null — ответственность снята.
    /// </summary>
    [ForeignKey(nameof(CreatedByUser))]
    public int? CreatedByUserId { get; set; }

    /// <summary>
    /// Момент создания партии
    /// </summary>
    [Required]
    [Column(TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Номер партии для отображения (опционально)
    /// </summary>
    [Column(TypeName = "varchar(50)")]
    public string? BatchNumber { get; set; }

    /// <summary>
    /// Связь с выпуском продукции (если партия создана при производстве)
    /// </summary>
    [ForeignKey(nameof(ProductOutput))]
    public int? ProductOutputId { get; set; }

    /// <summary>
    /// Примечание
    /// </summary>
    [Column(TypeName = "text")]
    public string? Note { get; set; }

    [Required]
    [Column(TypeName = "boolean")]
    public bool IsActive { get; set; } = true;

    // Навигационные свойства
    [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public virtual Product? Product { get; set; }

    [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public virtual Warehouse? Warehouse { get; set; }

    [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public virtual User? CreatedByUser { get; set; }

    [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual ProductOutput? ProductOutput { get; set; }
}
