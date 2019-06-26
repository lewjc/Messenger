using System;
using System.Net.Sockets;
using Client.ClientLib;

namespace Client.UserLib
{
    public class User
    {
        public int Number { get; set; }

        public string Username { get; set; }

        public int Permissions { get; set; }

        public string FirstName{ get; set; }

        public bool IsOnline { get; set; }

        public string SessionID { get; set; }

        public bool IsInChatroom { get; set; }

        // Reference to the users currently open TCP Client, use the stream to send messages to.
        public TcpClient Connection { get; set; }

        public User()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Client.UserLib.User"/> class.
        /// </summary>
        /// <param name="number">The user's current number in the server session..</param>
        /// <param name="username">The user's username.</param>
        /// <param name="permissions">The user's permissions.</param>
        /// <param name="firstname">The user's first name</param>
        public User(int number, string username, string firstname, int permissions)
        {
            this.Number = number;
            this.Username = username;
            this.Permissions = permissions;
            this.FirstName = firstname;
            this.IsOnline = true;
            this.SessionID = new Guid().ToString();
        }
    }
}
