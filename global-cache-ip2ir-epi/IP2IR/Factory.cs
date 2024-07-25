using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace global_cache_ip2ir_epi
{
    public class Factory: EssentialsPluginDeviceFactory<Device>
    {
        public Factory()
        {
            MinimumEssentialsFrameworkVersion = "1.15.5";
            TypeNames = new List<string>() { "ip2ir", "gc-ip2ir", "globalcache-ip2ir" };
        }
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "[{0}] Factory Attempting to create new device from type: {1}", dc.Key, dc.Type);
            //var props = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(dc.Properties.ToString());
            var props = dc.Properties.ToObject<Config>();
            if (props == null)
            {
                Debug.Console(0, "[{0}] Factory: failed to read properties config for {1}", dc.Key, dc.Name);
                return null;
            }
            var coms = CommFactory.CreateCommForDevice(dc);
            var device_ = new Device(dc.Key, dc.Name, props, coms);
            //Debug.Console(0, "[{0}] Factory {1} {2}", dc.Key, dc.Name, device_== null ? "== null" : "exists");
            return device_;
        }
    }
}
