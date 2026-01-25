using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NekrasovskyAPP.Models
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

        [NotMapped]
        public string FromUserDisplay { get; set; } = string.Empty;

        [NotMapped]
        public string ToUserDisplay { get; set; } = string.Empty;

        // 🔗 Навигационные свойства
        public virtual User FromUser { get; set; } = null!;
        public virtual User ToUser { get; set; } = null!;
    }
}
