﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace SPOLKS_NetLib.Data.Requests
{
    [DataContract]
    public class CommandLineRequest: Request
    {
        [DataMember]
        public string? CommandName { get; set; }
        [DataMember]
        public List<string>? Flags { get; set; }
        [DataMember]
        public List<string>? Arguments { get; set; }

        [JsonConstructor]
        public CommandLineRequest() :base(RequestType.CommandLineRequest)
        {
            Flags = new List<string>();
            Arguments = new List<string>();
        }

        public override string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
