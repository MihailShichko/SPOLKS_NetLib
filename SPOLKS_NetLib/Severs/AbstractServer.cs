using SPOLKS_NetLib.Data.Requests;
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
        public int Port { get; }
        public IPAddress Address { get; }
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

        public void Start()
        {
            this.ecoSystem.listener.Start();
            GetConnections();
        }

        /// <summary>
        /// The way server receiving connections 
        /// </summary>
        protected abstract void GetConnections();

        /// <summary>
        /// The way server handle connection 
        /// </summary>
        protected abstract void HandleConnection(TcpClient client);
        
    }
}
