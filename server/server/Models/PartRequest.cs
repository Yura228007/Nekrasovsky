using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace server.Models
{
    [Table("PartRequest")]
    public class PartRequest
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

        [Required]
        [ForeignKey(nameof(Material))]
        public int MaterialId { get; set; }

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
        public PartRequestStatus Status { get; set; } = PartRequestStatus.Pending;

        // Навигационные свойства (не участвуют в валидации/JSON)
        [JsonIgnore]
        [ValidateNever]
        public virtual User? FromUser { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public virtual User? ToUser { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public virtual Warehouse? FromWarehouse { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public virtual Warehouse? ToWarehouse { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public virtual Material? Material { get; set; }
    }

    public enum PartRequestStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }
}