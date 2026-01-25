namespace server.Models
{
    public class ResponsibilityAssignment
    {
        public int ItemId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}
