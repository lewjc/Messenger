using System;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using Client.Client.UserLib;
using System.Text;
using System.Collections.Generic;

namespace Client.Client
                
{
    public class ClientConnection
    {
        private TcpClient client;
        public static readonly int MAX_MESSAGE = 2048;
        private byte[] messageBuffer = new byte[MAX_MESSAGE];
        public NetworkStream clientStream;
        private User user;

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
            while(true)
            {
                string userInput = Console.ReadLine();

                switch(userInput.ToLower())
                {
                    case "exit":
                        this.client.Close();
                        break;
                }
                this.messageBuffer = Encoding.ASCII.GetBytes(userInput);

                this.clientStream.Write(this.messageBuffer,0, messageBuffer.Length);
            }
        }

        private void recieveMessages()
        {
            
            byte[] receivedBytes = new byte[1024];
            int byte_count = 0;

            try
            {
                while (true)
                {
                    byte_count = clientStream.Read(receivedBytes, 0, receivedBytes.Length);
                    Console.Write(Encoding.ASCII.GetString(receivedBytes, 0, byte_count));
                }

            } catch(Exception e)
            {
                Console.Write(e);
            }
        }

    }
}
