using System;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Client.ClientLib
                
{
    public class ClientConnection
    {
        private TcpClient client;
        public static readonly int MAX_MESSAGE = 2048;
        private byte[] messageBuffer = new byte[MAX_MESSAGE];
        public NetworkStream clientStream;


        public ClientConnection(TcpClient client)
        {
            this.client = client;
            clientStream = client.GetStream();

            Thread recieveThread = new Thread(recieveMessages);
            recieveThread.Start();
            sendMessage();

        }

        /// <summary>
        /// Sends message to the server
        /// </summary>
        private void sendMessage()
        {
            try
            {
                while (true)
                {
                    // Read the message to send
                    string userInput = Console.ReadLine();

                    switch (userInput.ToLower())
                    {
                        case "exit":
                            this.client.GetStream().Close();
                            this.client.Close();
                            this.client.Dispose();
                            return;

                    }

                    this.messageBuffer = Encoding.ASCII.GetBytes(userInput);

                    this.clientStream.Write(this.messageBuffer, 0, messageBuffer.Length);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("Unable to write message - Connection lost.");
            }
            catch (ObjectDisposedException)
            {
            }

        }

        /// <summary>
        /// Recieves the messages from the server, running continuoulsy on a seperate thread.
        /// </summary>
        private void recieveMessages()
        {
            
            byte[] receivedBytes = new byte[MAX_MESSAGE];
            int byteCount = 0;

            try
            {
                while (true)
                {
                    byteCount = clientStream.Read(receivedBytes, 0, receivedBytes.Length);
                    Console.Write(Encoding.ASCII.GetString(receivedBytes, 0, byteCount));
                }

            } 
            catch(IOException)
            {
                Console.Write("Unable to read data - Connection lost.");
            }
            catch (ObjectDisposedException)
            {
            }

        }

    }
}
