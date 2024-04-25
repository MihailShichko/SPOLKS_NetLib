using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SPOLKS_NetLib.Data.Responces
{
    public class ErrorResponse : Response
    {
        [DataMember]
        public string ErrorMessage { get; }

        [DataMember]
        public ErrorType ErrorType { get; }
        [JsonConstructor]
        public ErrorResponse(string ErrorMessage, ErrorType errorType) : base(ResponseType.ErrorResponse)
        {
            this.ErrorMessage = ErrorMessage;
            this.ErrorType = errorType;
        }

        public override string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

    }
}
