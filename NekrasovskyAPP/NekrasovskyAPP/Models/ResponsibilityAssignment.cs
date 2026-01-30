namespace NekrasovskyAPP.Models
{
    public class ResponsibilityAssignment
    {
        public int ItemId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int? Quantity { get; set; }
        public string? MeasuringUnit { get; set; }
    }
}
