using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models;

/// <summary>
/// Продажа материала/продукта со склада СДХ - запись о списании для отчетов
/// </summary>
[Table("MaterialSDHSale")]
public class MaterialSDHSale
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
    /// Склад СДХ, с которого списывается материал/продукт
    /// </summary>
    [Required]
    [ForeignKey(nameof(Warehouse))]
    public int WarehouseId { get; set; }

    /// <summary>
    /// ID материала (для продажи материалов). Должен быть заполнен только один из MaterialId или ProductId.
    /// </summary>
    [ForeignKey(nameof(Material))]
    public int? MaterialId { get; set; }

    /// <summary>
    /// ID продукта (для продажи продуктов). Должен быть заполнен только один из MaterialId или ProductId.
    /// </summary>
    [ForeignKey(nameof(Product))]
    public int? ProductId { get; set; }

    /// <summary>
    /// Количество проданного материала/продукта
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
    public virtual Material? Material { get; set; }

    [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual Product? Product { get; set; }
}
