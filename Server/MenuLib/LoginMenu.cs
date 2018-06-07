using System;
using Client.UserLib;
using System.Net.Sockets;
using Server.ServerLib;
namespace Server.MenuLib
                
{
    public class LoginMenu
    {
        // The login menu to be output to clients.
        public static string loginMenuString 
        {
            get
            {
                return "[MENU - MAKE A CHOICE]\n[0]Quit\n[1]Login\n[2]New User\n";    
            }
        } 

        // Additional hash used for passwords when storing or checking
        private extraSalt = "TT-|>"

        public static Boolean verifyLoginMenuChoice(string input)
        {
            return (input == "0") || (input == "1") || (input == "2");
        }
        /// <summary>
        /// This is where we create a new user profile or where we allow the user to login, 
        /// if the input is 0 then we are going to return Null as
        /// the user wants to quit. 
        /// </summary>
        /// <param name="input">The use input for the menu.</param>
        /// <param name="clientConnection">The client</param>
        public static User loginUser(string input, TcpClient clientConnection)
        {
            switch (input)
            {
                case("0"):
                    return null;
                // Here we are creating a new user
                case("1"):
                    Console.WriteLine("Logging user information");

                    // User wants to login, so we ask them for their username and password.
                    HostServer.SendMessage("Enter Username - ", clientConnection.GetStream());
                    string username = HostServer.RecieveMessage(clientConnection.GetStream());

                    Console.WriteLine(username);

                    HostServer.SendMessage("Enter Password - ", clientConnection.GetStream());


                    return new User(0, "");
            }

            return new User(0, "dd\t");
            
        }
    }
}
