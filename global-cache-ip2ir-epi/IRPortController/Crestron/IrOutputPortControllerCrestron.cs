using Crestron.SimplSharpPro;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using System;

namespace global_cache_ip2ir_epi.IRPortController
{
    internal class IrOutputPortControllerCrestron: IrOutputPortController, IIrOutputPortController
    {
        public IrOutputPortControllerCrestron(string key, IIROutputPort port, string irDriverFilepath)
            : base(key, port as IROutputPort, irDriverFilepath)
        { }

        public IrOutputPortControllerCrestron(string key, Func<DeviceConfig, IROutputPort> postActivationFunc, DeviceConfig config)
            : base(key, postActivationFunc, config)
        { }
    }
}
