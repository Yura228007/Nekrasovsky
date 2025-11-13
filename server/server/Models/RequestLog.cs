using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    public class RequestLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string HttpMethod { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string Url { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string? Controller { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string? Action { get; set; }

        [Column(TypeName = "text")]
        public string? RequestBody { get; set; }

        [Column(TypeName = "text")]
        public string? ResponseBody { get; set; }

        public int StatusCode { get; set; }

        public int? UserId { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string? ClientIp { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string? UserAgent { get; set; }

        [Column(TypeName = "timestamp with time zone")]
        public DateTime RequestTime { get; set; } = DateTime.UtcNow;

        public long DurationMs { get; set; }

        // ????????????? ????????
        public virtual User? User { get; set; }
    }
}
