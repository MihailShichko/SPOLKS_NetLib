using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPOLKS_NetLib.Data.Requests;

namespace SPOLKS_NetLib.Parsing
{
    public interface IParser
    {
        public Request Parse(string message);
    }
}
