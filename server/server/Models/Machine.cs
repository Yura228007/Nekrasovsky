using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    [Table("Machine")]
    public class Machine
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Тип станка (экструзия, ТПА, покраска и т.д.)
        /// </summary>
        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Type { get; set; } = "???";

        /// <summary>
        /// Активен ли станок
        /// </summary>
        [Required]
        [Column(TypeName = "boolean")]
        public bool IsActive { get; set; } = true;
    }
}
