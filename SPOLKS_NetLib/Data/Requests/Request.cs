using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SPOLKS_NetLib.Data.Requests
{
    public abstract class Request
    {
        public RequestType RequestType { get;}
        public Request(RequestType requestType)
        {
            this.RequestType = requestType;
        }

        public abstract string Serialize();
        
    }
}
