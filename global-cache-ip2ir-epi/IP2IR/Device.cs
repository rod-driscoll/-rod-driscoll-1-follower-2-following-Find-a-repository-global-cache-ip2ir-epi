using avit_essentials_common.IRPorts;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Queues;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Thread = Crestron.SimplSharpPro.CrestronThread.Thread;

namespace global_cache_ip2ir_epi
{
    // iTach default DHCP, link local fallback: http://169.254.1.70
    // use iHelp to find on network

    public class ChannelCallbackObject
    {
        public uint Port { get; private set; }
        public uint Channel { get; private set; }
        public ChannelCallbackObject(uint port, uint channel)
        {
            Port = port;
            Channel = channel;
        }
    }

    /// <summary>
    /// Using IQueue to parse a string via Enqueue method, then deserialising into IRMethodProps
    /// </summary>
    public class IRMethodProps
    {
        [JsonProperty("method")]
        public string Method { get; set; }
        [JsonProperty("command")]
        public string Command { get; set; }
        [JsonProperty("port")]
        public uint Port { get; set; }
        [JsonProperty("repeat")]
        public uint Repeat { get; set; }

        /// <summary>
        /// Constuctor
        /// </summary>
        /// <remarks>
        /// If using a collection you must instantiate the collection in the constructor
        /// to avoid exceptions when reading the configuration file 
        /// </remarks>
        public IRMethodProps()
        {

        }
    }
    public class Device : EssentialsDevice,
        IOnline, ICommunicationMonitor, IDisposable, IIROutputPortsAdvanced, IQueue<string> // IQueue is a way to get the cmmands into the epi without having a defined interface
        //, IHasPowerControl, IChannel, IColor, IDPad, ISetTopBoxNumericKeypad, ITransport //, ISetTopBoxControls
        
