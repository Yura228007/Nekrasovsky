using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NekrasovskyAPP.Models
{
    [Table("ReprocessingItem")]
    public class ReprocessingItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ReprocessingId { get; set; }

        public int? MaterialId { get; set; }

        public int? ProductId { get; set; }

        public int Quantity { get; set; }

        public string? MeasuringType { get; set; }
    }
}
