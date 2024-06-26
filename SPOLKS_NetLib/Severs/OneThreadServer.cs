﻿using SPOLKS_NetLib.Parsing;
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
using SPOLKS_NetLib.Data;
using SPOLKS_NetLib.Clients;

namespace SPOLKS_NetLib.Severs
{
    public class OneThreadServer : AbstractServer
    {
        //private Dictionary<IPAddress, int> ConnectionsData = new Dictionary<IPAddress, int>();
        public OneThreadServer(DirectoryInfo storage, IParser parser, IPAddress ip, int port) : base(storage, parser, ip, port)
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
                if(((IPEndPoint?)client.Client.RemoteEndPoint)?.Address.ToString() != ecoSystem.lastClient)
                {
                    ecoSystem.ClearTempData();
                    ecoSystem.lastClient = ((IPEndPoint?)client.Client.RemoteEndPoint).Address.ToString();
                }

                ecoSystem.lastClient = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                Console.WriteLine($"Client connected {ecoSystem.lastClient}");
                HandleConnection(client);
            }
        }

        protected void HandleConnection(TcpClient client)
        {
            try
            {
                var writer = new StreamWriter(client.GetStream());
                writer.AutoFlush = true;
                writer.WriteLine(new InfoResponse("Connection Established").Serialize());

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
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Client {((IPEndPoint)client.Client.RemoteEndPoint).Address} disconnected");
                return;
            }
        }

        //private void HandleUDPUploadRequest(TcpClient client, UploadRequest uploadRequest)
        //{
        //    UdpClient receiver = new UdpClient(Port);
        //    IPEndPoint endPoint = new IPEndPoint(Address, Port);
        //    IPEndPoint clientEndPoint = null;
        //    if (uploadRequest.Position == 0)
        //    {
        //        using (var fileStream = new FileStream(new string(ecoSystem.storage.FullName + "\\" + uploadRequest.FileName), FileMode.Create, FileAccess.Write))
        //        {
                   
        //            while (true)
        //            {
        //                byte[] buffer = receiver.Receive(ref clientEndPoint);
        //                Datagram datagram = JsonConvert.DeserializeObject<Datagram>(Encoding.ASCII.GetString(buffer));
        //                if (datagram.IsLastDatagramm) break;
        //                datagrams.Add(datagram);
        //            }

        //            datagrams.Sort((d1, d2) => d1.SequenceNumber.CompareTo(d2.SequenceNumber));
        //            foreach (var d in datagrams)
        //            {
        //                fileStream.Write(d.Data, 0, d.Data.Length);
        //                ecoSystem.LastDownloadedData += d.Data.Length;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        using (var fileStream = new FileStream(new string(ecoSystem.storage.FullName + "\\" + uploadRequest.FileName), FileMode.Open, FileAccess.Write))
        //        {
        //            fileStream.Seek(uploadRequest.Position, SeekOrigin.Begin);
        //            int bytesRead;
        //            while (true)
        //            {
        //                var buffer = receiver.Receive(ref clientEndPoint);
        //                Datagram datagram = JsonConvert.DeserializeObject<Datagram>(Encoding.ASCII.GetString(buffer));
        //                fileStream.Write(datagram.Data, 0, datagram.Data.Length);
        //                ecoSystem.LastDownloadedData += datagram.Data.Length;
        //                if(datagram.IsLastDatagramm) break;
                        
        //            }

        //            datagrams.Sort((d1, d2) => d1.SequenceNumber.CompareTo(d2.SequenceNumber));
        //            foreach (var d in datagrams)
        //            {
        //                fileStream.Write(d.Data, 0, d.Data.Length);
        //                ecoSystem.LastDownloadedData += d.Data.Length;
        //            }

        //        }
        //    }

        //    receiver.Close();
        //    datagrams.Clear();
        //    ecoSystem.LastUploadedData = 0;
        //}

        //private void HandleTCPUploadRequest(TcpClient client, UploadRequest uploadRequest)
        //{
        //    if (uploadRequest.Position == 0)
        //    {
        //        using (var fileStream = new FileStream(new string(ecoSystem.storage.FullName + "\\" + uploadRequest.FileName), FileMode.Create, FileAccess.Write))
        //        {
        //            var reader = new StreamReader(client.GetStream());

        //            byte[] buffer = new byte[1024];
        //            long totalBytesRead = 0;
        //            int bytesRead;
        //            while (totalBytesRead < uploadRequest.FileSize && (bytesRead = reader.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
        //            {
        //                fileStream.Write(buffer, 0, bytesRead);
        //                ecoSystem.LastUploadedData += bytesRead;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        using (var fileStream = new FileStream(new string(ecoSystem.storage.FullName + "\\" + uploadRequest.FileName), FileMode.Open, FileAccess.Write))
        //        {
        //            var reader = new StreamReader(client.GetStream());

        //            fileStream.Seek(uploadRequest.Position, SeekOrigin.Begin);

        //            byte[] buffer = new byte[1024];
        //            long totalBytesRead = 0;
        //            int bytesRead;
        //            while (totalBytesRead < uploadRequest.FileSize && (bytesRead = reader.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
        //            {
        //                fileStream.Write(buffer, 0, bytesRead);
        //                ecoSystem.LastUploadedData += bytesRead;
        //            }
        //        }
        //    }

        //    ecoSystem.LastUploadedData = 0;
        //}

        //private void HandleCommandLineRequest(TcpClient client, CommandLineRequest request)
        //{
        //    try
        //    {
        //        Commands[request.CommandName].Invoke(client, request, this.ecoSystem);
        //    }
        //    catch (KeyNotFoundException ex)
        //    {
        //        var writer = new StreamWriter(client.GetStream());
        //        writer.AutoFlush = true;
        //        var response = new ErrorResponse("Server does not support this command");
        //        writer.WriteLine(response.Serialize());
        //        return;
        //    }
        //}


    }
}
