using System;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Collections.Generic;
using Client.Client.UserLib;
            
            
namespace Server.ServerLib
{
    public class HostServer
    {

        // Max Message length
        public static readonly int MAX_MESSAGE = 2048;

        // instance of the server class
        private static HostServer instance = null;
        // IP address of the server to host the messaging system
        private IPAddress HOST { get; set; }

        private Dictionary<int, User> clientList = new Dictionary<int, User>();

        // Port to open the socket on
        private int PORT { get; set; }

        // Listening socket, used for establishing connection
        public TcpListener serverSocket;

        /// <summary>
        /// CONSTRUCTOR for the Server class.
        /// </summary>
        /// <param name="host">IP address of the server</param>
        /// <param name="port">Port to open the socket on</param>
        private HostServer(int port, string host)
        {
            PORT = port;
            HOST = IPAddress.Parse(host);
            serverSocket = new TcpListener(HOST, PORT);
            getClientConnections();

        }

        // Server property 
        public static HostServer serverInstance
        {
            get
            {   // Here we check to see if there already exists an instance of the server class
                // We do this 
                if(instance == null)
                {
                    instance = new HostServer(8000, "127.0.0.1");
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
            try
            {

                serverSocket.Start();
                Console.WriteLine(String.Format("Server: {0} Init on port {1}", HOST, PORT));

                while (true)
                {
                    // Here we incrememnt the client counter and wait for a new client to connect to the server.
                    clientCounter++;
                    // Waiting for a new client to establish connection
                    client = serverSocket.AcceptTcpClient();
                    // Load the client onto a new thread
                    Thread newClientThread = new Thread(() => manageUserLogin(clientCounter, client));
                    Console.WriteLine(String.Format("Client {0} connected to the server", clientCounter));

                    // Start the new client's thread.
                    newClientThread.Start();

                }
            } catch(Exception e){
                Console.WriteLine(e);
            }
        }
        /// <summary>
        /// Here is where the user will login to the system, using previous credentials.
        /// if they have not before made an account, they will be prompted to.
        /// </summary>
        /// <param name="clientNumber">Client number.</param>
        /// <param name="client">Client socket connection</param>
        /// 
        private void manageUserLogin(int clientNumber, TcpClient client)
        {
            
            // Presenting a menu to the user, allow them to make their choice on what to do.
            // Loop until the user has made a correct choice

            string clientMenuString = "[MENU - MAKE A CHOICE]\n[0] Quit\n[1]Login\n[2]New User\n";

            // Open the clients stream and present to them the menu.
            NetworkStream stream = client.GetStream();
            byte[] textBuffer = Encoding.ASCII.GetBytes(clientMenuString);
            // Output the menu
            stream.Write(textBuffer, 0, textBuffer.Length);
        
            while(true)
            {
                // Read their response
                stream = client.GetStream();
                int response = stream.Read(textBuffer, 0, textBuffer.Length);
                // Converting the bytes to a manipulatiable string
                string userInput = Encoding.ASCII.GetString(textBuffer, 0, response);

                // Checking if the user's input is within the login menu choice constraints, this allows us to pass
                // a correct input into the function to determine what the user wants to do.
                if(MenuLib.Menu.verifyLoginMenuChoice(userInput))
                {
                    MenuLib.Menu.LoginMenu(userInput);
                } else{
                    // The user has not input a viable choice, so we tell them to try again


                }

            }
        }
        /// <summary>
        /// Sends a message to the specified client's input stream
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <param name="clientStream">The clients input stream of which the message is being sent..</param>
        private void sendMessage(string message, NetworkStream clientStream)
        {
            byte[] textBuffer = Encoding.ASCII.GetBytes(message);
            // Output the menu
            clientStream.Write(textBuffer, 0, textBuffer.Length);
        }
    }
}
