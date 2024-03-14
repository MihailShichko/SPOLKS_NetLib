using Newtonsoft.Json;
using SPOLKS_NetLib.Data.Responces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace SPOLKS_NetLib.Data.Requests
{
    [DataContract]
    [KnownType(typeof(CommandLineRequest))]
    [KnownType(typeof(UploadRequest))]
    public class Request
    {
        [DataMember]
        public RequestType RequestType { get;}

        [JsonConstructor]
        public Request(RequestType requestType)
        {
            this.RequestType = requestType;
        }

        public virtual string Serialize()
        {
            return this.Serialize();
        }
        
    }
}
