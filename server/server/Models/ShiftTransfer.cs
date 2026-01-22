using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server.Models
{
    [Table("ShiftTransfer")]
    public class ShiftTransfer
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
        [Column(TypeName = "timestamp with time zone")]
        public DateTime TransferDate { get; set; }

        [Required]
        [Column(TypeName = "boolean")]
        public bool IsConfirmed { get; set; } = false;

        // Навигационные свойства (не участвуют в валидации/JSON)
        [JsonIgnore]
        [ValidateNever]
        public virtual User? FromUser { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public virtual User? ToUser { get; set; }
    }
}
