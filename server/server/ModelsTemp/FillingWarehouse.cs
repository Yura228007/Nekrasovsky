using System;
using System.Collections.Generic;

namespace server.ModelsTemp;

public partial class FillingWarehouse
{
    public int WarehouseId { get; set; }

    public int MaterialId { get; set; }

    public int Quantity { get; set; }

    public string? MeasuringType { get; set; }

    public virtual Material Material { get; set; } = null!;

    public virtual Warehouse Warehouse { get; set; } = null!;
}
