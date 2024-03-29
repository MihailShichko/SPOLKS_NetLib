﻿using Newtonsoft.Json;
using SPOLKS_NetLib.Data.Responces.JsonConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SPOLKS_NetLib.Data.Responces
{
    [DataContract]
    public class InfoResponse: Response
    {
        [DataMember]
        public string Info { get; set; }

        [JsonConstructor]
        public InfoResponse(string info) : base(ResponseType.InfoResponse)
        {
            this.Info = info;
        }

        public override string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
