using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PasswordManagerV1.Models
{
    public class PartRequest
    {
        public int Id { get; set; }
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public int FromWarehouseId { get; set; }
        public int ToWarehouseId { get; set; }
        public int MaterialId { get; set; }
        public int Quantity { get; set; } = 0;
        public string? MeasuringType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public PartRequestStatus Status { get; set; } = PartRequestStatus.Pending;
    }
    public enum PartRequestStatus { Pending = 0, Approved = 1, Rejected = 2 }
}