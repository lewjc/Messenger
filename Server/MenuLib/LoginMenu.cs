using System;
using Client.UserLib;
using System.Net.Sockets;
using Server.ServerLib;
using Server.DatabaseLib;
using MySql.Data.MySqlClient;
            
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
        /// <param name="currentClientNumber">The number of the client in the current server session./param>
        public static User ManageUserChoice(string input, TcpClient clientConnection, int currentClientNumber)
        {
            switch (input)
            {
                case("0"):
                    return null;
                // Here we are logging in the user
                case("1"):
                    LoginUser(clientConnection, currentClientNumber);
                    break;

                // Here we are creating a new user
                case("2"):

                    break;

                default:
                    Console.WriteLine("Invalid user choice input");
                    break;
            }

            return new User();
    
        }

        private static User LoginUser(TcpClient clientConnection, int currentClientNumber)
        {
            bool notLoggedIn = true;


            while (notLoggedIn)
            {
                Console.WriteLine("Logging user information");

                // User wants to login, so we ask them for their username and password.
                HostServer.SendMessage("Enter Username - ", clientConnection.GetStream());
                string username = HostServer.RecieveMessage(clientConnection.GetStream());

                Console.WriteLine(username);

                HostServer.SendMessage("Enter Password - ", clientConnection.GetStream());
                string password = HostServer.RecieveMessage(clientConnection.GetStream());


                Console.WriteLine(password);
                var database = new Database();
                try
                {
                    string getUsernameQuery = "SELECT username, password, first_name, user_type FROM user_accounts WHERE username = ?username;";

                    // Holds info grabbed from the db
                    string dbPassword = null;
                    string dbUsername = null;
                    int permissions = 0;
                    string firstname = null;

                    using (MySqlConnection con = new MySqlConnection(database.ConnectionString))
                    {
                        con.Open();

                        using (var cmd = con.CreateCommand())

                        {
                            cmd.CommandText = getUsernameQuery;

                            cmd.Parameters.Add("?username", MySqlDbType.VarChar).Value = username;
                            Console.WriteLine(cmd.CommandText);
                            Console.WriteLine(cmd.Parameters);

                            using (var queryReader = cmd.ExecuteReader())
                            {


                                // Using while even though only one value should return as the usernames are unique.
                                while (queryReader.Read())
                                {
                                    try
                                    {
                                        dbUsername = queryReader.GetString(queryReader.GetOrdinal("username"));
                                        dbPassword = queryReader.GetString(queryReader.GetOrdinal("password"));
                                        permissions = queryReader.GetInt32(queryReader.GetOrdinal("permissions"));
                                        firstname = queryReader.GetString(queryReader.GetOrdinal("first_name"));

                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e);
                                    }
                                }

                                // The username doesn't exist in the database or the password is incorrect
                                // so we loop again asking for a new password.

                                if ((dbUsername == null) || (!PasswordEncryptor.CheckPassword(password, dbPassword)))
                                {
                                    // Tell the user the username or password is incorrect. 
                                    // Not too sure whether or not to be specific about which one
                                    // The vagueness in the message increases security though, as 
                                    // Someone trying to guess a user's account might not know which one.
                                    HostServer.SendMessage("Username or password is incorrect, Try again? Y/N\n", clientConnection.GetStream());
                                    string userResponse = HostServer.RecieveMessage(clientConnection.GetStream());



                                    continue;
                                }

                                // If this point is reached, the user has entered successful login information.

                                HostServer.SendMessage(String.Format("Welcome back, {0}", firstname), clientConnection.GetStream());

                                return new User(currentClientNumber, username, firstname, permissions);


                            }

                        }

                    }

                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("Query cannot be executed, please check parameters.");
                }


            }

            // Return null if the loop to login is broken, this means that the user would like to return to the main menu.
            return null;
        }
    }
}
