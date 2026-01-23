using System;
using System.Collections.Generic;

namespace server.ModelsTemp;

public partial class RequestLog
{
    public int Id { get; set; }

    public string HttpMethod { get; set; } = null!;

    public string Url { get; set; } = null!;

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

    public virtual User? User { get; set; }
}
