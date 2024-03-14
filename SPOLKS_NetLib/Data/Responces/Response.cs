using SPOLKS_NetLib.Data.Responces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SPOLKS_NetLib.Data
{
    //https://github.com/manuc66/JsonSubTypes
    [DataContract]
    [KnownType(typeof(InfoResponse))]
    [KnownType(typeof(DownloadResponse))]
    [KnownType(typeof(ErrorResponse))]
    [KnownType(typeof(UploadResponse))]
    public class Response
    {
        [DataMember]
        public ResponseType ResponseType { get; set; }

        public Response(ResponseType responseType)
        {
            this.ResponseType = responseType;
        }

        public virtual string Serialize()
        {
            return this.Serialize();
        }
    }
}
