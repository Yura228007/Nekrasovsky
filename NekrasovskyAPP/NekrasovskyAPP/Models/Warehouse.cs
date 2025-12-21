using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NekrasovskyAPP.Models;

namespace NekrasovskyAPP.Models
{
    [Table("Warehouse")]
    public class Warehouse
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string Type { get; set; } = "Цех";

        // 🔗 Навигационные свойства (пригодятся для связей)
        public virtual ICollection<AccessibleMovement> FromMovements { get; set; } = new List<AccessibleMovement>();
        public virtual ICollection<AccessibleMovement> ToMovements { get; set; } = new List<AccessibleMovement>();
        public virtual ICollection<PartRequest> FromRequests { get; set; } = new List<PartRequest>();
        public virtual ICollection<PartRequest> ToRequests { get; set; } = new List<PartRequest>();
    }
}
