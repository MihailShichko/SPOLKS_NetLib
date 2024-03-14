using Newtonsoft.Json;
using SPOLKS_NetLib.Data.Responces.JsonConverters;
using SPOLKS_NetLib.Data.Responces;
using SPOLKS_NetLib.Data;
using SPOLKS_NetLib.Data.Requests.JsonConverters;
using SPOLKS_NetLib.Data.Requests;

namespace test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string serializedRequest = "{ \"RequestType\": \"UploadRequest\", \"FileName\": \"Some\", \"FileSize\": \"10\" }";

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new JsonRequestConverter());

            var a = JsonConvert.DeserializeObject<Request>(serializedRequest, settings);

            if (a is UploadRequest uploadRequest)
            {
                Console.WriteLine(uploadRequest.FileName);
            }
            else if (a is CommandLineRequest commandLineRequest)
            {
                Console.WriteLine(commandLineRequest.CommandName);
            }
        }
    }
}