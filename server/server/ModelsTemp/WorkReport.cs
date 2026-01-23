using System;
using System.Collections.Generic;

namespace server.ModelsTemp;

public partial class WorkReport
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateOnly Date { get; set; }

    public DateTime StartWork { get; set; }

    public DateTime? FinishWork { get; set; }

    public string? Note { get; set; }

    public virtual User User { get; set; } = null!;
}
