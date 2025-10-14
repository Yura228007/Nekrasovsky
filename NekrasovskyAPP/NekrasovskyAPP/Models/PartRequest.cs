using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PasswordManagerV1.Models
{
    public class PartRequest
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int MaterialId { get; set; }
        public int Quantity { get; set; } = 0;
        public string? MeasuringType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public PartRequestStatus Status { get; set; } = PartRequestStatus.Pending;
    }
    public enum PartRequestStatus { Pending = 0, Approved = 1, Rejected = 2 }
}