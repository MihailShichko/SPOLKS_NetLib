using SPOLKS_NetLib.Data;
using SPOLKS_NetLib.Data.Requests;
using SPOLKS_NetLib.Data.Responces;
using SPOLKS_NetLib.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;



namespace SPOLKS_NetLib.Clients
{
    public class CommandLineClient : AbstractClient
    {

        private CommandLineParser parser = new CommandLineParser();
        protected override void HandleConnection()
        {
            var writer = new StreamWriter(client.GetStream());
            var reader = new StreamReader(client.GetStream());
            
            while (true)
            {
                writer.WriteLine(GetRequest().Serialize());
                var serializedResponse = reader.ReadLine();
                ManageResponce(JsonSerializer.Deserialize<Response>(serializedResponse));
                
            }

        }

        private void ManageResponce(Response response)
        {
            switch (response.ResponseType)
            {
                case ResponseType.InfoResponce:
                    {
                        var SpecificResponse = (InfoResponse)response;
                        Console.WriteLine(SpecificResponse);
                    }
                    break;
                default:
                    {
                        
                    }
                    break;
            }

            

        }
        private CommandLineRequest GetRequest()
        {
            var req = Console.ReadLine();
            return (CommandLineRequest)parser.Parse(req);

        }

    }
}
