using avit_essentials_common.IRPorts;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpProInternal;
using System.Text;

namespace global_cache_ip2ir_epi.IRPortController
{
    public class IROutputPortCrestron: IIROutputPort// IROutputPort, IIROutputPort
    {
        public IROutputPort CrestronIROutputPort { get; set; }

        public int IRDriversLoadedCount { get { return CrestronIROutputPort.IRDriversLoadedCount; } }

        public IROutputPortCrestron(uint portID, CrestronDevice parentDevice, CrestronControlSystem Owner, object paramParent)
        {
            CrestronIROutputPort = new IROutputPort(portID, parentDevice, Owner, paramParent);
        }
        public IROutputPortCrestron(uint portID, bool TxByUID, CrestronDevice parentDevice, CrestronControlSystem Owner, object paramParent)
        { 
            CrestronIROutputPort = new IROutputPort(portID, TxByUID, parentDevice, Owner, paramParent);
        }

        public string[] AvailableIRCmds()
        {
            return CrestronIROutputPort.AvailableIRCmds();
        }

        public string[] AvailableIRCmds(uint IRDriverID)
        {
            return CrestronIROutputPort.AvailableIRCmds(IRDriverID);
        }

        public string[] AvailableStandardIRCmds()
        {
            return CrestronIROutputPort.AvailableStandardIRCmds();
        }

        public string[] AvailableStandardIRCmds(uint IRDriverID)
        {
            return CrestronIROutputPort.AvailableStandardIRCmds(IRDriverID);
        }

        public string GetStandardCmdFromIRCmd(string IRCommand)
        {
            return CrestronIROutputPort.GetStandardCmdFromIRCmd(IRCommand);
        }

        public string GetStandardCmdFromIRCmd(uint IRDriverID, string IRCommand)
        {
            return CrestronIROutputPort.GetStandardCmdFromIRCmd(IRDriverID, IRCommand);
        }

        public string IRDriverFileNameByIRDriverId(uint IRDriverId)
        {
            return CrestronIROutputPort.IRDriverFileNameByIRDriverId((uint)IRDriverId);
        }

        public uint IRDriverIdByFileName(string IRFileName)
        {
            return CrestronIROutputPort.IRDriverIdByFileName(IRFileName);
        }

        public bool IsIRCommandAvailable(string IRCmdName)
        {
            return CrestronIROutputPort.IsIRCommandAvailable(IRCmdName);
        }

        public bool IsIRCommandAvailable(uint IRDriverID, string IRCmdName)
        {
            return CrestronIROutputPort.IsIRCommandAvailable(IRDriverID, IRCmdName);
        }

        public uint LoadIRDriver(string IRFileName)
        {
            return CrestronIROutputPort.LoadIRDriver(IRFileName);
        }

        public void Press(string IRCmdName)
        {
            CrestronIROutputPort.Press(IRCmdName);
        }

        public void Press(uint IRDriverID, string IRCmdName)
        {
            CrestronIROutputPort.Press(IRDriverID, IRCmdName);
        }

        public void PressAndRelease(string IRCmdName, ushort TimeOutInMS)
        {
            CrestronIROutputPort.PressAndRelease(IRCmdName, TimeOutInMS);
        }

        public void PressAndRelease(uint IRDriverID, string IRCmdName, ushort TimeOutInMS)
        {
            CrestronIROutputPort.PressAndRelease(IRDriverID, IRCmdName, TimeOutInMS);
        }

        public void Release()
        {
            CrestronIROutputPort.Release();
        }

        public void SendSerialData(string SerialDataToSend)
        {
            CrestronIROutputPort.SendSerialData(SerialDataToSend);
        }

        public void SetIRSerialSpec(eIRSerialBaudRates baudRate, eIRSerialDataBits numberOfDataBits, eIRSerialParityType parityType, eIRSerialStopBits numStopBits, Encoding stringEncoding)
        {
            CrestronIROutputPort.SetIRSerialSpec(baudRate, numberOfDataBits, parityType, numStopBits, stringEncoding);
        }

        public void UnloadAllIRDrivers()
        {
            CrestronIROutputPort.UnloadAllIRDrivers();
        }

        public void UnloadIRDriver()
        {
            CrestronIROutputPort.UnloadAllIRDrivers();
        }

        public void UnloadIRDriver(uint IRDriverIDtoUnload)
        {
            CrestronIROutputPort.UnloadIRDriver(IRDriverIDtoUnload);
        }
    }
}
