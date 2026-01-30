using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NekrasovskyAPP.Models
{
    [Table("Responsibility")]
    public class Responsibility
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public int? MaterialId { get; set; }

        public int? ProductId { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReleasedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public int? Quantity { get; set; }

        public string? MeasuringUnit { get; set; }
    }
}
