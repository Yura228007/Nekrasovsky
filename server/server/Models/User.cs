using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PasswordManagerV1.Models
{
    [Table("User")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string Login { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(255)")]
        public string EncryptedPassword { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Surname { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(150)")]
        public string Email { get; set; } = string.Empty;

        [Column(TypeName = "varchar(20)")]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "timestamp with time zone")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 🔗 Навигационные свойства (связи с другими таблицами)
        public virtual ICollection<AlarmEvent> AlarmEvents { get; set; } = new List<AlarmEvent>();
        public virtual ICollection<PartRequest> SentPartRequests { get; set; } = new List<PartRequest>();
        public virtual ICollection<PartRequest> ReceivedPartRequests { get; set; } = new List<PartRequest>();
        public virtual ICollection<ShiftTransfer> SentShiftTransfers { get; set; } = new List<ShiftTransfer>();
        public virtual ICollection<ShiftTransfer> ReceivedShiftTransfers { get; set; } = new List<ShiftTransfer>();
    }
}