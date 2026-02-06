namespace NekrasovskyAPP.Models
{
    public class DisposalProcessRequest
    {
        public int DisposalWarehouseId { get; set; }
        public int? MaterialId { get; set; }
        public int? ProductId { get; set; }
        public double ReturnableQuantity { get; set; }
        public double NonReturnableQuantity { get; set; }
    }
}
