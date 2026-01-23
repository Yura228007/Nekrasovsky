using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NekrasovskyAPP.Models
{
    [Table("Material")]
    public class Material
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "text")]
        public string? Description { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string? Code { get; set; }

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string MeasuringUnit { get; set; } = "шт";

        [Required]
        [Column(TypeName = "boolean")]
        public bool IsActive { get; set; } = true;
    }

}