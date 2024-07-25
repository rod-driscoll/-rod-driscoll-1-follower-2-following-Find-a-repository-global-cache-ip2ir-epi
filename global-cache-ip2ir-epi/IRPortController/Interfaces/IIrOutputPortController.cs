using PepperDash.Essentials.Core;

namespace global_cache_ip2ir_epi.IRPortController
{
    /// <summary>
    /// 
    /// </summary>
    public interface IIrOutputPortController
    {
        BoolFeedback DriverLoaded { get; }

        ushort StandardIrPulseTime { get; set; }
        string DriverFilepath { get; }
        bool DriverIsLoaded { get; }

        string[] IrFileCommands { get; } // { return IrPort.AvailableStandardIRCmds(IrPortUid); } }

        bool UseBridgeJoinMap { get; }


        void PrintAvailableCommands();
        void LoadDriver(string path);
        void PressRelease(string command, bool state);
        void Pulse(string command, ushort time);
    }
}
