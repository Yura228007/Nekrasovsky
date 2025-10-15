using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManagerV1.Models
{
    public class Law
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int UserId { get; set; }
        public bool AddAllRankUsers { get; set; } = true;
        public bool AddLowerRankUsers { get; set; } = true;
        public bool ReceiveMaterials { get; set; } = true;
        public bool AssignBarcode { get; set; } = true;
        public bool SendToDisposal { get; set; } = true;
        public bool SendToSDH { get; set; } = true;
        public bool SendForSale { get; set; } = true;
        public bool WriteOff { get; set; } = true;
        public bool TransferMainToWorkshop { get; set; } = true;
        public bool TransferWorkshopToMain { get; set; } = true;
        public bool ShiftHandover { get; set; } = true;
        public bool CreateRecipeOrUnit { get; set; } = true;
        public bool Inventory { get; set; } = true;
    }
}
