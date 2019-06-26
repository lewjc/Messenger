using System;
using System.Threading;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Collections.Generic;
using Client.UserLib;
using Server.MenuLib;
using Dependency;
using System.Collections.Concurrent;
using Server.RoomLib;

namespace Server.ServerLib
{
    public class HostServer : SocketStream
    {
        // instance of the server class
        private static HostServer instance;

        // IP address of the server to host the messaging system
        private IPAddress HOST { get; set; }

        // Port to open the socket on
        private int PORT { get; set; }

        private ConcurrentDictionary<int, IChatRoom> ChatRooms;

        private IMainMenuLogic mainMenuLogic;

        private readonly ILoginMenuLogic loginMenuLogic;

        // Server property 
        public static HostServer ServerInstance
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
            instanceSocket = new TcpListener(HOST, PORT);
            ChatRooms = new ConcurrentDictionary<int, IChatRoom>();
            while (true)
            {
                try
                {
                    // Send off a new thread to intialise the database information from the json info file.
                    Thread initialiseDatabase = new Thread(DatabaseLib.Database.LoadDatabaseInfo);
                    initialiseDatabase.Start();
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
                instanceSocket.Start();
                Console.WriteLine(String.Format("Server: {0} Init on port {1}", HOST, PORT));

                while (true)
                {
                    // Here we incrememnt the client counter and wait for a new client to connect to the server.
                    clientCounter++;
                    // Waiting for a new client to establish connection
                    client = instanceSocket.AcceptTcpClient();
                    // Load the client onto a new thread

                    Thread newClientThread = new Thread(() => Entry(clientCounter, client));
                    Console.WriteLine(String.Format("Client {0} connected to the server", clientCounter));

                    // Start the new client's thread.
                    newClientThread.Start();
                }
            } catch(SocketException){
                Console.WriteLine("Address already in use!");
                Thread.Sleep(3000);
            }
        }

        private void Entry(int clientNumber, TcpClient client)
        {
            User threadUser = ManageUserLogin(clientNumber, client);
            if (threadUser == null) 
            {
                // The thread is closed.
                return;
            }
            mainMenuLogic = new MainMenuLogic(threadUser);
        }

        /// <summary>
        /// Here is where the user will login to the system, using previous credentials.
        /// if they have not before made an account, they will be prompted to.
        /// </summary>
        /// <param name="clientNumber">Client number.</param>
        /// <param name="client">Client socket connection</param>
        /// 
        private User ManageUserLogin(int clientNumber, TcpClient client)
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
                    if (LoginMenu.VerifyLoginMenuChoice(userInput))
                    {
                        // S    §end off the input to the login menu, to allow the user to login.
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
                            return null;
                        }

                        // The user wants to view the menu again
                        if ((!shouldUserQuit) && (userAccount == null))
                        {
                            continue;
                        }

                        // The user is logged in
                        // Assign the current tcp client connection to this user,
                        userAccount.Connection = client;
                        return userAccount;
                    }
                    // Tell the current client to try again.
                    SendMessage("Invalid Input, try agian.", stream);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("Unable to read data from client. ABORTING.\n" + ex);
                return null;
            }
        }
    }
}
