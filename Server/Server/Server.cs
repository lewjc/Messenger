using System;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using Client.Client.User

            
namespace MessagingApplication.Server
{
    public class Server
    {
        // instance of the server class
        private static Server instance = null;
        // IP address of the server to host the messaging system
        private IPAddress HOST { get; set; }
       
        private Dictionary<int, >
        // Port to open the socket on
        private int PORT { get; set; }

        // Listening socket, used for establishing connection
        public TcpListener serverSocket;

        /// <summary>
        /// CONSTRUCTOR for the Server class.
        /// </summary>
        /// <param name="host">IP address of the server</param>
        /// <param name="port">Port to open the socket on</param>
        private Server(int port, string host)
        {
            PORT = port;
            HOST = IPAddress.Parse(host);
            serverSocket = new TcpListener(HOST, PORT);

        }

        // Server property 
        public static Server serverInstance
        {
            get
            {   // Here we check to see if there already exists an instance of the server class
                // We do this 
                if(instance == null)
                {
                    instance = new Server(8000, "138.68.152.134");
                }
                return instance;

            }

        }
        /// <summary>
        /// Listens for clients connecting on the server
        /// </summary>
        private void getClientConnections()
        {
            TcpClient client = new TcpClient();
            int clientCounter = 0;

            Console.WriteLine(String.Format("Server: {0} Init on port {1}", HOST, PORT));
            while(true)
            {
                clientCounter++;

            }

        }
    }
}
