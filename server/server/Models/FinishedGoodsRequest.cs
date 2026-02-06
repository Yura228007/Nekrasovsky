using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models;

/// <summary>
/// Запрос на перемещение продукции на склад готовой продукции (из упаковки).
/// Аналогично DisposalRequest, но для готовой продукции.
/// </summary>
[Table("FinishedGoodsRequest")]
public class FinishedGoodsRequest
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(FromUser))]
    public int FromUserId { get; set; }

    /// <summary>
    /// Пользователь, который подтвердил запрос (менеджер склада готовой продукции)
    /// </summary>
    [ForeignKey(nameof(ApprovedByUser))]
    public int? ApprovedByUserId { get; set; }

    /// <summary>
    /// Склад, с которого отправляется продукция (склад производства/упаковки)
    /// </summary>
    [Required]
    [ForeignKey(nameof(FromWarehouse))]
    public int FromWarehouseId { get; set; }

    [Required]
    [ForeignKey(nameof(ToWarehouse))]
    public int ToWarehouseId { get; set; }

    [Required]
    [ForeignKey(nameof(Product))]
    public int ProductId { get; set; }

    [Required]
    [Column(TypeName = "double precision")]
    public double Quantity { get; set; }

    [Column(TypeName = "varchar(20)")]
    public string? MeasuringUnit { get; set; }

    /// <summary>
    /// Тип результата: Normal (нормальная продукция) или Eco (ЭКО продукция)
    /// </summary>
    [Required]
    [Column(TypeName = "varchar(20)")]
    public FinishedGoodsRequestType RequestType { get; set; }

    [Required]
    [Column(TypeName = "integer")]
    public FinishedGoodsRequestStatus Status { get; set; } = FinishedGoodsRequestStatus.Pending;

    [Required]
    [Column(TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "timestamp with time zone")]
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// ID записи упаковки (ProductOutput), из которой создан запрос
    /// </summary>
    [ForeignKey(nameof(ProductOutput))]
    public int? ProductOutputId { get; set; }

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
    public virtual Product Product { get; set; } = null!;

    [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual ProductOutput? ProductOutput { get; set; }
}

public enum FinishedGoodsRequestType
{
    Normal = 0,  // Нормальная продукция
    Eco = 1      // ЭКО продукция
}

public enum FinishedGoodsRequestStatus
{
    Pending = 0,    // Ожидает подтверждения
    Approved = 1,   // Подтвержден (ответственность передана)
    Rejected = 2    // Отклонен (ответственность остается у создателя)
}
