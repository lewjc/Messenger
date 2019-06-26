using System;
using System.Net.Sockets;
using System.Text;

namespace Dependency
{

    public abstract class SocketStream
    {
        protected static readonly int MAX_MESSAGE = 2048;

        protected TcpListener instanceSocket;

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
            clientStream.Flush();

            Console.WriteLine("Message sent");
        }

        /// <summary>
        /// Waits to recieve a message
        /// </summary>
        /// <returns>The message.</returns>
        /// <param name="stream">Stream.</param>
        public static string RecieveMessage(NetworkStream stream)
        {
            byte[] textBuffer = new byte[MAX_MESSAGE];

            int response = stream.Read(textBuffer, 0, textBuffer.Length);
            // Converting the bytes to a manipulatiable string
            return Encoding.ASCII.GetString(textBuffer, 0, response);
        }
    }
}
