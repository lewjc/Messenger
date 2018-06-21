using System;
using System.Threading;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Collections.Generic;
using Client.UserLib;
using Server.MenuLib;
            
            
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

        // Holds the current users logged into the system.
        private Dictionary<int, User> clientList = new Dictionary<int, User>();

        // Port to open the socket on
        private int PORT { get; set; }

        // Listening socket, used for establishing connection
        public TcpListener serverSocket;

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
        /// CONSTRUCTOR for the Server class.
        /// </summary>
        /// <param name="host">IP address of the server</param>
        /// <param name="port">Port to open the socket on</param>
        private HostServer(int port, string host)
        {
            PORT = port;
            HOST = IPAddress.Parse(host);
            serverSocket = new TcpListener(HOST, PORT);
            while (true)
            {

                try
                {
                    GetClientConnections();
                }
                catch (SocketException)
                {
                    Console.WriteLine("Address already in use! trying connection again 1 second...");
                }
            }
                


        }

        /// <summary>
        /// Listens for clients connecting on the server
        /// </summary>
        private void GetClientConnections()
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
                    Thread newClientThread = new Thread(() => ManageUserLogin(clientCounter, client));
                    Console.WriteLine(String.Format("Client {0} connected to the server", clientCounter));

                    // Start the new client's thread.
                    newClientThread.Start();

                }
            } catch(SocketException){
                Console.WriteLine("Address already in use!");
                Thread.Sleep(1000);
            }
        }
        /// <summary>
        /// Here is where the user will login to the system, using previous credentials.
        /// if they have not before made an account, they will be prompted to.
        /// </summary>
        /// <param name="clientNumber">Client number.</param>
        /// <param name="client">Client socket connection</param>
        /// 
        private void ManageUserLogin(int clientNumber, TcpClient client)
        {
            try
            {
                // Loop until the user has made a correct choice
                while (true)
                {
                    
                    // Presenting and sending the menu to the user, allow them to make their choice on what to do.
                    NetworkStream stream = client.GetStream();
                    SendMessage(LoginMenu.LoginMenuString, stream);
          
                    // Read the users input
                    string userInput = RecieveMessage(client.GetStream());

                    // Checking if the user's input is within the login menu choice constraints, this allows us to pass
                    // a correct input into the function to determine what the user wants to do.
                    if (LoginMenu.verifyLoginMenuChoice(userInput))
                    {
                        // Send off the input to the login menu, to allow the user to login.
                        var loginInformation = LoginMenu.ManageUserChoice(userInput, client, clientNumber);
                        bool shouldUserQuit = loginInformation.Item1;
                        User userAccount = loginInformation.Item2;

                        // If the user wants to quit
                        if (shouldUserQuit)
                        {
                            SendMessage("Your session has been ended. Type Exit to terminate program.\n", client.GetStream());
                            // If the user wants to quit, we simply terminate the connection.
                            client.GetStream().Close();
                            client.Close();
                            client.Dispose();
                            // Return to close this thread that the user is operating on.
                            return;
                        }

                        // The user wants to view the menu again
                        else if ((!shouldUserQuit) && (userAccount == null))
                        {
                            continue;
                        }
                        // The user is logged in
                        else
                        {
                            // TODO: Main menu stuff, add user to current client list.
                        }


                    }
                    else
                    {
                        // Tell the current client to try again.
                        SendMessage("Invalid Input, try agian.", stream);
                    }
                } 
            }
            catch (IOException ex)
            {
                Console.WriteLine("Unable to read data from client. ABORTING." + ex);
            }

        }
        /// <summary>
        /// Sends a message to the specified client's input stream
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <param name="clientStream">The clients input stream of which the message is being sent..</param>
        public static void SendMessage(string message, NetworkStream clientStream)
        {
            byte[] textBuffer = Encoding.ASCII.GetBytes(message);
            // Output the menu
            clientStream.Write(textBuffer, 0, textBuffer.Length);

            Console.WriteLine("Message sent");
        }

        public static string RecieveMessage(NetworkStream stream)
        {
            byte[] textBuffer = new byte[MAX_MESSAGE];

            int response = stream.Read(textBuffer, 0, textBuffer.Length);
            // Converting the bytes to a manipulatiable string
            return Encoding.ASCII.GetString(textBuffer, 0, response);
        }
    }
}
