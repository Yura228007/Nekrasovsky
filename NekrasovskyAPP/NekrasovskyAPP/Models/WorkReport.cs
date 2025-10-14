using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PasswordManagerV1.Models
{
    public class WorkReport
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public double Hours { get; set; }
        public List<PartRequest> partRequests { get; set; } = new List<PartRequest> ();
        public string Note { get; set; }
    }
}