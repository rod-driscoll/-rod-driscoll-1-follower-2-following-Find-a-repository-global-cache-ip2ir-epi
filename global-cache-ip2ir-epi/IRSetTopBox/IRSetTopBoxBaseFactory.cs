using global_cache_ip2ir_epi.IRPortController;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Devices.Common;
using System.Collections.Generic;

namespace global_cache_ip2ir_epi.IRSetTopBox
{
    public class IRSetTopBoxBaseFactory : EssentialsPluginDeviceFactory<IRSetTopBoxBaseAdvanced>
    {
        public IRSetTopBoxBaseFactory()
        {
            MinimumEssentialsFrameworkVersion = "1.15.5";
            TypeNames = new List<string>() { "stb-advanced", "settopbox-advanced" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new SetTopBox Device");
            var irCont = IRPortHelperAdvanced.GetIrOutputPortController(dc); // e.g. irCont.Key=="stb-1-ir"
            var config = dc.Properties.ToObject<SetTopBoxPropertiesConfig>();
            var stb = new IRSetTopBoxBaseAdvanced(dc.Key, dc.Name, irCont, config);

            var listName = dc.Properties.Value<string>("presetsList");
            if (listName != null)
                stb.LoadPresets(listName);
            return stb;

        }
    }
}
