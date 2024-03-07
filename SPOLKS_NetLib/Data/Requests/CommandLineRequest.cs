using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SPOLKS_NetLib.Data.Requests
{
    public class CommandLineRequest: Request
    {
        public string? CommandName { get; set; }
        public List<string>? Flags { get; set; }
        public List<string>? Arguments { get; set; }

        public CommandLineRequest() :base(RequestType.CommandLineRequest)
        {
        
        }

        public override string Serialize()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
