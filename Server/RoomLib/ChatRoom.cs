using System;
using System.Net;
using Client.ClientLib;

namespace Server.RoomLib
{
    public class ChatRoom : ChatroomBase
    {
        public ChatRoom(int port, IPAddress address, int maxUsers = 5)  : base(maxUsers, port, address)
        {
            
        }
    }
}
