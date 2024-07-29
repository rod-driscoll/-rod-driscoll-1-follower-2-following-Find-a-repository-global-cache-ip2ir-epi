using avit_essentials_common.IRPorts;
using Crestron.SimplSharpPro;
using System.Collections.Generic;

namespace global_cache_ip2ir_epi.IRPortController
{
    public class IROutputPortsAdvanced : IIROutputPortsAdvanced
    {
        // can't create or convert to CrestronCollection because the Crestron framework is shit
        // so using 2 collections and having to pick the right collection to read in other classes

        //private CrestronCollection<IROutputPort> _IROutputPorts;
        //private Dictionary<uint, IIROutputPort> _IROutputPortsDict;
        public CrestronCollection<IROutputPort> IROutputPorts { get; set; }
        /*
        { 
            get { return _IROutputPorts; } 
            set
            {
                 _IROutputPorts = value;
                if (_IROutputPorts != null && _IROutputPorts.Count > 0)
                {
                    var _value = new Dictionary<uint, IIROutputPort>();
                    for (uint i = 1; i <= _IROutputPorts.Count; i++)
                        _value.Add(1, _IROutputPorts[i]);
                    _IROutputPortsDict = _value;
                }
            }
        }
        */
        public Dictionary<int, IIROutputPort> IROutputPortsDict { get; set; }
        /*
        {
            get
            {
                if (_IROutputPorts != null && _IROutputPorts.Count > 0)
                {
                    var _value = new Dictionary<uint, IIROutputPort>();
                    for (uint i = 1; i <= _IROutputPorts.Count; i++)
                        _value.Add(1, _IROutputPorts[i]);
                    return _value;
                }
                else
                    return _IROutputPortsDict;
            }
            set 
            {
                _IROutputPortsDict = value; 
            }
        }
        */

        public int NumberOfIROutputPorts { get; set; }
    }
}
