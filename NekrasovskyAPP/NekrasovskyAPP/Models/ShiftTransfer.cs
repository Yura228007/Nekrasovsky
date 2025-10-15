using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManagerV1.Models
{
    public class ShiftTransfer
    {
        public int Id { get; set; }
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public DateTime TransferDate { get; set; }
        public bool IsConfirmed { get; set; }
    }
}
