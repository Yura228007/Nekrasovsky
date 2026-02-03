using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace server.Models
{
    [Table("ShiftReport")]
    public class ShiftReport
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(WorkReport))]
        public int WorkReportId { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [Required]
        [Column(TypeName = "varchar(255)")]
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Legacy: path on server disk. Not used when FileContent is set.
        /// </summary>
        [Column(TypeName = "varchar(500)")]
        public string? FilePath { get; set; }

        /// <summary>
        /// Report file content (Excel bytes). When set, file is not stored on disk.
        /// </summary>
        [Column(TypeName = "bytea")]
        public byte[]? FileContent { get; set; }

        [Required]
        [Column(TypeName = "bigint")]
        public long FileSize { get; set; }

        [Required]
        [Column(TypeName = "timestamp with time zone")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "timestamp with time zone")]
        public DateTime ShiftStart { get; set; }

        [Required]
        [Column(TypeName = "timestamp with time zone")]
        public DateTime ShiftEnd { get; set; }

        /// <summary>
        /// Summary of what was done during the shift
        /// </summary>
        [Column(TypeName = "text")]
        public string? Summary { get; set; }

        // Navigation properties
        [JsonIgnore]
        public virtual WorkReport? WorkReport { get; set; }

        [JsonIgnore]
        public virtual User? User { get; set; }
    }
}
