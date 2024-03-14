using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Design;

namespace SPOLKS_NetLib.Data.Responces.JsonConverters
{
    public class JsonResponseConverter: JsonConverter<Response>
    {
        public override bool CanRead => true;
        public override bool CanWrite => true;

        public override Response ReadJson(JsonReader reader, Type objectType, Response existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            var responseType = (ResponseType)Enum.Parse(typeof(ResponseType), jsonObject["ResponseType"].Value<string>());

            if (responseType == ResponseType.InfoResponse)
            {
                var info = jsonObject["Info"].Value<string>();
                return new InfoResponse(info);
            }
            else if (responseType == ResponseType.DownloadResponse)
            {
                var dataSize = jsonObject["DataSize"].Value<int>();
                var fileName = jsonObject["FileName"].Value<string>();
                var position = jsonObject["Position"].Value<int>();
                return new DownloadResponse(fileName, dataSize, position);
            }
            else if (responseType == ResponseType.ErrorResponse)
            {
                var error = jsonObject["ErrorMessage"].Value<string>();
                return new ErrorResponse(error);
            }
            else if (responseType == ResponseType.UploadResponse)
            {
                var filePath = jsonObject["FilePath"].Value<string>();
                var position = jsonObject["Position"].Value<int>();
                return new UploadResponse(filePath, position);
            }

            throw new JsonSerializationException($"Unsupported response type: {responseType}");
        }

        public override void WriteJson(JsonWriter writer, Response value, JsonSerializer serializer)
        {
            JObject jsonObject = new JObject();
            jsonObject.Add("ResponseType", value.ResponseType.ToString());

            if (value is InfoResponse infoResponse)
            {
                jsonObject.Add("Info", infoResponse.Info);
            }
            else if (value is DownloadResponse downloadResponse)
            {
                jsonObject.Add("DataSize", downloadResponse.DataSize);
                jsonObject.Add("FileName", downloadResponse.FileName);
                jsonObject.Add("Position", downloadResponse.Position);
            }
            else if(value is ErrorResponse errorResponse)
            {
                jsonObject.Add("ErrorMessage", errorResponse.ErrorMessage);
            }
            else if(value is UploadResponse uploadResponse)
            {
                jsonObject.Add("FilePath", uploadResponse.FilePath);
                jsonObject.Add("Position", uploadResponse.Position);
            }

            jsonObject.WriteTo(writer);
        }
    }
}
