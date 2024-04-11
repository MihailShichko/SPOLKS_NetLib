using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SPOLKS_NetLib.Data.Responces
{
    [DataContract]
    public class DownloadResponse : Response
    {
        [DataMember]
        public int DataSize { get; }

        [DataMember]
        public string FileName { get; }

        [DataMember]
        public int Position { get; }

        [DataMember]
        public Protocol Protocol { get; }

        [JsonConstructor]
        public DownloadResponse(string FileName, int DataSize, int position, Protocol protocol) : base(ResponseType.DownloadResponse)
        {
            this.DataSize = DataSize; 
            this.FileName = FileName;
            this.Position = position;
            this.Protocol = protocol;
        }

        public override string Serialize()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
