using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PasswordManagerV1.Models;

namespace NekrasovskyAPP.Models
{
    public class Product : Material
    {
        public string? Recipe { get; set; }
    }
}
