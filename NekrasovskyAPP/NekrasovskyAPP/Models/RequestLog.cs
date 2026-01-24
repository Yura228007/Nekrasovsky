using System;

namespace NekrasovskyAPP.Models
{
    public class RequestLog
    {
        public int Id { get; set; }
        public string HttpMethod { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public string? RequestBody { get; set; }
        public string? ResponseBody { get; set; }
        public int StatusCode { get; set; }
        public int? UserId { get; set; }
        public string? ClientIp { get; set; }
        public string? UserAgent { get; set; }
        public DateTime RequestTime { get; set; }
        public long DurationMs { get; set; }

        public int? WarehouseId { get; set; }
        public int? MaterialId { get; set; }
        public int? ProductId { get; set; }

        public User? User { get; set; }
        public Warehouse? Warehouse { get; set; }
        public Material? Material { get; set; }
        public Product? Product { get; set; }

        // Display properties for UI
        public string DisplayTime => RequestTime.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss");
        public string DisplayUser => User != null ? $"{User.Name} {User.Surname}" : "N/A";
        public string DisplayWarehouse => Warehouse?.Name ?? "-";
        public string DisplayMaterial => Material?.Name ?? "-";
        public string DisplayProduct => Product?.Name ?? "-";
        public string DisplayDuration => $"{DurationMs} ms";
        public string StatusCodeColor => StatusCode >= 200 && StatusCode < 300 ? "Green" :
                                          StatusCode >= 400 && StatusCode < 500 ? "Orange" :
                                          StatusCode >= 500 ? "Red" : "Gray";
    }
}
