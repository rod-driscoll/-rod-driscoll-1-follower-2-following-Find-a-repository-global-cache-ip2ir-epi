using Crestron.SimplSharpPro;
using System.Collections.Generic;

namespace global_cache_ip2ir_epi.IRPortController
{
    public interface IIROutputPortsAdvanced: IIROutputPorts
    {
        //
        // Summary:
        //     Collection of IR output ports on the device.
        Dictionary<int, IIROutputPort> IROutputPortsDict { get; set; }
    }
}
