using avit_essentials_common.interfaces;
using avit_essentials_common.IRPorts;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using PepperDash.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace global_cache_ip2ir_epi
{
    public class IROutputPortIP2IR : IIROutputPort, ILogClassDetails
    {
        public string ClassName { get; private set; }
        public uint LogLevel { get; set; }
        public int IRDriversLoadedCount { get { return IRDrivers.Count; } }
        int port;
        public int IRDefaultRepeat { get; set; }

        Dictionary<int, IP2IRDriver> IRDrivers;

        public delegate void SendStringDelegate(string str); // this is subscribed by global_cache_ip2ir_epi.Device
        SendStringDelegate sendString;

        public IROutputPortIP2IR(int port, int repeat, SendStringDelegate dele)
        {
            this.port = port;
            IRDefaultRepeat = repeat;
            this.sendString = dele;
            ClassName = String.Format(ClassName + "IP2IR-port-{0}", this.port);
            IRDrivers = new Dictionary<int, IP2IRDriver>();
        }
        public uint LoadIRDriver(string IRFileName)
        {
            // e.g. "\\html\\presets\\lists\\Digitel Terestrial Set Top Box.ccf";
            Debug.Console(1, "{0} LoadIRDriver file: {1}", ClassName, IRFileName);
            try
            {
                var first_ = IRDrivers.FirstOrDefault(x => x.Value.Filename == IRFileName);
                //var index_ = first_.Equals(default(Dictionary<int, IP2IRDriver>)) ? IRDriversLoadedCount + 1 : first_.Key;
                var index_ = 1;
                if(first_.Key == 0) // not in Dict
                {
                    foreach(var ir_ in IRDrivers)
                    {
                        if (!IRDrivers.ContainsKey(index_))
                            break;
                        index_++;
                    }
                }
                else // in dict, overwrite
                    index_ = first_.Key;

                //Debug.Console(2, "{0} LoadIRDriver IRDriverID: {1}", ClassName, index_);
                var dict_ = IP2IROperations.ReadIrFile(IRFileName);
                var driver_ = new IP2IRDriver(index_, IRFileName, dict_);
                if (IRDrivers.ContainsKey(index_))
                    IRDrivers[index_] = driver_;
                else
                    IRDrivers.Add(index_, driver_);
            }
            catch (Exception e)
            {
                Debug.Console(1, "{0} LoadIRDriver ERROR: {1}", ClassName, e.Message);
            }
            return (uint)IRDriversLoadedCount;
        }
        public string[] AvailableIRCmds()
        {
            string[] result_ = new string[0];
            foreach (var driver in IRDrivers)
            {
                var next_ = AvailableIRCmds((uint)driver.Key);
                string[] newResult_ = new string[result_.Length + next_.Length];
                result_.CopyTo(newResult_, 0);
                next_.CopyTo(newResult_, next_.Length);
                result_ = newResult_;
            }
            return result_;
        }
        public string[] AvailableIRCmds(uint IRDriverID)
        {
            if (IRDrivers.ContainsKey((int)IRDriverID))
                return IRDrivers[(int)IRDriverID].Driver.Keys.ToArray<string>();
            string[] result = { };
            return result;
        }
        public string[] AvailableStandardIRCmds() // have you seen the inept way they have defined IROutputStandardCommands?
        {
            string[] result = {};
            return result;
        }
        public string[] AvailableStandardIRCmds(uint IRDriverID)
        {
            string[] result = { };
            return result;
        }
        public string GetStandardCmdFromIRCmd(string IRCommand)
        {
            foreach (var driver in IRDrivers)
            {
                var result_ = GetStandardCmdFromIRCmd((uint)driver.Key, IRCommand);
                if (!String.IsNullOrEmpty(result_))
                    return result_;
            }
            return String.Empty;
       }
        public string GetStandardCmdFromIRCmd(uint IRDriverID, string IRCommand) // this returns a vaid loaded command
        {
            //Debug.Console(2, "{0} GetStandardCmdFromIRCmd({1}:{2})", ClassName, IRDriverID, IRCommand);
            if (IRDrivers.ContainsKey((int)IRDriverID))
            {
                //Debug.Console(2, "{0} IRDrivers.ContainsKey({1})", ClassName, IRDriverID);
                string str_ = IP2IROperations.GetValidKeyFromDict(IRDrivers[(int)IRDriverID].Driver, IRCommand);
                //Debug.Console(2, "{0} GetStandardCmdFromIRCmd ContainsKey({1}:{2}) {3}", ClassName, IRDriverID, IRCommand, str_);
                return str_;
            }
            return String.Empty;
        }
        public string IRDriverFileNameByIRDriverId(uint IRDriverId)
        {
            if (IRDrivers.ContainsKey((int)IRDriverId))
                return IRDrivers[(int)IRDriverId].Filename;
            return String.Empty;
        }
        public uint IRDriverIdByFileName(string IRFileName)
        {
            var first_ = IRDrivers.FirstOrDefault(x => x.Value.Filename == IRFileName);
            return (uint)(first_.Equals(default(Dictionary<int, IP2IRDriver>)) ? 0 : first_.Key);
        }
        public bool IsIRCommandAvailable(string IRCmdName)
        {
            return !String.IsNullOrEmpty(GetStandardCmdFromIRCmd(IRCmdName));
        }
        public bool IsIRCommandAvailable(uint IRDriverID, string IRCmdName)
        {
            string str_ = GetStandardCmdFromIRCmd(IRDriverID, IRCmdName);
            //Debug.Console(2, "{0} IsIRCommandAvailable({1}:{2}) {3}", ClassName, IRDriverID, IRCmdName, str_);
            return !String.IsNullOrEmpty(str_);
        }
        private bool Send(Dictionary<string, IP2IRCommand> driver, string cmd)
        {
            if (!String.IsNullOrEmpty(cmd))
            {
                string cmd_ = IP2IROperations.GetValidKeyFromDict(driver, cmd);
                if (!String.IsNullOrEmpty(cmd_))
                {
                    //Debug.Console(2, "{0} Send({1}) found {2}", ClassName, cmd, cmd_==cmd?"":cmd_);
                    var str_ = IP2IROperations.GetIP2IRString(driver[cmd_], port, IRDefaultRepeat);
                    //Debug.Console(2, "{0} Send({1}) str {2}", ClassName, cmd_, str_);
                    if (!String.IsNullOrEmpty(str_))
                    {
                        SendSerialData(str_);
                        return true;
                    }
                }
            }
            return false;
        }
        public void Press(string IRCmdName)
        {
            foreach (var driver in IRDrivers) // check for exact match first
            {
                if(driver.Value.Driver.ContainsKey(IRCmdName))
                {
                    var str_ = IP2IROperations.GetIP2IRString(driver.Value.Driver[IRCmdName], port, IRDefaultRepeat);
                    if (!String.IsNullOrEmpty(str_))
                    {
                        SendSerialData(str_);
                        return;
                    }
                }

            }
            foreach (var driver in IRDrivers) // check for similar matches
                if (Send(driver.Value.Driver, IRCmdName)) return;
            Debug.Console(2, "{0} Press({1}:{2}) no function available", ClassName, port, IRCmdName);
        }
        public void Press(uint IRDriverID, string IRCmdName)
        {
            foreach (var driver in IRDrivers) // check for similar matches
                if (Send(driver.Value.Driver, IRCmdName)) return;
        }
        public void PressAndRelease(string IRCmdName, ushort TimeOutInMS)
        {
            Press(IRCmdName);
            if(IsIRCommandAvailable(IRCmdName))
            {
             // TODO: put a mutex on this
               CTimer _releaseTimer1 = new CTimer(new CTimerCallbackFunction((o) => Release()), TimeOutInMS);
            }
        }
        public void PressAndRelease(uint IRDriverID, string IRCmdName, ushort TimeOutInMS)
        {
            Press(IRDriverID, IRCmdName);
            if (IsIRCommandAvailable(IRDriverID, IRCmdName))
            {
             // TODO: put a mutex on this
               CTimer _releaseTimer2 = new CTimer(new CTimerCallbackFunction((o) => Release()), TimeOutInMS);
            }
        }
        public void Release()
        {
            try
            {
                sendString(String.Format("stopir,1:{0}", port));
            }
            catch (Exception e)
            {
                Debug.Console(2, "{0} Release({1}) ERROR: {2}", ClassName, port, e.Message);
            }
        }
        public void SendSerialData(string SerialDataToSend)
        {
            sendString(SerialDataToSend);
        }
        public void SetIRSerialSpec(eIRSerialBaudRates baudRate, eIRSerialDataBits numberOfDataBits, eIRSerialParityType parityType, eIRSerialStopBits numStopBits, Encoding stringEncoding)
        {
            Debug.Console(2, "{0} SetIRSerialSpec not implemented", ClassName);
        }
        public void UnloadAllIRDrivers()
        {
            IRDrivers.Clear();
        }
        public void UnloadIRDriver(uint IRDriverIDtoUnload)
        {
            IRDrivers.Remove((int)IRDriverIDtoUnload);
        }
        public void UnloadIRDriver()
        {
            UnloadAllIRDrivers();
        }

        /*
        #region channels

        bool busy;
        private Thread channelThread;
        public void SetChannel(uint port, uint val)
        {
            channelThread = new Thread(channelCallBack, new ChannelCallbackObject(port, val));
        }
        private object channelCallBack(object obj)
        {
            try
            {
                uint port = (obj as ChannelCallbackObject).Port;
                uint channel = (obj as ChannelCallbackObject).Channel;
                string str = channel.ToString();
                Debug.Console(1, "{0} channelCallBack, str: {1}", ClassName, str);
                while (!String.IsNullOrEmpty(str))
                {
                    string s1 = str.Substring(0, 1);
                    Debug.Console(1, "{0} channelCallBack, s1: {1}", ClassName, s1);
                    if (busy)
                    {
                        Debug.Console(1, "{0} busy while trying to send channel digit, delaying", ClassName);
                        Thread.Sleep(200);
                    }
                    else
                    {
                        Press(port, s1, 1);
                        if (str.Length > 1)
                        {
                            str = str.Substring(1, str.Length - 1);
                            string lastDigit = s1.ToString();
                            s1 = str.Substring(0, 1);
                            if (lastDigit.Equals(s1))
                            {
                                Debug.Console(1, "{0} duplicate ir digit in preset, delaying", ClassName);
                                Thread.Sleep(2000);
                            }
                            else
                                Thread.Sleep(300);
                        }
                        else
                            str = "";
                    }
                }
                Press(port, "ENTER", 1);
            }
            catch (Exception e)
            {
                Debug.Console(1, "{0} channelCallBack, ERROR: {1}", ClassName, e.Message);
            }
            return null; // delete thread
        }

        #endregion
        */

    }
}
