using PepperDash.Core;
using PepperDash.Essentials.Core.Queues;
using System;

namespace global_cache_ip2ir_epi
{
    public static class Commands
    {
        public class Command : IQueueMessage
        {
            public IBasicCommunication Coms { get; set; }
            public string Message { get; set; }

            public void Dispatch()
            {
                if (Coms == null || String.IsNullOrEmpty(Message))
                    return;
                Coms.SendText(Message + "\x0D");
            }

            public override string ToString()
            {
                return Message;
            }
        }

        public const string QueryDevice = "get_NET,0:1\rgetdevices\rgetversion\rget_IR,1:1\rget_IR,1:2\rget_IR,1:3\r";
        //public const string QueryDevice = "get_NET,0:1\r";
    }
}
