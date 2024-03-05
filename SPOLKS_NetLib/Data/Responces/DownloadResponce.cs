using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SPOLKS_NetLib.Data.Responces
{
    public class DownloadResponce : Response
    {
        byte[] data = new byte[1024]; 

        public DownloadResponce(ResponseType responseType, byte[] data) : base(responseType)
        {
            this.data = data;
        }

        public override string Serialize()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
