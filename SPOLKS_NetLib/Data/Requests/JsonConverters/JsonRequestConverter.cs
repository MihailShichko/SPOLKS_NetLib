using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SPOLKS_NetLib.Data.Responces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPOLKS_NetLib.Data.Requests.JsonConverters
{
    public class JsonRequestConverter: JsonConverter<Request>
    {
        public override bool CanRead => true;
        public override bool CanWrite => true;

        public override Request ReadJson(JsonReader reader, Type objectType, Request existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            var requestType = (RequestType)Enum.Parse(typeof(RequestType), jsonObject["RequestType"].Value<string>());

            if (requestType == RequestType.CommandLineRequest)
            {
                var commandName = jsonObject["CommandName"].Value<string>();
                var flags = jsonObject["Flags"].ToObject<List<string>>();
                var arguments = jsonObject["Arguments"].ToObject<List<string>>();
                var request =  new CommandLineRequest();
                request.CommandName = commandName;
                request.Arguments = arguments;
                request.Flags = flags;
                return request;
            }
            else if (requestType == RequestType.UploadRequest)
            {
                var filename = jsonObject["FileName"].Value<string>();
                var filesize = jsonObject["FileSize"].Value<int>();
                var position = jsonObject["Position"].Value<int>();
                return new UploadRequest(filename, filesize, position);
            }

            throw new JsonSerializationException($"Unsupported response type: {requestType}");
        }

        public override void WriteJson(JsonWriter writer, Request value, JsonSerializer serializer)
        {
            JObject jsonObject = new JObject();
            jsonObject.Add("RequestType", value.RequestType.ToString());

            if (value is CommandLineRequest commandLineRequest)
            {
                jsonObject.Add("CommandName", commandLineRequest.CommandName);
                jsonObject.Add("Flags", new JArray(commandLineRequest.Flags));
                jsonObject.Add("Arguments", new JArray(commandLineRequest.Arguments));
            }
            else if (value is UploadRequest uploadRequest)
            {
                jsonObject.Add("FileName", uploadRequest.FileName);
                jsonObject.Add("FileSize", uploadRequest.FileSize);
                jsonObject.Add("Position", uploadRequest.Position);
            }
           

            jsonObject.WriteTo(writer);
        }
    }
}
