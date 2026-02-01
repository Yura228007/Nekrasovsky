using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NekrasovskyAPP.Models
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
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        public long FileSize { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime ShiftStart { get; set; }

        [Required]
        public DateTime ShiftEnd { get; set; }

        public string? Summary { get; set; }

        // Navigation properties
        public virtual WorkReport? WorkReport { get; set; }
        public virtual User? User { get; set; }

        // Display properties
        public string DisplayShiftStart => ShiftStart.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        public string DisplayShiftEnd => ShiftEnd.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        public string DisplayCreatedAt => CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        public string DisplayFileSize => FileSize < 1024
            ? $"{FileSize} Б"
            : FileSize < 1024 * 1024
                ? $"{FileSize / 1024.0:F1} КБ"
                : $"{FileSize / (1024.0 * 1024.0):F1} МБ";
    }
}
