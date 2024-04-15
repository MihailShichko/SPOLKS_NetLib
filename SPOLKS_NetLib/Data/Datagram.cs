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
        public int SequenceNumber { get; set; }
        public byte[] Data { get; set; }
        
        public bool IsLastDatagramm { get; set; }

        public Datagram(byte[] data, int sequenceNaumber, bool isLastDatagramm)
        {
            this.Data = data;
            this.SequenceNumber = sequenceNaumber;
            this.IsLastDatagramm = isLastDatagramm;
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
