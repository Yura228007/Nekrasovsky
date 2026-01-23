using System;
using System.Collections.Generic;

namespace server.ModelsTemp;

public partial class Material
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Code { get; set; }

    public string MeasuringUnit { get; set; } = null!;

    public virtual ICollection<AccessibleMovement> AccessibleMovements { get; set; } = new List<AccessibleMovement>();

    public virtual ICollection<FillingWarehouse> FillingWarehouses { get; set; } = new List<FillingWarehouse>();

    public virtual ICollection<PartRequest> PartRequests { get; set; } = new List<PartRequest>();

    public virtual Product? Product { get; set; }

    public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
}
