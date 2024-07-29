using avit_essentials_common.IRPorts;
using Newtonsoft.Json;

namespace global_cache_ip2ir_epi.IRPortController
{

    /// <summary>
    /// Wrapper to help in IR port creation
    /// replaced IIROutputPort with IROutputPortAdvanced to allow third party IrPorts
    /// </summary>   
    public class IrOutPortConfigAdvanced : IIrOutPortConfig
    {

        //public IROutputPort CrestronPort { get; set; }
        [JsonProperty("port")]
        public IIROutputPort Port { get; set; }

        [JsonProperty("fileName")]
        public string FileName { get; set; }

        [JsonProperty("useBridgeJoinMap")]
        public bool UseBridgeJoinMap { get; set; }

        public IrOutPortConfigAdvanced()
        {
            FileName = "";
        }
    }
}