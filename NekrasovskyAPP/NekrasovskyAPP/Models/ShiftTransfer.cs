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
        public int FromEmployeeId { get; set; }
        public int ToEmployeeId { get; set; }
        public DateTime TransferDate { get; set; }
        public bool Confirmed { get; set; }
    }
}
