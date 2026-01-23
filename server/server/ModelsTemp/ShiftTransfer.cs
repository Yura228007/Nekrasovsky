using System;
using System.Collections.Generic;

namespace server.ModelsTemp;

public partial class ShiftTransfer
{
    public int Id { get; set; }

    public int FromUserId { get; set; }

    public int ToUserId { get; set; }

    public DateTime TransferDate { get; set; }

    public bool IsConfirmed { get; set; }

    public virtual User FromUser { get; set; } = null!;

    public virtual User ToUser { get; set; } = null!;
}
