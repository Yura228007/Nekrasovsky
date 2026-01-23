using System;
using System.Collections.Generic;

namespace server.ModelsTemp;

public partial class PartRequest
{
    public int Id { get; set; }

    public int FromUserId { get; set; }

    public int ToUserId { get; set; }

    public int FromWarehouseId { get; set; }

    public int ToWarehouseId { get; set; }

    public int MaterialId { get; set; }

    public int Quantity { get; set; }

    public string? MeasuringType { get; set; }

    public DateTime CreatedAt { get; set; }

    public int Status { get; set; }

    public virtual User FromUser { get; set; } = null!;

    public virtual Warehouse FromWarehouse { get; set; } = null!;

    public virtual Material Material { get; set; } = null!;

    public virtual User ToUser { get; set; } = null!;

    public virtual Warehouse ToWarehouse { get; set; } = null!;
}
