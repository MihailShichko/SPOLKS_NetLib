using Newtonsoft.Json;
using SPOLKS_NetLib.Data;
using SPOLKS_NetLib.Data.Requests;
using SPOLKS_NetLib.Data.Responces;
using SPOLKS_NetLib.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SPOLKS_NetLib.Severs
{
    public abstract class AbstractServer
    {
        private Dictionary<IPAddress, int> ConnectionsData = new Dictionary<IPAddress, int>();
        public int Port { get; }
        public IPAddress Address { get; }

        protected List<Datagram> datagrams = new List<Datagram>();
        public class ServerEcoSystem
        {
            public readonly DirectoryInfo storage;

            public readonly IParser parser;

            public readonly TcpListener listener;

            public Dictionary<IPAddress, int> ConnectionsData = new Dictionary<IPAddress, int>();
            
            public string lastClient = "bruh"; 
            
            public int LastUploadedData = 0;

            public int LastDownloadedData = 0;
            public ServerEcoSystem(DirectoryInfo storage, IParser parser, TcpListener listener)
            {
                this.storage = storage;
                this.parser = parser;
                this.listener = listener;
            }

            public void ClearTempData()
            {
                LastDownloadedData = 0;
                LastUploadedData = 0;
            }
        }

        protected ServerEcoSystem ecoSystem;
        protected Dictionary<string, Action<TcpClient, Request, ServerEcoSystem>> Commands = new Dictionary<string, Action<TcpClient, Request, ServerEcoSystem>>();
        /// <summary>
        /// Constructor AbstractServer.
        /// </summary>
        /// <param name="storage">Dir to keep server data.</param>
        /// <param name="parser">Object, implemention IParser interface, determines the way requests are parsed.</param>
        /// <param name="ip">Server Ip-Address.</param>
        /// <param name="port">Port.</param>
        public AbstractServer(DirectoryInfo storage, IParser parser, IPAddress ip, int port)
        {
            this.Port = port;
            this.Address = ip;
            ecoSystem = new ServerEcoSystem(storage, parser, new TcpListener(ip, port));
        }

        /// <summary>
        /// Adding supported commands in addition to built-in ones.
        /// </summary>
        /// <param name="Name">Commands name.</param>
        /// <param name="parser">Action delegate which determines how to handle command</param>
        public void AddCommand(string Name, Action<TcpClient, Request, ServerEcoSystem> Handler)
        {
            if(Commands.ContainsKey(Name))
            {
                throw new ArgumentException("Command already taken");
            }
            else
            {
                Commands.Add(Name, Handler);
            }
        }

        public abstract void Start();

        /// <summary>
        /// The way server receiving connections 
        /// </summary>
        protected abstract void GetConnections();

        /// <summary>
        /// The way server handle connection 
        /// </summary>
        //protected abstract void HandleConnection(TcpClient client);

        protected void HandleUDPUploadRequest(TcpClient client, UploadRequest uploadRequest)
        {
            UdpClient receiver = new UdpClient(Port);
            IPEndPoint endPoint = new IPEndPoint(Address, Port);
            IPEndPoint clientEndPoint = null;
            if (uploadRequest.Position == 0)
            {
                using (var fileStream = new FileStream(new string(ecoSystem.storage.FullName + "\\" + uploadRequest.FileName), FileMode.Create, FileAccess.Write))
                {

                    while (true)
                    {
                        byte[] buffer = receiver.Receive(ref clientEndPoint);
                        Datagram datagram = JsonConvert.DeserializeObject<Datagram>(Encoding.ASCII.GetString(buffer));
                        if (datagram.IsLastDatagramm) break;
                        datagrams.Add(datagram);
                    }

                    datagrams.Sort((d1, d2) => d1.SequenceNumber.CompareTo(d2.SequenceNumber));
                    foreach (var d in datagrams)
                    {
                        fileStream.Write(d.Data, 0, d.Data.Length);
                        ecoSystem.LastDownloadedData += d.Data.Length;
                    }
                }
            }
            else
            {
                using (var fileStream = new FileStream(new string(ecoSystem.storage.FullName + "\\" + uploadRequest.FileName), FileMode.Open, FileAccess.Write))
                {
                    fileStream.Seek(uploadRequest.Position, SeekOrigin.Begin);
                    int bytesRead;
                    while (true)
                    {
                        var buffer = receiver.Receive(ref clientEndPoint);
                        Datagram datagram = JsonConvert.DeserializeObject<Datagram>(Encoding.ASCII.GetString(buffer));
                        fileStream.Write(datagram.Data, 0, datagram.Data.Length);
                        ecoSystem.LastDownloadedData += datagram.Data.Length;
                        if (datagram.IsLastDatagramm) break;

                    }

                    datagrams.Sort((d1, d2) => d1.SequenceNumber.CompareTo(d2.SequenceNumber));
                    foreach (var d in datagrams)
                    {
                        fileStream.Write(d.Data, 0, d.Data.Length);
                        ecoSystem.LastDownloadedData += d.Data.Length;
                    }

                }
            }

            receiver.Close();
            datagrams.Clear();
            ecoSystem.LastUploadedData = 0;
        }

        protected void HandleTCPUploadRequest(TcpClient client, UploadRequest uploadRequest)
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

        protected void HandleCommandLineRequest(TcpClient client, CommandLineRequest request)
        {
            try
            {
                Commands[request.CommandName].Invoke(client, request, this.ecoSystem);
            }
            catch (KeyNotFoundException ex)
            {
                var writer = new StreamWriter(client.GetStream());
                writer.AutoFlush = true;
                var response = new ErrorResponse("Server does not support this command", ErrorType.WrongCommand);
                writer.WriteLine(response.Serialize());
                return;
            }
        }


    }
}
