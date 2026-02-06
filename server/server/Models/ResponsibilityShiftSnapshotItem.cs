using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models;

/// <summary>
/// Одна строка снимка: ответственность по складу + материал или продукт + количество.
/// </summary>
[Table("ResponsibilityShiftSnapshotItem")]
public class ResponsibilityShiftSnapshotItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(ResponsibilityShiftSnapshot))]
    public int ResponsibilityShiftSnapshotId { get; set; }

    [Required]
    [ForeignKey(nameof(Warehouse))]
    public int WarehouseId { get; set; }

    [ForeignKey(nameof(Material))]
    public int? MaterialId { get; set; }

    [ForeignKey(nameof(Product))]
    public int? ProductId { get; set; }

    [Required]
    [Column(TypeName = "integer")]
    public double Quantity { get; set; }

    [Column(TypeName = "varchar(20)")]
    public string? MeasuringUnit { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public virtual ResponsibilityShiftSnapshot ResponsibilityShiftSnapshot { get; set; } = null!;

    [System.Text.Json.Serialization.JsonIgnore]
    public virtual Warehouse Warehouse { get; set; } = null!;

    [System.Text.Json.Serialization.JsonIgnore]
    public virtual Material? Material { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public virtual Product? Product { get; set; }
}
