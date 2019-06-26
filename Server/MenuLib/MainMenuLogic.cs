using System;
using Client.UserLib;
using Dependency;

namespace Server.MenuLib
{
    public class MainMenuLogic : IMainMenuLogic
    {

        private User User { get; set; }

        public MainMenuLogic(User user)
        {
            User = user;
            ExectueMenuLoop(user);
        }

        private void ExectueMenuLoop(User user)
        {
            SocketStream.SendMessage(MainMenu.MenuString, user.Connection.GetStream());

            bool isInMenu = true;

            while (isInMenu)
            {
                string userChoice = SocketStream.RecieveMessage(user.Connection.GetStream());




            }
        }
    }
}
