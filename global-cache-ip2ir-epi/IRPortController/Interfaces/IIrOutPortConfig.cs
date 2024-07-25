using Newtonsoft.Json;

namespace global_cache_ip2ir_epi.IRPortController
{
    public interface IIrOutPortConfig
    {
        [JsonProperty("port")]
        IIROutputPort Port { get; set; }

        [JsonProperty("fileName")]
        string FileName { get; set; }

        [JsonProperty("useBridgeJoinMap")]
        bool UseBridgeJoinMap { get; set; }

        // CrestronPort is defined because I can't figure out how to make it work wrapped into IIROutputPort
        //IROutputPort CrestronPort { get; set; }
    }
}
