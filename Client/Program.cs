using System;
using System.Net.Sockets;
using System.Net;
using Client.ClientLib;

namespace Client
{
    class Program
    {
        /// <summary>
        /// The entry point of the Client, where the program control starts and ends.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        static void Main(string[] args)
        {
            // First we need to get the host and port for the server that we are connecting to
            // TODO: restrict input - Regexes, ip length checking etc. 
            while (true)
            {
                string host;
                string port;

                if (string.IsNullOrEmpty(args[0]))
                {
                    Console.WriteLine("Welcome to the messaging service, Enter host address - ");
                    host = Console.ReadLine();
                }
                else
                {
                    host = args[0];
                }

                if (string.IsNullOrEmpty(args[1]))
                {
                    Console.WriteLine("Enter port to connect on - ");
                    port = Console.ReadLine();
                }
                else
                {
                    port = args[1];
                }


                if (host.ToLower().Equals("localhost"))
                {
                    // The server is hosted in this machine, get local IPV4 and use that as the port.
                    host = "127.0.0.1";
                }

                // Attempting the connection, try catch errors
                try
                {
                    TcpClient client = new TcpClient();
                    client.Connect(IPAddress.Parse(host), Int32.Parse(port));
                    ClientConnection clientConnection = new ClientConnection(client);
                }
                catch (SocketException)
                {
                    // If the port is not open or incorrect
                    Console.WriteLine(String.Format("Socket error occured - Connection refused at {0}:{1} - Maybe port or IP is incorrect?", host, port));
                }
                catch (FormatException e)
                {
                    Console.WriteLine(e + "\n");
                }
            }         
        }
    }
}
