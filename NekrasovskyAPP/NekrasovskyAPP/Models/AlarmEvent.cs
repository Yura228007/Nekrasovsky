using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManagerV1.Models
{
    public class AlarmEvent
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Message { get; set; }
        public string Location { get; set; }
    }
}
