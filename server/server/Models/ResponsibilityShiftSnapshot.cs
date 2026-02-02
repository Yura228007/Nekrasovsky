using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models;

/// <summary>
/// Снимок ответственности сотрудника на начало смены (при StartWork).
/// </summary>
[Table("ResponsibilityShiftSnapshot")]
public class ResponsibilityShiftSnapshot
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
    [Column(TypeName = "timestamp with time zone")]
    public DateTime SnapshotAt { get; set; } = DateTime.UtcNow;

    [System.Text.Json.Serialization.JsonIgnore]
    public virtual WorkReport WorkReport { get; set; } = null!;

    [System.Text.Json.Serialization.JsonIgnore]
    public virtual User User { get; set; } = null!;

    public virtual ICollection<ResponsibilityShiftSnapshotItem> Items { get; set; } = new List<ResponsibilityShiftSnapshotItem>();
}
