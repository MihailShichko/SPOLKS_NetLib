using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SPOLKS_NetLib.Data.Requests
{
    [DataContract]
    public class UploadRequest: Request
    {
        [DataMember]
        public string FileName { get; }

        [DataMember]
        public int FileSize { get; }

        [DataMember]
        public int Position { get; }

        [DataMember]
        public Protocol Protocol { get; }

        [JsonConstructor]
        public UploadRequest(string filename, int fileSize, int position, Protocol protocol) : base(RequestType.UploadRequest)
        {
            this.FileName = filename;
            this.FileSize = fileSize;
            this.Position = position;
            this.Protocol = protocol;
        }

        public override string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
