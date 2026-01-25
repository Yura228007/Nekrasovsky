using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NekrasovskyAPP.Models
{
    public class HistoryEvent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int UserId { get; set; }
        public int? RelatedUserId { get; set; }

        public string Action { get; set; } = string.Empty;
        public string? EntityType { get; set; }
        public int? EntityId { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? WarehouseId { get; set; }
        public int? MaterialId { get; set; }
        public int? ProductId { get; set; }

        [NotMapped]
        public string DisplayTime => CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss");

        [NotMapped]
        public string ActorDisplay { get; set; } = string.Empty;

        [NotMapped]
        public string DisplayDescription { get; set; } = string.Empty;
    }
}
