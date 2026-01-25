using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NekrasovskyAPP.Models
{
    [Table("Reprocessing")]
    public class Reprocessing
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int UserId { get; set; }

        public int WarehouseId { get; set; }

        public int SourceMaterialId { get; set; }

        public int SourceQuantity { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<ReprocessingItem> Items { get; set; } = new();
    }
}
