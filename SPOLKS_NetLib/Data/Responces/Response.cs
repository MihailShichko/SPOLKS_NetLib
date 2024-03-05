using SPOLKS_NetLib.Data.Responces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPOLKS_NetLib.Data
{
    public abstract class Response
    {
        public ResponseType ResponseType { get; }

        public Response(ResponseType responseType)
        {
            this.ResponseType = responseType;
        }

        public abstract string Serialize();
    }
}
