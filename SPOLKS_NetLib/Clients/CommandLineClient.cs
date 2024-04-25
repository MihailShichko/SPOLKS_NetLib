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
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;



namespace SPOLKS_NetLib.Clients
{
    public class CommandLineClient : AbstractClient //TODO сделать нормально upload и download (один метод и на случай pos = 0 и pos != 0)
    {

        private CommandLineParser parser = new CommandLineParser();
        protected override void HandleConnection()
        {
            var writer = new StreamWriter(client.GetStream());
            writer.AutoFlush = true;
            var reader = new StreamReader(client.GetStream());
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new JsonResponseConverter());
            var connectionStatusSerialized = reader.ReadLine();
            var connectionStatusDeserialized = JsonConvert.DeserializeObject<Response>(connectionStatusSerialized, settings);
            ManageResponce(connectionStatusDeserialized);
            while (true)
            {
                writer.WriteLine(GetRequest().Serialize());
                var serializedResponse = reader.ReadLine();
               

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
                if(downloadResponse.Protocol == Protocol.TCP)
                {
                    this.TcpDownload(downloadResponse);
                }
                else
                {
                    this.UdpDownload(downloadResponse);
                }
            }
            else if(response is ErrorResponse errorResponse)
            {
                if(errorResponse.ErrorType == ErrorType.FullServer)
                {
                    this.client.Close();
                    throw new Exception(errorResponse.ErrorMessage);
                }

                Console.WriteLine(errorResponse.ErrorMessage);
            }
            else if(response is UploadResponse uploadResponse)
            {
                if(uploadResponse.Protocol == Protocol.TCP)
                {
                    this.TcpUpload(uploadResponse);
                }
                else
                {
                    this.UdpUpload(uploadResponse);
                }
                 
            }
        }
        
        private void TcpDownload(DownloadResponse downloadResponse)
        {
            var reader = new StreamReader(client.GetStream());
            if (downloadResponse.Position == 0)
            {
                using (var file = new FileStream(downloadResponse.FileName, FileMode.Create, FileAccess.Write))
                {
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
                    using (var progressBar = new ProgressBar((int)downloadResponse.DataSize, "Progress", options))
                    {
                        byte[] buffer = new byte[1024];
                        long totalBytesRead = downloadResponse.Position;
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

        private void UdpDownload(DownloadResponse downloadResponse)
        {
            UdpClient receiver = new UdpClient(port);
            IPEndPoint serverEndPoint = null;
            int pollingTimeoutMilliSec = 5000;
            if (downloadResponse.Position == 0)
            {
                using (var file = new FileStream(downloadResponse.FileName, FileMode.Create, FileAccess.Write))
                {
                    using (var progressBar = new ProgressBar((int)downloadResponse.DataSize, "Progress", options))
                    {
                        long totalBytesRead = downloadResponse.Position;
                        while (true)
                        {
                         
                            byte[] buffer = receiver.Receive(ref serverEndPoint);
                           
                            Datagram datagram = JsonConvert.DeserializeObject<Datagram>(Encoding.ASCII.GetString(buffer));
                            if (datagram.IsLastDatagramm) break;
                            
                            totalBytesRead += datagram.Data.Length;
                            progressBar.Tick((int)totalBytesRead);
                            datagrams.Add(datagram);
                            if (!receiver.Client.Poll(pollingTimeoutMilliSec * 1000, SelectMode.SelectRead))
                            {
                                Console.WriteLine("Timeout");
                                break;
                            }
                        }

                        datagrams.Sort((d1, d2) => d1.SequenceNumber.CompareTo(d2.SequenceNumber));
                        foreach (var d in datagrams)
                        {
                            file.Write(d.Data, 0, d.Data.Length);
                            totalBytesRead += d.Data.Length;
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
                    using (var progressBar = new ProgressBar((int)downloadResponse.DataSize, "Progress", options))
                    {
                        long totalBytesRead = downloadResponse.Position;
                        while (true)
                        { 
                            byte[] buffer = receiver.Receive(ref serverEndPoint);
                            Datagram datagram = JsonConvert.DeserializeObject<Datagram>(Encoding.ASCII.GetString(buffer));
                            if (datagram.IsLastDatagramm) break;
                           
                            datagrams.Add(datagram);
                            if (!receiver.Client.Poll(pollingTimeoutMilliSec * 1000, SelectMode.SelectRead))
                            {
                                Console.WriteLine("Timeout");
                                break;
                            }
                        }

                        datagrams.Sort((d1, d2) => d1.SequenceNumber.CompareTo(d2.SequenceNumber));
                        foreach(var d in datagrams)
                        {
                            file.Write(d.Data, 0, d.Data.Length);
                            totalBytesRead += d.Data.Length;
                            progressBar.Tick((int)totalBytesRead);
                        }
                    }
                }
            }

            receiver.Close();
            datagrams.Clear();
            Console.WriteLine("Done");
        }

        private void UdpUpload(UploadResponse uploadResponse)
        {
            using (var fileStream = new FileStream(uploadResponse.FilePath, FileMode.Open, FileAccess.Read))
            {
                fileStream.Seek(uploadResponse.Position, SeekOrigin.Begin);
           
                long fileSize = fileStream.Length;

                UdpClient sender = new UdpClient();
                var serverEndPoint = new IPEndPoint(ip, port);
                var writer = new StreamWriter(client.GetStream());
                writer.AutoFlush = true;
                writer.WriteLine(new UploadRequest(Path.GetFileName(fileStream.Name), (int)fileSize, uploadResponse.Position, Protocol.UDP).Serialize());
                
                Thread.Sleep(1000);
                int pollingTimeoutMilliSec = 5000;
                using (var progressBar = new ProgressBar((int)fileSize, "Progress", options))
                {
                    byte[] buffer = new byte[8192];
                    int totalBytes = uploadResponse.Position;
                    int bytesRead;
                    int sequeneceNumber = 1;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        Datagram datagram = new Datagram(buffer, sequeneceNumber, false);
                        sender.Send(Encoding.ASCII.GetBytes(datagram.Serialize()), serverEndPoint);
                        Thread.Sleep(1);
                        totalBytes += bytesRead;
                        progressBar.Tick(totalBytes);
                        sequeneceNumber++;
                        if (!sender.Client.Poll(pollingTimeoutMilliSec * 1000, SelectMode.SelectWrite))
                        {
                            Console.WriteLine("Timeout");
                            break;
                        }
                    }

                    var lastDatagram = new Datagram(null, -1, true);
                    sender.Send(Encoding.ASCII.GetBytes(lastDatagram.Serialize()), serverEndPoint);
                    sender.Close();
                }
            }
        }
        private void TcpUpload(UploadResponse uploadResponse)
        {
            using (var fileStream = new FileStream(uploadResponse.FilePath, FileMode.Open, FileAccess.Read))
            {
                fileStream.Seek(uploadResponse.Position, SeekOrigin.Begin);
                var writer = new StreamWriter(client.GetStream());
                writer.AutoFlush = true;

                long fileSize = fileStream.Length;

                writer.WriteLine(new UploadRequest(Path.GetFileName(fileStream.Name), (int)fileSize, uploadResponse.Position, Protocol.TCP).Serialize());


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
        private CommandLineRequest GetRequest()
        {
            var req = Console.ReadLine();
            return (CommandLineRequest)parser.Parse(req);

        }

    }
}
