using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekrasovskyAPP.Models
{
    [Table("UserPermissions")]
    public class UserPermissions
    {
        [Required]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [Required]
        [ForeignKey(nameof(Permission))]
        public int PermissionId { get; set; }

        // 🔗 Навигационные свойства
        public virtual User User { get; set; } = null!;
        public virtual Permission Permission { get; set; } = null!;
    }
}
