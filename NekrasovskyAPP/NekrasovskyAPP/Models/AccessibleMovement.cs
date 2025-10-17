using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekrasovskyAPP.Models
{
    public class AccessibleMovement
    {
        public int FromWarehouseId { get; set; }
        public int ToWarehouseId { get;set; }
        public int MaterialId { get; set; }

    }
}
