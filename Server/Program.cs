using System;
using Server.ServerLib;

namespace MessagingApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            // Get the server instance, this starts the server
            HostServer server = HostServer.ServerInstance;        
        }
    }
}
