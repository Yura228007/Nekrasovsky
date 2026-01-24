using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NekrasovskyAPP.Models
{
    [Table("WorkReport")]
    public class WorkReport
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [Required]
        [Column(TypeName = "timestamp with time zone")]
        public DateTime StartWork { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "timestamp with time zone")]
        public DateTime? FinishWork { get; set; }

        [Column(TypeName = "text")]
        public string? Note { get; set; }

        // 🔗 Навигационное свойство
        public virtual User User { get; set; } = null!;

        public string DisplayStartTime => StartWork.ToLocalTime().ToString("HH:mm");
        public string? DisplayFinishTime => FinishWork.HasValue
            ? FinishWork.Value.ToLocalTime().ToString("HH:mm")
            : null;
    }
}