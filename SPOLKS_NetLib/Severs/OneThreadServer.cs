using SPOLKS_NetLib.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using SPOLKS_NetLib.Data.Requests;

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
                Console.WriteLine($"Client connected {((IPEndPoint)client.Client.RemoteEndPoint).Address}");
                HandleConnection(client);
            }
        }

        protected override async void HandleConnection(TcpClient client)
        {
            var reader = new StreamReader(client.GetStream());
            while (!client.Connected)
            {
                var request = await JsonSerializer.DeserializeAsync<Request>(reader.BaseStream);
                switch (request.GetType())
                {
                    case Type t when t == typeof(CommandLineRequest):
                        CommandLineRequest r = (CommandLineRequest)request;
                        HandleCommandLineRequest(client, r);
                        break;
                }
            }

        }

        private void HandleCommandLineRequest(TcpClient client, CommandLineRequest request)
        {
            Commands[request.CommandName].Invoke(client, request, this.ecoSystem);
        }

        private void sendFile(string fileName, TcpClient client)
        {
            BinaryWriter writer = new BinaryWriter(client.GetStream());
            using (var fileStream = new FileStream(new string(this.ecoSystem.storage.FullName + "\\" + fileName), FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[1024];
                int bytesRead;
                int bytesReadTotally = 0;
                int offset;
                offset = ConnectionsData[((IPEndPoint)client.Client.RemoteEndPoint).Address];
                fileStream.Seek(offset, SeekOrigin.Begin);
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (client.Client.Poll(1000, SelectMode.SelectRead))
                    {
                        Console.WriteLine($"Client {0} disconnected", ((IPEndPoint)client.Client.RemoteEndPoint).Address);
                        return;
                    }

                    bytesReadTotally += bytesRead;
                    ConnectionsData[((IPEndPoint)client.Client.RemoteEndPoint).Address] = bytesReadTotally;
                    writer.Write(buffer, 0, bytesRead);

                }
            }
        }


    }
}
