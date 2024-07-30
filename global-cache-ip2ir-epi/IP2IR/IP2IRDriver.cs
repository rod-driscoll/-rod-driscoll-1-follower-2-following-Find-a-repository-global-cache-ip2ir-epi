using System.Collections.Generic;

namespace global_cache_ip2ir_epi
{
    public class IP2IRDriver
    {
        public string Filename { get; set; }
        public int ID { get; set; }
        public Dictionary<string, IP2IRCommand> Driver { get; set; }
        public IP2IRDriver(int ID, string Filename, Dictionary<string, IP2IRCommand> Driver)
        {
            this.ID = ID;
            this.Filename = Filename;
            this.Driver = Driver;
        }
    }
}
