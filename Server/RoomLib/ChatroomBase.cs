using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Client.ClientLib;
using Client.UserLib;
using Dependency;

namespace Server.RoomLib
{
    public class ChatroomBase : SocketStream, IChatRoom
    {
        private readonly int PORT;

        protected ConcurrentDictionary<string, User> currentClientsConnected;

        public bool IsFull { get; set; }

        public bool IsRunning;


        protected int CurrentNumberOfUsers
        {
            get
            {
                return currentClientsConnected.Count();
            }
        }

        protected int MaxUsers
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Server.RoomLib.ChatroomBase"/> class.
        /// </summary>
        /// <param name="maxUsers">Max users.</param>
        /// <param name="port">Port to host the chatroom on</param>
        /// <param name="address">Address of the server</param>
        public ChatroomBase(int maxUsers, int port, IPAddress address)
        {
            MaxUsers = maxUsers;
            PORT = port;
            instanceSocket = new TcpListener(address, port);
            Initialise();
        }


        /// <summary>
        /// Initialise this chatromom.
        /// </summary>
        private void Initialise()
        {
            IsRunning = true;
            currentClientsConnected = new ConcurrentDictionary<string, User>();
            Console.WriteLine("Initialised Chatroom on port:" + PORT + " - Max Users = " + MaxUsers);
        }

        /// <summary>
        /// Broadcast the specified message to all users.
        /// </summary>
        /// <returns>The broadcast.</returns>
        /// <param name="message">Message.</param>
        protected bool Broadcast(string message, string senderID)
        {
            IEnumerable<string> keys = currentClientsConnected.Keys.Where(x => x != senderID);
            message = AppendUsername(message, senderID);
            try
            {
                foreach (string key in keys)
                {
                    SendMessage(message, currentClientsConnected[key].Connection.GetStream());
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Appends the username to the message sent.
        /// </summary>
        /// <returns>The username.</returns>
        /// <param name="message">The message being sent to the users in the chatroom</param>
        /// <param name="senderID">the id of the user</param>
        private string AppendUsername(string message, string senderID)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("[");
            stringBuilder.Append(currentClientsConnected[senderID].Username);
            stringBuilder.Append("]: ");
            stringBuilder.Append(message);
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Adds the user to the chatroom.
        /// </summary>
        /// <param name="user">User.</param>
        public void AddUser(User user)
        {
            if (IsFull)
            {
                SendMessage("This chat room is full, please try to join another one", user.Connection.GetStream());
            }

            string sessionID = user.SessionID;
            if (currentClientsConnected.TryAdd(sessionID, user))
            {
                Console.Write("[" + user.Username + "] Has entered the chatroom, say hi!");
                IsFull = CurrentNumberOfUsers == MaxUsers;
                new Thread(() => PollUserMessages(user)).Start();
            }
        }

        /// <summary>
        /// Polls user messages waiting for them to be recieved.
        /// </summary>
        /// <param name="user">User.</param>
        private void PollUserMessages(User user)
        {
            while (true)
            {
                SpinWait.SpinUntil(() => user.Connection.GetStream().DataAvailable || !user.IsInChatroom);
                if (user.IsInChatroom)
                {
                    Broadcast(AppendUsername(RecieveMessage(user.Connection.GetStream()), user.SessionID), user.SessionID);
                }
                else
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Removes the specified user form the chatroom
        /// </summary>
        /// <param name="user">User.</param>
        public void RemoveUser(User user)
        {
            currentClientsConnected.TryRemove(user.SessionID, out user);
            Console.Write("[" + user.Username + "] Has left the chatroom.");
            user.IsInChatroom = false;
        }
    }
}
