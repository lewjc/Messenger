using System;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Text;

namespace Client.ClientLib                
{    
    public class ClientConnection : IDisposable
    {
        private TcpClient client;
        public static readonly int MAX_MESSAGE = 2048;
        private byte[] messageBuffer = new byte[MAX_MESSAGE];
        private readonly NetworkStream clientStream;

        public ClientConnection(TcpClient client)
        {
            this.client = client;
            Thread recieveThread = new Thread(RecieveMessages);
            this.clientStream = client.GetStream();
            recieveThread.Start();
            SendMessage();
        }

        /// <summary>
        /// Sends message to the server
        /// </summary>
        private void SendMessage()
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
                            return;

                    }

                    this.messageBuffer = Encoding.ASCII.GetBytes(userInput);

                    this.clientStream.Write(this.messageBuffer, 0, messageBuffer.Length);
                }
            }

            catch (IOException ex)
            {
                
                Console.WriteLine("Unable to write message - Connection lost.");
                return;

            }
            catch (Exception)
            {
                return;
            }
        }

        /// <summary>
        /// Recieves the messages from the server, running continuously on a seperate thread.
        /// </summary>
        private void RecieveMessages()
        {
            byte[] receivedBytes = new byte[MAX_MESSAGE];
            int byteCount = 0;

            try
            {
                while (true)
                {
                    byteCount = clientStream.Read(receivedBytes, 0, receivedBytes.Length);
                    string message = Encoding.ASCII.GetString(receivedBytes, 0, byteCount);
                    //if (message.Split("-")[0].Equals("CHANGEPORTREQUEST"))
                    //{
                    //    this.client.Connect(client.Client
                    //}
                    Console.Write(message);
                }
            } 

            catch(IOException)
            {
                Console.Write("Unable to read data - Connection lost.");
            }
            catch (ObjectDisposedException)
            {
                return;
            }
        }

        public void Dispose()
        {
            clientStream.Close();
            clientStream.Dispose();
        }
    }
}
