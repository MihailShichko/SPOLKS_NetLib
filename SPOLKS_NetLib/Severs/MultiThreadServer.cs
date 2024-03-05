using SPOLKS_NetLib.Parsing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SPOLKS_NetLib.Severs
{
    public class MultiThreadServer : AbstractServer
    {
        public MultiThreadServer(DirectoryInfo storage, IParser parser, IPAddress ip, int port) : base(storage, parser, ip, port)
        {

        }

        protected override void GetConnections()
        {
            while (true)
            {
                var client = this.ecoSystem.listener.AcceptTcpClient();
                Console.WriteLine($"Client connected {((IPEndPoint)client.Client.RemoteEndPoint).Address}");
                Task.Run(() => HandleConnection(client));
            }
        }

        protected override void HandleConnection(TcpClient client)
        {
                
        }
    }
}
