using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPOLKS_NetLib.Data
{
    public class Datagram
    {
        public int SquenceNumber { get; set; }
        public byte[] Data { get; set; }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
