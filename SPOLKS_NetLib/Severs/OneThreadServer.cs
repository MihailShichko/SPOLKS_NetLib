using SPOLKS_NetLib.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SPOLKS_NetLib.Data.Requests;
using SPOLKS_NetLib.Data.Responces;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SPOLKS_NetLib.Data.Requests.JsonConverters;
using System.Reflection.PortableExecutable;
using ShellProgressBar;

namespace SPOLKS_NetLib.Severs
{
    public class OneThreadServer : AbstractServer
    {
        private Dictionary<IPAddress, int> ConnectionsData = new Dictionary<IPAddress, int>();
        public OneThreadServer(DirectoryInfo storage, IParser parser, IPAddress ip, int port) : base(storage, parser, ip, port)
        {

        }

        protected override void GetConnections()
        {
            while (true)
            {
                var client = this.ecoSystem.listener.AcceptTcpClient();
                if(((IPEndPoint)client.Client.RemoteEndPoint).Address != ecoSystem.lastClient)
                {
                    ecoSystem.ClearTempData();
                    ecoSystem.lastClient = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
                }

                ecoSystem.lastClient = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
                Console.WriteLine($"Client connected {ecoSystem.lastClient}");
                HandleConnection(client);
            }
        }

        protected override void HandleConnection(TcpClient client)
        {
            try
            {
                var reader = new StreamReader(client.GetStream());
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new JsonRequestConverter());
                while (client.Connected)
                {

                    var request = JsonConvert.DeserializeObject<Request>(reader.ReadLine(), settings);
                    if(request is CommandLineRequest commandLineRequest)
                    {
                        HandleCommandLineRequest(client, commandLineRequest);
                    }
                    else if(request is UploadRequest uploadRequest)
                    {
                        HandleUploadRequest(client, uploadRequest);
                    }
                    
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Client {((IPEndPoint)client.Client.RemoteEndPoint).Address} disconnected");
                return;
            }
        }

        private void HandleUploadRequest(TcpClient client, UploadRequest uploadRequest)
        {
            if (uploadRequest.Position == 0)
            {
                using (var fileStream = new FileStream(new string(ecoSystem.storage.FullName + "\\" + uploadRequest.FileName), FileMode.Create, FileAccess.Write))
                {
                    var reader = new StreamReader(client.GetStream());

                    byte[] buffer = new byte[1024];
                    long totalBytesRead = 0;
                    int bytesRead;
                    while (totalBytesRead < uploadRequest.FileSize && (bytesRead = reader.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fileStream.Write(buffer, 0, bytesRead);
                        ecoSystem.LastUploadedData += bytesRead;
                    }
                }
            }
            else
            {
                using (var fileStream = new FileStream(new string(ecoSystem.storage.FullName + "\\" + uploadRequest.FileName), FileMode.Open, FileAccess.Write))
                {
                    var reader = new StreamReader(client.GetStream());

                    fileStream.Seek(uploadRequest.Position, SeekOrigin.Begin);

                    byte[] buffer = new byte[1024];
                    long totalBytesRead = 0;
                    int bytesRead;
                    while (totalBytesRead < uploadRequest.FileSize && (bytesRead = reader.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fileStream.Write(buffer, 0, bytesRead);
                        ecoSystem.LastUploadedData += bytesRead;
                    }
                }
            }

            ecoSystem.LastUploadedData = 0;
        }

        private void HandleCommandLineRequest(TcpClient client, CommandLineRequest request)
        {
            try
            {
                Commands[request.CommandName].Invoke(client, request, this.ecoSystem);
            }
            catch (KeyNotFoundException ex)
            {
                var writer = new StreamWriter(client.GetStream());
                writer.AutoFlush = true;
                var response = new ErrorResponse("Server does not support this command");
                writer.WriteLine(response.Serialize());
                return;
            }
        }


    }
}
