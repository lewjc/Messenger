using System;
namespace Server.MenuLib
{
    public class Menu
    {
        public static Boolean verifyLoginMenuChoice(string input)
        {
            return (input == "0") || (input == "1") || (input == "2");
        }

        public static void LoginMenu()
        {
            
        }
    }
}
