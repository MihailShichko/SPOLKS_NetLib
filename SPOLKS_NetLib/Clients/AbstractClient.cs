using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SPOLKS_NetLib.Clients
{
    public abstract class AbstractClient
    {
        protected readonly TcpClient client;
        public AbstractClient()
        { 
            client = new TcpClient();
        }

        public void Connect(IPAddress ip, int port)
        {
            try
            {
                client.Connect(ip, port);
                HandleConnection();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }

        protected abstract void HandleConnection();

    }
}