    {
        #region variables
        public uint LogLevel { get; set; }
        public uint DefaultTcpPort { get; set; }
        public uint DefaultIRRepeat { get; set; }
        public Config config { get; private set; }
        int tcpBasePort = 4999;

        private CTimer _pollTimer;
        private const int _pollTime = 6000;

        public BoolFeedback IsOnline
        {
            get { return CommunicationMonitor.IsOnlineFeedback; }
        }
        public StatusMonitorBase CommunicationMonitor { get; private set; }
        public bool Disposed { get { return _pollTimer != null; } }

        public Dictionary<int, IIROutputPort> IROutputPortsDict { get; set; }
        public CrestronCollection<IROutputPort> IROutputPorts { get; } // not to be used, part of IIROutputPortsAdvanced
        public int NumberOfIROutputPorts { get; private set; }
        int defaultNumberOfIROutputPorts = 3;

        private readonly IBasicCommunication _coms;
        private readonly GenericQueue _commandQueue;

        private static Dictionary<int, string> errorCodes = new Dictionary<int, string>()
        {
            { 1, "Invalid command. Command not found" },
            { 2, "Invalid module address (does not exist)" },
            { 3, "Invalid connector address (does not exist)" },
            { 4, "Invalid ID value" },
            { 5, "Invalid frequency value" },
            { 6, "Invalid repeat value" },
            { 7, "Invalid offset value" },
            { 8, "Invalid pulse count" },
            { 9, "Invalid pulse data" },
            { 10, "Uneven amount of <on|off> statements" },
            { 11, "No carriage return found" },
            { 12, "Repeat count exceeded" },
            { 13, "IR command sent to input connector" },
            { 14, "Blaster command sent to non-blaster connector" },
            { 15, "No carriage return before buffer full" },
            { 16, "No carriage return" },
            { 17, "Bad command syntax" },
            { 18, "Sensor command sent to non-input connector" },
            { 19, "Repeated IR transmission failure" },
            { 20, "Above designated IR <on|off> pair limit" },
            { 21, "Symbol odd boundary" },
            { 22, "Undefined symbol" },
            { 23, "Unknown option" },
            { 24, "Invalid baud rate setting" },
            { 25, "Invalid flow control setting" },
            { 26, "Invalid parity setting" },
            { 27, "Settings are locked" }
        };

        #endregion variables

        public Device(string key, string name, Config config, IBasicCommunication coms)
            : base(key, name)
        {
            Debug.Console(1, this, "Constructor starting");
            _coms = coms;
            this.config = config;
            this.config.PulseTime = this.config.PulseTime == 0 ? 200 : this.config.PulseTime;
            //this.config.Control.TcpSshProperties.Port = this.config.Control.TcpSshProperties.Port == 0 ? tcpBasePort : this.config.Control.TcpSshProperties.Port; -- this won't work, need to set port in coms on creation
            
            DefaultTcpPort = (uint)tcpBasePort; //this.config.Control.ControlPortNumber == 0 ? 1 : this.config.Control.ControlPortNumber;
            if (this.config.Monitor == null)
                this.config.Monitor = GetDefaultMonitorConfig();
            CommunicationMonitor = new GenericCommunicationMonitor(this, _coms, this.config.Monitor);
            var gather = new CommunicationGather(_coms, "\x0D");
            _commandQueue = new GenericQueue(key + "-command-queue", 213, Thread.eThreadPriority.MediumPriority, 50);
            new StringResponseProcessor(gather, s => { ProcessResponse(s); });
            CrestronEnvironment.ProgramStatusEventHandler += type =>
            {
                if (type != eProgramStatusEventType.Stopping) return;
                if (_pollTimer == null) return;
                _pollTimer.Stop();
                _pollTimer.Dispose();
            };

            DefaultIRRepeat = this.config.Repeat == 0 ? 1 : this.config.Repeat;
            NumberOfIROutputPorts = defaultNumberOfIROutputPorts; // this could be set by polling the device
            IROutputPortsDict = new Dictionary<int, IIROutputPort>();
            for (int i = 1; i <= NumberOfIROutputPorts; i++)
                IROutputPortsDict.Add(i, new IROutputPortIP2IR(i, (int)DefaultIRRepeat, SendCommand));
        }

        #region methods

        private static CommunicationMonitorConfig GetDefaultMonitorConfig()
        {
            return new CommunicationMonitorConfig()
            {
                PollInterval = 30000,
                PollString = Commands.QueryDevice,
                TimeToWarning = 120000,
                TimeToError = 360000,
            };
        }

        void CommunicationMonitor_StatusChange(object sender, MonitorStatusChangeEventArgs e)
        {
            Debug.Console(0, this, "CommunicationMonitor_StatusChange: {0} - {1}", e.Status, e.Message);
        }

        public override bool CustomActivate()
        {
            _pollTimer = new CTimer(o =>
            {
                //Debug.Console(2, this, "Polling, IsOnline: {0}, Status: {1}, IsConnected: {2}, ", CommunicationMonitor.IsOnlineFeedback.BoolValue, CommunicationMonitor.Status, _coms.IsConnected);
                if (!CommunicationMonitor.IsOnlineFeedback.BoolValue)
                {
                    CommunicationMonitor.Stop();
                    CommunicationMonitor.Start();
                }
                if (!_coms.IsConnected)
                    _coms.Connect();

                //_commandQueue.Enqueue(new Commands.Command { Coms = _coms, Message = Commands.QueryDevice });

            }, null, 5189, _pollTime);

            CommunicationMonitor.StatusChange += new EventHandler<MonitorStatusChangeEventArgs>(CommunicationMonitor_StatusChange);
            CommunicationMonitor.Start();
            if (!_coms.IsConnected)
                _coms.Connect();
            Debug.Console(1, this, "CommunicationMonitor {0} Start, IsOnline: {1}", CommunicationMonitor.Key, CommunicationMonitor.IsOnlineFeedback.BoolValue);
            var device_ = DeviceManager.GetDeviceForKey(CommunicationMonitor.Key);
            if (device_ != null)
                Debug.Console(2, this, "CommunicationMonitor key: {0}", device_.Key);

            return base.CustomActivate();
        }

        private void ProcessResponse(string response)
        {
            Debug.Console(1, this, "ParseRx: {0}", response.Printable(false));
            Match m = Regex.Match(response, @"ERR_(\d*):(\d+),(\d+)");
            if (m.Success)
            {
                Debug.Console(1, this, "ERROR: {0}, {1}", m.Groups[3].Value, errorCodes[StringExtensions.Atoi(m.Groups[3].Value)]);
                // m.Groups[2].Value =
                // ERR_0:0,002 // module does not exist
                // ERR_1:2,010 // not an equal number of <on> and <off>
                // ERR_1:3,007 // <offset> is an even number
            }
            else if (response.Contains("completeir"))
            { // completeir,1:1,<ID>
                //busy = false;
            }
            // stopir,1:1\x0D
            // busyir,1:1\x0D
            // completeir,1:2,2445\x0D
        }
        public void SendCommand(string command)
        {
            Debug.Console(1, this, "SendCommand: {0}", command);
            _commandQueue.Enqueue(new Commands.Command
            {
                Coms = _coms,
                Message = command,
            });
        }

        public string MakeCommand(string command, int module, int port, string data)
        {
            var data_ = String.IsNullOrEmpty(data) ? String.Empty : String.Format(",{0}", data);
            var msg_ = String.Format("{0},{1}:{2}{3}", command, module, port, data_);
            return msg_;
        }
        public string MakeCommand(string command, int module, int port)
        {
            var msg_ = MakeCommand(command, module, port, String.Empty);
            return msg_;
        }

        public void GetDevices()
        {
            SendCommand("getdevices"); // Rx: "device,0,0 ETHERNET\x0Ddevice,1,3 IR\x0Dendlistdevices\x0D"
        }
        public void GetVersion()
        {
            SendCommand("getversion"); // Rx: "710-1005-05\x0d"
        }
        public void GetNetworkDetails()
        {
            MakeCommand("get_NET",0,1); //"get_NET,0:1\n"
            //Rx: "NET,0:1,UNLOCKED,DHCP,192.168.104.111,255.255.255.0,192.168.104.1\x0D"
        }
        public void GetIRPortSetup(int module, int port)
        {
            MakeCommand("get_IR", module, port); //"get_IR,1:3\n"
            // Rx: "IR,1:3,IR\x0d"
        }
        public void SetupIRPort(int module, int port)
        {
            MakeCommand("set_IR", module, port, "IR"); //"set_IR,1:3,IR\n"
            // Rx: "IR,1:3,IR\x0d"
        }
        public void Dispose()
        {
            Debug.Console(1, this, "Dispose");
            if (_pollTimer != null)
            {
                _pollTimer.Stop();
                _pollTimer.Dispose();
                _pollTimer = null;
            }
        }

        public void Enqueue(string item)
        {
            try
            {
                IRMethodProps props = JsonConvert.DeserializeObject<IRMethodProps>(item);
                switch(props.Method)
                {
                    //case "Press"  : Press(  Convert.ToUInt32(props.Port), props.Command, Convert.ToUInt32(props.Repeat)); break;
                    //case "Release": Release(Convert.ToUInt32(props.Port)); break;
                    //case "Channel": SetChannel(Convert.ToUInt32(props.Port), Convert.ToUInt32(props.Command)); break;
                    case "Command": SendCommand(props.Command); break;
                }
            }
            catch (Exception e)
            {
                Debug.Console(1, this, "Enqueue ERROR: {0}", e.Message);
            }

        }

        /*
        #region IIrOutputPortController
        public void PrintAvailableCommands()
        {
            Debug.Console(2, this, "Available IR Commands in IR File {0}", this.DriverFilepath);
            foreach (var cmd in IrPort.AvailableIRCmds())
            {
                Debug.Console(2, this, "{0}", cmd);
            }
        }

        #endregion

        #region ISetTopBoxControls Members

        public void DvrList(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_DVR, pressRelease, DefaultIRRepeat);
        }
        public void Replay(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_REPLAY, pressRelease, DefaultIRRepeat);
        }
        #endregion
        #region IDPad Members

        public void Up(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_UP_ARROW, pressRelease, DefaultIRRepeat);
        }
        public void Down(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_DN_ARROW, pressRelease, DefaultIRRepeat);
        }
        public void Left(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_LEFT_ARROW, pressRelease);
        }
        public void Right(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_RIGHT_ARROW, pressRelease, DefaultIRRepeat);
        }
        public void Select(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_ENTER, pressRelease, DefaultIRRepeat);
        }
        public void Menu(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_MENU, pressRelease);
        }
        public void Exit(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_EXIT, pressRelease, DefaultIRRepeat);
        }

        #endregion
        #region INumericKeypad Members

        public void Digit0(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_0, pressRelease, DefaultIRRepeat);
        }
        public void Digit1(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_1, pressRelease, DefaultIRRepeat);
        }
        public void Digit2(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_2, pressRelease, DefaultIRRepeat);
        }
        public void Digit3(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_3, pressRelease, DefaultIRRepeat);
        }
        public void Digit4(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_4, pressRelease, DefaultIRRepeat);
        }
        public void Digit5(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_5, pressRelease, DefaultIRRepeat);
        }
        public void Digit6(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_6, pressRelease, DefaultIRRepeat);
        }
        public void Digit7(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_7, pressRelease, DefaultIRRepeat);
        }
        public void Digit8(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_8, pressRelease, DefaultIRRepeat);
        }
        public void Digit9(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_9, pressRelease, DefaultIRRepeat);
        }
        public string KeypadAccessoryButton1Command { get; set; }
        public void KeypadAccessoryButton1(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, KeypadAccessoryButton1Command, pressRelease, DefaultIRRepeat);
        }
        public string KeypadAccessoryButton2Command { get; set; }

        public bool HasKeypadAccessoryButton1 => throw new NotImplementedException();

        public string KeypadAccessoryButton1Label => throw new NotImplementedException();

        public bool HasKeypadAccessoryButton2 => throw new NotImplementedException();

        public string KeypadAccessoryButton2Label => throw new NotImplementedException();

        public void KeypadAccessoryButton2(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, KeypadAccessoryButton2Command, pressRelease, DefaultIRRepeat);
        }

        #endregion
        #region ISetTopBoxNumericKeypad Members

        /// <summary>
        /// Corresponds to "dash" IR command
        /// </summary>
        public void Dash(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, "dash", pressRelease, DefaultIRRepeat);
        }
        /// <summary>
        /// Corresponds to "numericEnter" IR command
        /// </summary>
        public void KeypadEnter(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, "enter", pressRelease, DefaultIRRepeat);
        }

        #endregion
        #region IChannelFunctions Members

        public void ChannelUp(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_CH_PLUS, pressRelease, DefaultIRRepeat);
        }

        public void ChannelDown(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_CH_MINUS, pressRelease, DefaultIRRepeat);
        }

        public void LastChannel(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_LAST, pressRelease, DefaultIRRepeat);
        }

        public void Guide(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_GUIDE, pressRelease, DefaultIRRepeat);
        }

        public void Info(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_INFO, pressRelease, DefaultIRRepeat);
        }

        #endregion
        #region IColorFunctions Members

        public void Red(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_RED, pressRelease, DefaultIRRepeat);
        }
        public void Green(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_GREEN, pressRelease, DefaultIRRepeat);
        }
        public void Yellow(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_YELLOW, pressRelease, DefaultIRRepeat);
        }
        public void Blue(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_BLUE, pressRelease, DefaultIRRepeat);
        }

        #endregion
        #region ITransport Members

        public void ChapMinus(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_CH_MINUS, pressRelease, DefaultIRRepeat);
        }
        public void ChapPlus(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_CH_PLUS, pressRelease, DefaultIRRepeat);
        }
        public void FFwd(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_FSCAN, pressRelease, DefaultIRRepeat);
        }
        public void Pause(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_RSCAN, pressRelease, DefaultIRRepeat);
        }
        public void Play(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_PLAY, pressRelease, DefaultIRRepeat);
        }
        public void Record(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_RECORD, pressRelease, DefaultIRRepeat);
        }
        public void Rewind(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_RSCAN, pressRelease, DefaultIRRepeat);
        }
        public void Stop(bool pressRelease)
        {
            PressRelease(DefaultTcpPort, IROutputStandardCommands.IROut_STOP, pressRelease, DefaultIRRepeat);
        }

        #endregion
        #region IPower Members

        public void PowerOn()
        {
            Press(DefaultTcpPort, IROutputStandardCommands.IROut_POWER_ON, DefaultIRRepeat);
        }
        public void PowerOff()
        {
            Press(DefaultTcpPort, IROutputStandardCommands.IROut_POWER_OFF, DefaultIRRepeat);
        }
        public void PowerToggle()
        {
            Press(DefaultTcpPort, IROutputStandardCommands.IROut_POWER, DefaultIRRepeat);
        }

        #endregion    
        */
        
        #endregion methods
    }
}

