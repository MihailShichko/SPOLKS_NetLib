using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShellProgressBar;
using SPOLKS_NetLib.Data;
using SPOLKS_NetLib.Data.Requests;
using SPOLKS_NetLib.Data.Responces;
using SPOLKS_NetLib.Data.Responces.JsonConverters;
using SPOLKS_NetLib.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
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
            writer.AutoFlush = true;
            var reader = new StreamReader(client.GetStream());

            while (true)
            {                
                writer.WriteLine(GetRequest().Serialize());
                var serializedResponse = reader.ReadLine();
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new JsonResponseConverter());

                var response = JsonConvert.DeserializeObject<Response>(serializedResponse, settings);
                ManageResponce(response);

            }

        }

        private void ManageResponce(Response response)
        {
            if (response is InfoResponse infoResponse)
            {
                Console.WriteLine(infoResponse.Info);
            }
            else if(response is DownloadResponse downloadResponse)
            {
                var reader = new StreamReader(client.GetStream());
                if (downloadResponse.Position == 0)
                {
                    using (var file = new FileStream(downloadResponse.FileName, FileMode.Create, FileAccess.Write))
                    {
                        ProgressBarOptions options = new ProgressBarOptions
                        {
                            ProgressCharacter = '#',
                            ProgressBarOnBottom = true
                        };
                        using (var progressBar = new ProgressBar((int)downloadResponse.DataSize, "Progress", options))
                        {
                            byte[] buffer = new byte[1024];
                            long totalBytesRead = 0;
                            int bytesRead;
                            while (totalBytesRead < downloadResponse.DataSize && (bytesRead = reader.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                file.Write(buffer, 0, bytesRead);
                                totalBytesRead += bytesRead;
                                progressBar.Tick((int)totalBytesRead);
                            }
                        }
                    }
                }
                else
                {
                    using (var file = new FileStream(downloadResponse.FileName, FileMode.Open, FileAccess.Write))
                    {
                        file.Seek(downloadResponse.Position, SeekOrigin.Begin);
                        ProgressBarOptions options = new ProgressBarOptions
                        {
                            ProgressCharacter = '#',
                            ProgressBarOnBottom = true
                        };
                        using (var progressBar = new ProgressBar((int)downloadResponse.DataSize, "Progress", options))
                        {
                            byte[] buffer = new byte[1024];
                            long totalBytesRead = 0;
                            int bytesRead;
                            while (totalBytesRead < downloadResponse.DataSize && (bytesRead = reader.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                file.Write(buffer, 0, bytesRead);
                                totalBytesRead += bytesRead;
                                progressBar.Tick((int)totalBytesRead);
                            }
                        }
                    }
                }

                Console.WriteLine("Done");
            }
            else if(response is ErrorResponse errorResponse)
            {
                Console.WriteLine(errorResponse.ErrorMessage);
            }
            else if(response is UploadResponse uploadResponse)
            {
                using (var fileStream = new FileStream(uploadResponse.FilePath, FileMode.Open, FileAccess.Read))
                {
                    fileStream.Seek(uploadResponse.Position, SeekOrigin.Begin);
                    var writer = new StreamWriter(client.GetStream());
                    writer.AutoFlush = true;

                    long fileSize = fileStream.Length;

                    writer.WriteLine(new UploadRequest(Path.GetFileName(fileStream.Name), (int)fileSize, uploadResponse.Position).Serialize());

                    ProgressBarOptions options = new ProgressBarOptions
                    {
                        ProgressCharacter = '#',
                        ProgressBarOnBottom = true
                    };

                    using (var progressBar = new ProgressBar((int)fileSize, "Progress", options))
                    {
                        byte[] buffer = new byte[1024];
                        int totalBytes = uploadResponse.Position;
                        int bytesRead;
                        while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            writer.BaseStream.Write(buffer, 0, bytesRead);
                            totalBytes += bytesRead;
                            progressBar.Tick(totalBytes);
                        }
                    }
                }
                 
             
            }
        }
        private CommandLineRequest GetRequest()
        {
            var req = Console.ReadLine();
            return (CommandLineRequest)parser.Parse(req);

        }

    }
}
