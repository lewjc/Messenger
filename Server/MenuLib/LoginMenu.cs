using System;
using Client.UserLib;
using System.Net.Sockets;
using Server.ServerLib;
using Server.DatabaseLib;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
            
namespace Server.MenuLib
                
{
    public class LoginMenu
    {
        // The login menu to be output to clients.
        public static string LoginMenuString 
        {
            get
            {
                return "[MENU - MAKE A CHOICE]\n[0]Quit\n[1]Login\n[2]New User\n";    
            }
        } 

        public static Boolean verifyLoginMenuChoice(string input)
        {
            return (input == "0") || (input == "1") || (input == "2");
        }

        /// <summary>
        /// This is where we create a new user profile or where we allow the user to login, 
        /// if the input is 0 then we are going to return Null as
        /// the user wants to quit. 
        /// </summary>
        /// <returns>
        /// Returns a tuple of a boolean and a user object. First value is a bool
        /// to determine if the user wants to quit or if they just want to view the login menu again.
        /// </returns>
        /// <param name="input">The use input for the menu.</param>
        /// <param name="clientConnection">The client</param>
        /// <param name="currentClientNumber">The number of the client in the current server session.</param>
        public static Tuple<bool, User> ManageUserChoice(string input, TcpClient clientConnection, int currentClientNumber)
        {
            // Used to determine if the user wants to quit or actually 
            bool shouldQuit = true;
            switch (input)
            {
                case("0"):
                    return new Tuple<bool, User>(shouldQuit, null);
                // Here we are logging in the user
                case("1"):
                    return new Tuple<bool, User>(shouldQuit = false, LoginUser(clientConnection, currentClientNumber));
                // Here we are creating a new user
                case("2"):
                    RegisterNewUser(clientConnection, currentClientNumber);
                    break;

                default:
                    Console.WriteLine("Invalid user choice input");
                    break;
            }

            return new Tuple<bool, User>(shouldQuit, null);
    
        }

        /// <summary>
        /// Logs in the user
        /// </summary>
        /// <returns> Instance of the user object if they login or null else.</returns>
        /// <param name="clientConnection">Client connection.</param>
        /// <param name="currentClientNumber">Current client number.</param>
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

                try
                {
                    string getUsernameQuery = "SELECT username, password, first_name, user_type FROM user_accounts WHERE username = ?username;";

                    // Holds info grabbed from the db
                    string dbPassword = null;
                    string dbUsername = null;
                    int permissions = 0;
                    string firstname = null;

                    using (MySqlConnection con = new MySqlConnection(Database.instance.ConnectionString))
                    {
                        
                        con.Open();

                        using (var cmd = con.CreateCommand())

                        {
                            cmd.CommandText = getUsernameQuery;

                            cmd.Parameters.Add("?username", MySqlDbType.VarChar).Value = username;
                          
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

                                    // Used to decide if the user wants to continue with the login,
                                    bool continueLogin = true;
                                    // Use this as a form of user input error checking, so we only get y or n.
                                    bool decisionNotMade = true;

                                    while (decisionNotMade)
                                    {
                                        string userResponse = HostServer.RecieveMessage(clientConnection.GetStream());

                                        switch (userResponse.ToLower())
                                        {
                                            // the user wants to try again, so we can just break the loop and continue
                                            case ("y"):
                                                continueLogin = true;
                                                decisionNotMade = false;
                                                break;

                                            case ("n"):
                                                continueLogin = false;
                                                decisionNotMade = false;
                                                break;

                                            // If the choice is not what we want, send the message saying incorrect response and
                                            // try again.
                                            default:
                                                HostServer.SendMessage("Invalid input, try again.", clientConnection.GetStream());
                                                continue;
                                        }
                                    }
                                    // If the user does not want to continue attempting the login then we return a null user
                                    // 
                                    // the user is not logged in.
                                    if (!continueLogin)
                                    {
                                        return null;
                                    }
                                    // Here we go back to the start of the loop and ask for a username again.
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

        /// <summary>
        /// Registers a new user into the messenger database
        /// </summary>
        /// <param name="clientConnection">Client tcp connection.</param>
        /// <param name="clientNumber">Client number for this current session.</param>
        private static void RegisterNewUser(TcpClient clientConnection, int clientNumber)
        {
            // Display the info asking for the user to input registration 

            string registationMenu = "[REGISTRATION MENU]\nUsername must be between 3 and 20 characters\n";
            HostServer.SendMessage(registationMenu, clientConnection.GetStream());


            /////////////////////////////
            ///  USERNAME VALIDATION  ///
            /////////////////////////////

            bool usernameValid = false;
            bool usernameExists = false;
            string newUsername = null;

            // While the username the user inputs is invalid, ask for a correct one.
            while (!usernameValid)
            {
                HostServer.SendMessage("Please enter username: ", clientConnection.GetStream());

                string inputUsername = HostServer.RecieveMessage(clientConnection.GetStream());
                int usernameLength = inputUsername.Length;

                // We must also check to see if that username already exists for another user.

                using (MySqlConnection connection = new MySqlConnection(Database.instance.ConnectionString))
                {
                    connection.Open();
                    using (MySqlCommand usernameExistsCommand = connection.CreateCommand())
                    {
                        usernameExistsCommand.CommandText = "SELECT username from user_accounts WHERE username = ?username";
                        usernameExistsCommand.Parameters.Add("?username", MySqlDbType.VarChar).Value = inputUsername;

                        // Here we execute the command to the database and if it returns a non null value, the username exists
                        string usernameInDatabase = (string) usernameExistsCommand.ExecuteScalar();

                        if (usernameInDatabase != null)
                        {
                            usernameExists = true;   
                        }
                       
                    }
                    
                }

                // Here we put our username validity conditionals, space for more can be added
                // if there are more constraints to be added at a later date
                // such as certain strings not being accepted.

                if (usernameLength >= 3 && usernameLength <= 20)
                {
                    // Satisfies our length requirement
                    usernameValid = true;
                    newUsername = inputUsername;
                    HostServer.SendMessage("Username Accepted!\n", clientConnection.GetStream());
                }
                else if (usernameExists)
                {
                    HostServer.SendMessage("Username already exists! Please choose another.", clientConnection.GetStream());
                    continue;
                }
                else
                {
                    HostServer.SendMessage("Username does not satisfy requirements, please insert a new one.", clientConnection.GetStream());
                    continue;
                }
            }


            /////////////////////////////
            ///  PASSWORD VALIDATION  ///
            /////////////////////////////

            Console.WriteLine("Username validated");

            bool passwordValidated = false;
            string encryptedPassword = null;

            HostServer.SendMessage("Password must be atleast 6 characters and contain 1 lowercase, 1 uppercase.\n", clientConnection.GetStream());

            // While the user has not input a valid password, we must ask and check for a correct one.
            while (!passwordValidated)
            {
                HostServer.SendMessage("Enter Password: ", clientConnection.GetStream());
                string userInputPassword = HostServer.RecieveMessage(clientConnection.GetStream());

                if (Regex.IsMatch(userInputPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,15}$"))
                {
                    // If the password contains a lower case and uppercase and at least 1 number.
                    passwordValidated = true;
                    encryptedPassword = BCrypt.Net.BCrypt.HashPassword(PasswordEncryptor.GenerateNewPassword(userInputPassword));
                }
                else
                {
                    // The password is invalid
                    HostServer.SendMessage("\nPassword invalid. Try again.\n", clientConnection.GetStream());
                    continue;
                }
            }

            /////////////////////
            ///   USER INFO   ///
            /////////////////////


        }
    }
}
