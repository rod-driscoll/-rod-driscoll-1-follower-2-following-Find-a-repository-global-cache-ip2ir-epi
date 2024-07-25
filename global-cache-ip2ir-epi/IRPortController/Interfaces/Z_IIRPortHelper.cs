using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core.Config;

namespace global_cache_ip2ir_epi.IRPortController
{
    // IRPortHelper is static so can't use interfaces
    public interface Z_IIRPortHelper
    {
         string IrDriverPathPrefix { get; }
        IIrOutPortConfig GetIrPort(JToken propsToken);
        IIROutputPort GetIrOutputPort(DeviceConfig dc);
        IIrOutputPortController GetIrOutputPortController(DeviceConfig config);
    }
}
