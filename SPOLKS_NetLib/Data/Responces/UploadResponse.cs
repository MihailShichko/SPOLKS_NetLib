using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

using System.Threading.Tasks;

namespace SPOLKS_NetLib.Data.Responces
{
    [DataContract]
    public class UploadResponse: Response
    {
        [DataMember]
        public string FilePath { get; }
        [DataMember]
        public int Position { get; }

        [JsonConstructor]
        public UploadResponse(string filePath, int position) : base(ResponseType.UploadResponse)
        {
            FilePath = filePath;
            Position = position;
        }

        public override string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
