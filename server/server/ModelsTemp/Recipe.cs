using System;
using System.Collections.Generic;

namespace server.ModelsTemp;

public partial class Recipe
{
    public int ProductId { get; set; }

    public int MaterialId { get; set; }

    public int Quantity { get; set; }

    public string? MeasuringType { get; set; }

    public virtual Material Material { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
