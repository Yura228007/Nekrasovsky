using System;
using System.Collections.Generic;

namespace server.ModelsTemp;

public partial class Warehouse
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Type { get; set; } = null!;

    public virtual ICollection<AccessibleMovement> AccessibleMovementFromWarehouses { get; set; } = new List<AccessibleMovement>();

    public virtual ICollection<AccessibleMovement> AccessibleMovementToWarehouses { get; set; } = new List<AccessibleMovement>();

    public virtual ICollection<FillingWarehouse> FillingWarehouses { get; set; } = new List<FillingWarehouse>();

    public virtual ICollection<PartRequest> PartRequestFromWarehouses { get; set; } = new List<PartRequest>();

    public virtual ICollection<PartRequest> PartRequestToWarehouses { get; set; } = new List<PartRequest>();
}
