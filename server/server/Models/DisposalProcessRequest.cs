namespace server.Models
{
    public class DisposalProcessRequest
    {
        public int DisposalWarehouseId { get; set; }
        public int? MaterialId { get; set; }
        public int? ProductId { get; set; }
        public int ReturnableQuantity { get; set; }
        public int NonReturnableQuantity { get; set; }
    }
}
