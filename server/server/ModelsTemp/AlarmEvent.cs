using System;
using System.Collections.Generic;

namespace server.ModelsTemp;

public partial class AlarmEvent
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Message { get; set; }

    public string Location { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
