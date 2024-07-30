﻿using avit_essentials_common.IRPorts;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;
using System;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Modified version of IrOutputPortController with interface so it can be used with non-native IR ports
    /// IR port wrapper. May act standalone
    /// </summary>
    public class IrOutputPortControllerIP2IR : Device, IIrOutputPortController
    {
        uint IrPortUid;
        public IIROutputPort IrPort { get; private set; }
        
        public BoolFeedback DriverLoaded { get; private set; }

        public ushort StandardIrPulseTime { get; set; }
        public string DriverFilepath { get; private set; }
        public bool DriverIsLoaded { get; private set; }

        public string[] IrFileCommands { get { return IrPort.AvailableStandardIRCmds(IrPortUid); } }

        public bool UseBridgeJoinMap { get; private set; }

        /// <summary>
        /// Constructor for IrDevice base class.  If a null port is provided, this class will 
        /// still function without trying to talk to a port.
        /// </summary>
        public IrOutputPortControllerIP2IR(string key, IIROutputPort port, string irDriverFilepath)
            : base(key)
        {
            //if (port == null) throw new ArgumentNullException("port");

            DriverLoaded = new BoolFeedback(() => DriverIsLoaded);
            IrPort = port;
            if (port == null)
            {
                Debug.Console(0, this, "WARNING No valid IR Port assigned to controller. IR will not function");
                return;
            }
            LoadDriver(irDriverFilepath);
        }

        public IrOutputPortControllerIP2IR(string key, Func<DeviceConfig, IIROutputPort> postActivationFunc,
            DeviceConfig config)
            : base(key)
        {
            Debug.Console(1, this, "IrOutputPortControllerIP2IR constructor, key: {0}", key);
            // e.g. stb-advanced-ir
            DriverLoaded = new BoolFeedback(() => DriverIsLoaded);
            UseBridgeJoinMap = config.Properties["control"].Value<bool>("useBridgeJoinMap");
            AddPostActivationAction(() =>
            {
                Debug.Console(1, this, "IrOutputPortControllerIP2IR PostActivationAction");
                IrPort = postActivationFunc(config);

                if (IrPort == null)
                {
                    Debug.Console(0, this, "WARNING No valid IR Port assigned to controller. IR will not function");
                    return;
                }
                var filePath = config.Properties["control"]["irFile"].Value<string>();
                if(!filePath.Contains("\\"))
                    filePath = Global.FilePathPrefix + "ir" + Global.DirectorySeparator + filePath;
                
                Debug.Console(1, "*************Attempting to load IR file: {0}***************", filePath);

                LoadDriver(filePath);

                PrintAvailableCommands();
            });
            Debug.Console(1, this, "{0} IrOutputPortControllerIP2IR constructor done", key);
        }

        public void PrintAvailableCommands()
        {
            var cmds_ = IrPort.AvailableIRCmds(1);
            if(cmds_ != null)
            {
                Debug.Console(2, this, "{0} Available IR {1} File commands:", cmds_.Length, IrPortUid);
                foreach (var cmd in cmds_)
                    Debug.Console(2, this, cmd);
            }
        }

        /// <summary>
        /// Loads the IR driver at path
        /// </summary>
        /// <param name="path"></param>
        public void LoadDriver(string path)
        {
            Debug.Console(2, this, "***Loading IR File***");
            if (string.IsNullOrEmpty(path)) path = DriverFilepath;
            try
            {
                IrPortUid = IrPort.LoadIRDriver(path);
                DriverFilepath = path;
                StandardIrPulseTime = 200;
                DriverIsLoaded = true;

                DriverLoaded.FireUpdate();
                //Debug.Console(2, this, "***IR {0} Files loaded***", IrPort.IRDriversLoadedCount);
            }
            catch
            {
                DriverIsLoaded = false;
                var message = string.Format("WARNING IR Driver '{0}' failed to load", path);
                Debug.Console(0, this, Debug.ErrorLogLevel.Error, message);
                DriverLoaded.FireUpdate();
            }
        }


        /// <summary>
        /// Starts and stops IR command on driver. Safe for missing commands
        /// </summary>
        public virtual void PressRelease(string command, bool state)
        {
            Debug.Console(2, this, "IR:'{0}'={1}", command, state);
            if (IrPort == null)
            {
                Debug.Console(2, this, "WARNING No IR Port assigned to controller");
                return;
            }
            if (!DriverIsLoaded)
            {
                Debug.Console(2, this, "WARNING IR driver is not loaded");
                return;
            }
            if (state)
            {
                if (IrPort.IsIRCommandAvailable(IrPortUid, command))
                    IrPort.Press(IrPortUid, command);
                else
                    NoIrCommandError(command);
            }
            else
                IrPort.Release();
        }

        /// <summary>
        /// Pulses a command on driver. Safe for missing commands
        /// </summary>
        public virtual void Pulse(string command, ushort time)
        {
            if (IrPort == null)
            {
                Debug.Console(2, this, "WARNING No IR Port assigned to controller");
                return;
            }
            if (!DriverIsLoaded)
            {
                Debug.Console(2, this, "WARNING IR driver is not loaded");
                return;
            }
            if (IrPort.IsIRCommandAvailable(IrPortUid, command))
                IrPort.PressAndRelease(IrPortUid, command, time);
            else
                NoIrCommandError(command);
        }

        /// <summary>
        /// Notifies the console when a bad command is used.
        /// </summary>
        protected void NoIrCommandError(string command)
        {
            Debug.Console(2, this, "Device {0}: IR Driver {1} does not contain command {2}",
                Key, IrPort.IRDriverFileNameByIRDriverId(IrPortUid), command);
        }
    }
}