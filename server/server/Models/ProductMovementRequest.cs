using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace server.Models;

/// <summary>
/// Заявка на перемещение продукции между складами
/// </summary>
[Table("ProductMovementRequest")]
public class ProductMovementRequest
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(FromUser))]
    public int FromUserId { get; set; }

    [Required]
    [ForeignKey(nameof(ToUser))]
    public int ToUserId { get; set; }

    [Required]
    [ForeignKey(nameof(FromWarehouse))]
    public int FromWarehouseId { get; set; }

    [Required]
    [ForeignKey(nameof(ToWarehouse))]
    public int ToWarehouseId { get; set; }

    /// <summary>
    /// Партия, которую перемещаем
    /// </summary>
    [Required]
    [ForeignKey(nameof(ProductBatch))]
    public int ProductBatchId { get; set; }

    [Required]
    [Column(TypeName = "integer")]
    public int Quantity { get; set; } = 0;

    [Column(TypeName = "varchar(20)")]
    public string? MeasuringType { get; set; }

    [Required]
    [Column(TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [Column(TypeName = "integer")]
    public ProductMovementStatus Status { get; set; } = ProductMovementStatus.Pending;

    [Column(TypeName = "text")]
    public string? Note { get; set; }

    // Навигационные свойства (сериализуются для отображения в приложении)
    [ValidateNever]
    public virtual User? FromUser { get; set; }
    
    [ValidateNever]
    public virtual User? ToUser { get; set; }
    
    [ValidateNever]
    public virtual Warehouse? FromWarehouse { get; set; }
    
    [ValidateNever]
    public virtual Warehouse? ToWarehouse { get; set; }
    
    [ValidateNever]
    public virtual ProductBatch? ProductBatch { get; set; }
}

public enum ProductMovementStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}
