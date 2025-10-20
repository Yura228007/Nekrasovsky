using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekrasovskyAPP.Models
{
    public class Recipe
    {
        public int ProductId { get; set; }
        public int MaterialId { get; set; }
        public int Quantity { get; set; }
        public string? MeasuringType { get; set; }
    }
}
