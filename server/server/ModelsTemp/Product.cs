using System;
using System.Collections.Generic;

namespace server.ModelsTemp;

public partial class Product
{
    public int Id { get; set; }

    public virtual Material IdNavigation { get; set; } = null!;

    public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
}
