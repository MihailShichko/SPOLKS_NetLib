using Newtonsoft.Json;
using SPOLKS_NetLib.Data.Requests.JsonConverters;
using SPOLKS_NetLib.Data.Requests;
using SPOLKS_NetLib.Data;
using SPOLKS_NetLib.Parsing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SPOLKS_NetLib.Data.Responces;

namespace SPOLKS_NetLib.Severs
{
    public class MultiThreadServer : AbstractServer
    {
        private int MaxThread = 3;
        private int CurrentThreads = 0;
        public MultiThreadServer(DirectoryInfo storage, IParser parser, IPAddress ip, int port) : base(storage, parser, ip, port)
        {

        }

        public override void Start()
        {
            this.ecoSystem.listener.Start();
            GetConnections();
        }

        protected override void GetConnections()
        {
            while (true)
            {
                var client = this.ecoSystem.listener.AcceptTcpClient();
                Console.WriteLine($"Client connected {((IPEndPoint)client.Client.RemoteEndPoint).Address}");
                if (CurrentThreads < MaxThread)
                {
                    this.CurrentThreads++;
                    var writer = new StreamWriter(client.GetStream());
                    writer.AutoFlush = true;
                    writer.WriteLine(new InfoResponse("Connection Established").Serialize());
                    Task.Run(async () =>
                    {
                        await HandleConnection(client);
                        this.CurrentThreads--;
                    });
                }
                else
                {
                    var writer = new StreamWriter(client.GetStream());
                    writer.AutoFlush = true;
                    writer.WriteLine(new ErrorResponse("Server is full", ErrorType.FullServer).Serialize());
                    client.Close();
                }
            }
        }

        protected new Task HandleConnection(TcpClient client) //int i is used for testing only will be deleted later
        {
            try
            {
                var reader = new StreamReader(client.GetStream());
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new JsonRequestConverter());
                while (client.Connected)
                {

                    var request = JsonConvert.DeserializeObject<Request>(reader.ReadLine(), settings);
                    if (request is CommandLineRequest commandLineRequest)
                    {
                        HandleCommandLineRequest(client, commandLineRequest);
                    }
                    else if (request is UploadRequest uploadRequest)
                    {
                        if (uploadRequest.Protocol == Protocol.TCP)
                        {
                            HandleTCPUploadRequest(client, uploadRequest);
                        }
                        else
                        {
                            HandleUDPUploadRequest(client, uploadRequest);
                        }
                    }

                }

                return Task.CompletedTask;
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Client {((IPEndPoint)client.Client.RemoteEndPoint).Address} disconnected");
                return Task.CompletedTask;
            }
        }

    }
}
