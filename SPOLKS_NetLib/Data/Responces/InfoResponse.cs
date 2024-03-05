using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SPOLKS_NetLib.Data.Responces
{
    public class InfoResponse: Response
    {
        public string Info { get;}
        InfoResponse(ResponseType type, string info) : base(type)
        {
            this.Info = info;
        }

        public override string Serialize()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
