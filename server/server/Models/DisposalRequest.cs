using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models;

/// <summary>
/// Запрос на перемещение материала на склад утиля (из переработки).
/// Ответственность остается у создателя до подтверждения менеджером склада утиля.
/// </summary>
[Table("DisposalRequest")]
public class DisposalRequest
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(FromUser))]
    public int FromUserId { get; set; }

    /// <summary>
    /// Пользователь, который подтвердил запрос (менеджер склада утиля)
    /// </summary>
    [ForeignKey(nameof(ApprovedByUser))]
    public int? ApprovedByUserId { get; set; }

    /// <summary>
    /// Склад, с которого отправляется материал (склад переработки)
    /// </summary>
    [Required]
    [ForeignKey(nameof(FromWarehouse))]
    public int FromWarehouseId { get; set; }

    [Required]
    [ForeignKey(nameof(ToWarehouse))]
    public int ToWarehouseId { get; set; }

    /// <summary>
    /// ID материала (для запросов на материалы). Должен быть заполнен только один из MaterialId или ProductId.
    /// </summary>
    [ForeignKey(nameof(Material))]
    public int? MaterialId { get; set; }

    /// <summary>
    /// ID продукта (для запросов на продукты, например, брак из упаковки). Должен быть заполнен только один из MaterialId или ProductId.
    /// </summary>
    [ForeignKey(nameof(Product))]
    public int? ProductId { get; set; }

    [Required]
    [Column(TypeName = "double precision")]
    public double Quantity { get; set; }

    [Column(TypeName = "varchar(20)")]
    public string? MeasuringUnit { get; set; }

    /// <summary>
    /// Тип запроса: Defect (невозвратный брак) или Recycling (переработка)
    /// </summary>
    [Required]
    [Column(TypeName = "varchar(20)")]
    public DisposalRequestType RequestType { get; set; }

    [Required]
    [Column(TypeName = "integer")]
    public DisposalRequestStatus Status { get; set; } = DisposalRequestStatus.Pending;

    [Required]
    [Column(TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "timestamp with time zone")]
    public DateTime? ProcessedAt { get; set; }

    [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual User FromUser { get; set; } = null!;

    [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual User? ApprovedByUser { get; set; }

    [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual Warehouse FromWarehouse { get; set; } = null!;

    [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual Warehouse ToWarehouse { get; set; } = null!;

    [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual Material? Material { get; set; }

    [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual Product? Product { get; set; }
}

public enum DisposalRequestType
{
    Defect = 0,      // Невозвратный брак
    Recycling = 1   // Переработка
}

public enum DisposalRequestStatus
{
    Pending = 0,    // Ожидает подтверждения
    Approved = 1,   // Подтвержден (ответственность передана)
    Rejected = 2    // Отклонен (ответственность остается у создателя)
}
