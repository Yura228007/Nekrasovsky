using System;
using System.Collections.Generic;

namespace server.ModelsTemp;

public partial class AccessibleMovement
{
    public int FromWarehouseId { get; set; }

    public int ToWarehouseId { get; set; }

    public int MaterialId { get; set; }

    public virtual Warehouse FromWarehouse { get; set; } = null!;

    public virtual Material Material { get; set; } = null!;

    public virtual Warehouse ToWarehouse { get; set; } = null!;
}
