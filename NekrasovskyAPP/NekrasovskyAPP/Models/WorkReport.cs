using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PasswordManagerV1.Models
{
    public class WorkReport
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public DateTime StartWork { get; set; } = DateTime.Now;
        public DateTime? FinishWork { get; set; }
        public string? Note { get; set; } 
    }
}