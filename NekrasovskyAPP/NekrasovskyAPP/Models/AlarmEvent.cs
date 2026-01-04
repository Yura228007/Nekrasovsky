using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NekrasovskyAPP.Models
{
    [Table("AlarmEvent")]
    public class AlarmEvent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [Required]
        [Column(TypeName = "timestamp with time zone")]
        public DateTime CreatedAt { get; set; }

        [Column(TypeName = "text")]
        public string? Message { get; set; }

        [Required]
        [Column(TypeName = "varchar(255)")]
        public string Location { get; set; } = string.Empty;

        // Навигационное свойство (к пользователю)
        public virtual User User { get; set; } = null!;
    }
}
