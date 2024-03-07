using SPOLKS_NetLib.Data.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SPOLKS_NetLib.Parsing
{
    public class CommandLineParser : IParser
    {
        public Request Parse(string message)
        {
            var regex = new Regex("^\\w+");
            var request = new CommandLineRequest();
            request.CommandName = regex.Match(message).Value;
            regex = new Regex("-\\w");
            foreach (Match flag in regex.Matches(message))
            {
                request.Flags.Add(flag.Value);
            }

            regex = new Regex(" \\w+");
            foreach (Match arg in regex.Matches(message))
            {
                request.Arguments.Add(arg.Value.Substring(1));
            }

            return request;
        }
    }
}
