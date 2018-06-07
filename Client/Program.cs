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
            // TODO: restrict input - Regexes, ip lenth checkting etc. 
            while (true)
            {

                Console.WriteLine("Welcome to the messaging service, Enter host address - ");
                string host = Console.ReadLine();
                Console.WriteLine("Enter port to connect on - ");
                string port = Console.ReadLine();

                // Attempting the connection, try catch the many errors
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
