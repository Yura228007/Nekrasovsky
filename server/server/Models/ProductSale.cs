using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models;

/// <summary>
/// Продажа готовой продукции - запись о списании продукции со склада готовой продукции при продаже
/// </summary>
[Table("ProductSale")]
public class ProductSale
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Пользователь, который оформил продажу
    /// </summary>
    [Required]
    [ForeignKey(nameof(User))]
    public int UserId { get; set; }

    /// <summary>
    /// Склад готовой продукции, с которого списывается продукция
    /// </summary>
    [Required]
    [ForeignKey(nameof(Warehouse))]
    public int WarehouseId { get; set; }

    /// <summary>
    /// Продукт, который был продан
    /// </summary>
    [Required]
    [ForeignKey(nameof(Product))]
    public int ProductId { get; set; }

    /// <summary>
    /// Количество проданной продукции
    /// </summary>
    [Required]
    [Column(TypeName = "double precision")]
    public double Quantity { get; set; }

    /// <summary>
    /// Единица измерения
    /// </summary>
    [Column(TypeName = "varchar(20)")]
    public string? MeasuringUnit { get; set; }

    /// <summary>
    /// Дата и время продажи
    /// </summary>
    [Required]
    [Column(TypeName = "timestamp with time zone")]
    public DateTime SoldAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Примечание к продаже (опционально)
    /// </summary>
    [Column(TypeName = "text")]
    public string? Note { get; set; }

    // Навигационные свойства
    [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual User User { get; set; } = null!;

    [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual Warehouse Warehouse { get; set; } = null!;

    [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual Product Product { get; set; } = null!;
}
