using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekrasovskyAPP.Services
{
    public interface IFlashlightService
    {
        bool IsSupported { get; }
        Task<bool> ToggleAsync();
        Task TurnOffAsync();
    }
}
