using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace global_cache_ip2ir_epi.IRPortController
{
    /// <summary>
    /// IRPortHelper modified to allow brand-inspecific IR ports
    /// I really tried to make it still support Crestron ports by wrapping everything in interfaces but it's impossible to decouple Crestron classes 
    /// </summary>
    public static class IRPortHelperAdvanced
    {
        public static string IrDriverPathPrefix
        {
            get
            {
                return Global.FilePathPrefix + "IR" + Global.DirectorySeparator;
            }
        }

        /// <summary>
        /// Finds either the ControlSystem or a device controller that contains IR ports and
        /// returns a port from the hardware device
        /// </summary>
        /// <param name="propsToken"></param>
        /// <returns>IrPortConfig object.  The port and or filename will be empty/null 
        /// if valid values don't exist on config</returns>
        public static IrOutPortConfigAdvanced GetIrPort(JToken propsToken)
        {
            Debug.Console(1, "GetIrPort not implemented");
            /*
            var control = propsToken["control"];
            if (control == null)
                return null;
            if (control["method"].Value<string>() != "ir")
            {
                Debug.Console(0, "IRPortHelper called with non-IR properties");
                return null;
            }

            var port = new IrOutPortConfigAdvanced(); // port, fileName, useBridgeJoinMap

            var portDevKey = control.Value<string>("controlPortDevKey");
            var portNum = control.Value<uint>("controlPortNumber");
            if (portDevKey == null || portNum == 0)
            {
                Debug.Console(1, "WARNING: Properties is missing port device or port number");
                return port;
            }

            var irDev = new IROutputPortsAdvanced();
            if (portDevKey.Equals("controlSystem", StringComparison.OrdinalIgnoreCase)
                || portDevKey.Equals("processor", StringComparison.OrdinalIgnoreCase))
            {
                var irTemp = Global.ControlSystem as IIROutputPorts; // IIROutputPorts is in Crestron namespace
                irDev.IROutputPorts = irTemp.IROutputPorts;
                irDev.NumberOfIROutputPorts = irTemp.NumberOfIROutputPorts;
                if (irDev.IROutputPorts == null)
                {
                    Debug.Console(1, "[Config] Error, device with IR ports '{0}' not found", portDevKey);
                    return port;
                }
                if (portNum <= irDev.NumberOfIROutputPorts) // success!
                {
                    var file = IrDriverPathPrefix + control["irFile"].Value<string>();
                    port.Port = irDev.IROutputPorts[portNum];
                    port.FileName = file;
                    return port; // new IrOutPortConfigAdvanced { Port = irDev.IROutputPorts[portNum], FileName = file };
                }
                else
                {
                    Debug.Console(1, "[Config] Error, device '{0}' IR port {1} out of range",
                        portDevKey, portNum);
                    return port;
                }
            }
            else
            {
                var irDev = DeviceManager.GetDeviceForKey(portDevKey) as IIROutputPortsAdvanced;
                if (irDev == null)
                {
                    Debug.Console(1, "[Config] Error, device with IR ports '{0}' not found", portDevKey);
                    return port;
                }
                if (portNum <= irDev.NumberOfIROutputPorts) // success!
                {
                    var file = IrDriverPathPrefix + control["irFile"].Value<string>();
                    port.Port = irDev.IROutputPorts[portNum];
                    port.FileName = file;
                    return port; // new IrOutPortConfigAdvanced { Port = irDev.IROutputPorts[portNum], FileName = file };
                }
                else
                {
                    Debug.Console(1, "[Config] Error, device '{0}' IR port {1} out of range",
                        portDevKey, portNum);
                    return port;
                }
            }
            */
            return null;
        }

        public static IIROutputPort GetIrOutputPort(DeviceConfig dc)
        {
            Debug.Console(0, "[{0}] GetIrOutputPort", dc.Key);
            //var irControllerKey = dc.Key + "-ir";
            if (dc.Properties == null)
            {
                Debug.Console(0, "[{0}] WARNING: Device config does not include properties.  IR will not function.", dc.Key);
                return null;
            }
            var control = dc.Properties["control"];
            if (control == null)
            {
                Debug.Console(0,
                    "WARNING: Device config does not include control properties.  IR will not function for {0}", dc.Key);
                return null;
            }
            var portDevKey = control.Value<string>("controlPortDevKey");
            if (portDevKey == null)
            {
                Debug.Console(0, "WARNING: control properties is missing ir device for {0}", dc.Key);
                return null;
            }
            var portNum = control.Value<uint>("controlPortNumber");
            if (portNum == 0)
            {
                Debug.Console(0, "WARNING: control properties is missing ir port number for {0}", dc.Key);
                return null;
            }

            var irDev = new IROutputPortsAdvanced();

            if (portDevKey.Equals("controlSystem", StringComparison.OrdinalIgnoreCase)
                || portDevKey.Equals("processor", StringComparison.OrdinalIgnoreCase))
            {
                irDev.IROutputPorts = Global.ControlSystem.IROutputPorts;
                if (irDev.IROutputPorts == null)
                {
                    Debug.Console(0, "WARNING: device with IR ports '{0}' not found", portDevKey);
                    return null;
                }
                if (portNum > irDev.NumberOfIROutputPorts)
                {
                    Debug.Console(0, "WARNING: device '{0}' IR port {1} out of range",
                        portDevKey, portNum);
                    return null;
                }
                Debug.Console(0, "WARNING: device '{0}' IR port {1} is crestron port, can't convert to third party",
                    portDevKey, portNum);
                //return irDev.IROutputPorts[portNum];
                return null;
            }
            else // this is the ip2ir device defined in the Factory
            {   
                var dev_ = DeviceManager.GetDeviceForKey(portDevKey) as Device;
                if (dev_ == null)
                {
                    Debug.Console(0, "WARNING: device '{0}' not found", portDevKey);
                    return null;
                }
                var ports_ = dev_ as IIROutputPortsAdvanced;
                if(ports_ == null)
                {
                    Debug.Console(0, "WARNING: device '{0}' ports not found", portDevKey);
                    return null;
                }
                irDev.IROutputPortsDict = ports_.IROutputPortsDict;
                if (irDev.IROutputPortsDict == null)
                {
                    Debug.Console(0, "WARNING: device with IR ports '{0}' not found", portDevKey);
                    return null;
                }
                irDev.NumberOfIROutputPorts = ports_.NumberOfIROutputPorts;
                if (!irDev.IROutputPortsDict.ContainsKey((int)portNum))
                {
                    Debug.Console(0, "WARNING: device '{0}' IR port {1} out of range",
                        portDevKey, portNum);
                    return null;
                }
                return irDev.IROutputPortsDict[(int)portNum];
            }
        }

        public static IROutputPort GetCretronIrOutputPort(DeviceConfig dc)
        {
            var irControllerKey = dc.Key + "-ir";
            if (dc.Properties == null)
            {
                Debug.Console(0, "[{0}] WARNING: Device config does not include properties.  IR will not function.", dc.Key);
                return null;
            }

            var control = dc.Properties["control"];
            if (control == null)
            {
                Debug.Console(0,
                    "WARNING: Device config does not include control properties.  IR will not function for {0}", dc.Key);
                return null;
            }

            var portDevKey = control.Value<string>("controlPortDevKey");
            var portNum = control.Value<uint>("controlPortNumber");
            IIROutputPorts irDev = null;

            if (portDevKey == null)
            {
                Debug.Console(0, "WARNING: control properties is missing ir device for {0}", dc.Key);
                return null;
            }

            if (portNum == 0)
            {
                Debug.Console(0, "WARNING: control properties is missing ir port number for {0}", dc.Key);
                return null;
            }

            if (portDevKey.Equals("controlSystem", StringComparison.OrdinalIgnoreCase)
                || portDevKey.Equals("processor", StringComparison.OrdinalIgnoreCase))
                irDev = Global.ControlSystem;
            else
                irDev = DeviceManager.GetDeviceForKey(portDevKey) as IIROutputPorts;

            if (irDev == null)
            {
                Debug.Console(0, "WARNING: device with IR ports '{0}' not found", portDevKey);
                return null;
            }
            if (portNum > irDev.NumberOfIROutputPorts)
            {
                Debug.Console(0, "WARNING: device '{0}' IR port {1} out of range",
                    portDevKey, portNum);
                return null;
            }

            IROutputPort port = irDev.IROutputPorts[portNum];

            return port;
        }

        public static IIrOutputPortController GetIrOutputPortController(DeviceConfig config)
        {
            Debug.Console(1, "Attempting to create new Ir Port Controller");

            if (config == null)
            {
                return null;
            }

            IIrOutputPortController irDevice = null;
            var control = config.Properties["control"];
            if (control != null)
            {
                var method = control.Value<string>("method");
                if (method != null)
                    if (method.Equals("ir", StringComparison.OrdinalIgnoreCase))
                    {
                         // Crestron IR port
                        Debug.Console(1, "Attempting to create new Crestron Ir Port Controller");
                        var crestronPostActivationFunc = new Func<DeviceConfig, IROutputPort>(GetCretronIrOutputPort);
                        irDevice = new IrOutputPortControllerCrestron(config.Key + "-ir", crestronPostActivationFunc, config);
                        return irDevice;
                    }
            }

            // Not a crestron IR port
            Debug.Console(1, "Attempting to create new non-native Ir Port Controller");
            var postActivationFunc = new Func<DeviceConfig, IIROutputPort>(GetIrOutputPort);
            irDevice = new IrOutputPortControllerIP2IR(config.Key + "-ir", postActivationFunc, config); // e.g. Key=="stb-1-ir"
            //irDevice = new IrOutputPortControllerIP2IR("key", irport, filepath) // other optional ctor
            return irDevice;
        }

        /*
        /// <summary>
        /// Returns a ready-to-go IrOutputPortController from a DeviceConfig object.
        /// </summary>	
        public static IrOutputPortController GetIrOutputPortController(DeviceConfig devConf)
        {
            var irControllerKey = devConf.Key + "-ir";
            if (devConf.Properties == null)
            {
                Debug.Console(0, "[{0}] WARNING: Device config does not include properties.  IR will not function.", devConf.Key);
                return new IrOutputPortController(irControllerKey, null, "");
            }

            var control = devConf.Properties["control"];
            if (control == null)
            {
                var c = new IrOutputPortController(irControllerKey, null, "");
                Debug.Console(0, c, "WARNING: Device config does not include control properties.  IR will not function");
                return c;
            }

            var portDevKey = control.Value<string>("controlPortDevKey");
            var portNum = control.Value<uint>("controlPortNumber");
            IIROutputPorts irDev = null;

            if (portDevKey == null)
            {
                var c = new IrOutputPortController(irControllerKey, null, "");
                Debug.Console(0, c, "WARNING: control properties is missing ir device");
                return c;
            }

            if (portNum == 0)
            {
                var c = new IrOutputPortController(irControllerKey, null, "");
                Debug.Console(0, c, "WARNING: control properties is missing ir port number");
                return c;
            } 

            if (portDevKey.Equals("controlSystem", StringComparison.OrdinalIgnoreCase)
                || portDevKey.Equals("processor", StringComparison.OrdinalIgnoreCase))
                irDev = Global.ControlSystem;
            else
                irDev = DeviceManager.GetDeviceForKey(portDevKey) as IIROutputPorts;

            if (irDev == null)
            {
                var c = new IrOutputPortController(irControllerKey, null, "");
                Debug.Console(0, c, "WARNING: device with IR ports '{0}' not found", portDevKey);
                return c;
            }

            if (portNum <= irDev.NumberOfIROutputPorts) // success!
                return new IrOutputPortController(irControllerKey, irDev.IROutputPorts[portNum],
                    IrDriverPathPrefix + control["irFile"].Value<string>());
            else
            {
                var c = new IrOutputPortController(irControllerKey, null, "");
                Debug.Console(0, c, "WARNING: device '{0}' IR port {1} out of range",
                    portDevKey, portNum);
                return c;
            }
        }*/
    }
}