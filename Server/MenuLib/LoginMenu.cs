using System;
using Client.UserLib;
using System.Net.Sockets;
using Server.ServerLib;
using Server.DatabaseLib;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using Dependency;

namespace Server.MenuLib
{
    public static class LoginMenu
    {
        // The login menu to be output to clients.
        public static string LoginMenuString 
        {
            get
            {
                return "\n[MENU - MAKE A CHOICE]\n[0]Quit\n[1]Login\n[2]New User\n";    
            }
        } 

        public static Boolean VerifyLoginMenuChoice(string input)
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
        /// <param name="input">The user input for the menu.</param>
        /// <param name="clientConnection">The client tcp connection</param>
        /// <param name="currentClientNumber">The number of the client in the current server session.</param>
        public static Tuple<bool, User> ManageUserChoice(string input, TcpClient clientConnection, int currentClientNumber)
        {
            // Used to determine if the user wants to quit or actually 
            bool shouldQuit = true;
            switch (input)
            {
                // Here the user     wants to exit.
                case("0"):
                    return new Tuple<bool, User>(shouldQuit, null);
                // Here we are logging in the user
                
                case("1"):
                    // Should quit is false as the user is attempting a login.
                    // login user should return a complete use object. if the user fails to login, null is 
                    // returned, indicating a failed login and is dealt with on the return of this function.

                    return new Tuple<bool, User>(shouldQuit = false, LoginUser(clientConnection, currentClientNumber));
                
                // Here we are creating a new user 
                case("2"):
                    RegisterNewUser(clientConnection);
                    return new Tuple<bool, User>(shouldQuit = false, LoginUser(clientConnection, currentClientNumber));
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
                SocketStream.SendMessage("Enter Username - ", clientConnection.GetStream());
                string username = SocketStream.RecieveMessage(clientConnection.GetStream());

                Console.WriteLine(username);

                SocketStream.SendMessage("Enter Password - ", clientConnection.GetStream());
                string password = SocketStream.RecieveMessage(clientConnection.GetStream());

                Console.WriteLine(password);

                try
                {
                    string getUsernameQuery = "SELECT * FROM user_accounts WHERE username = ?username;";

                    // Holds info grabbed from the db
                    string dbPassword = null;
                    string dbUsername = null;
                    int permissions = 0;
                    string lastname = null;
                    string firstname = null;

                    using (MySqlConnection con = new MySqlConnection(Database.Instance.ConnectionString))
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
                                        permissions = queryReader.GetInt32(queryReader.GetOrdinal("user_type"));
                                        firstname = queryReader.GetString(queryReader.GetOrdinal("first_name"));
                                        lastname = queryReader.GetString(queryReader.GetOrdinal("last_name"));

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
                                    SocketStream.SendMessage("Username or password is incorrect, Try again? Y/N\n", clientConnection.GetStream());

                                    // Used to decide if the user wants to continue with the login,
                                    bool continueLogin = true;

                                    // Use this as a form of user input error checking, so we only get y or n.
                                    bool decisionNotMade = true;

                                    while (decisionNotMade)
                                    {
                                        string userResponse = SocketStream.RecieveMessage(clientConnection.GetStream());

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
                                                SocketStream.SendMessage("Invalid input, try again.", clientConnection.GetStream());
                                                continue;
                                        }
                                    }

                                    // If the user does not want to continue attempting the login then we return a null user
                                    // the user is not logged in.
                                    if (!continueLogin)
                                    {
                                        return null;
                                    }
                                    // Here we go back to the start of the loop and ask for a username again.
                                    continue;
                                    
                                }

                                // If this point is reached, the user has entered successful login information.

                                SocketStream.SendMessage(String.Format("Welcome back, {0}", firstname), clientConnection.GetStream());

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
        private static void RegisterNewUser(TcpClient clientConnection)
        {
            // Display the info asking for the user to input registration 

            string registationMenu = "[REGISTRATION MENU]\nUsername must be between 3 and 20 characters\n";
            SocketStream.SendMessage(registationMenu, clientConnection.GetStream());

            string username = GetNewUsername(clientConnection);
            string encryptedPassword = GetNewPassword(clientConnection);

            // While the username the user inputs is invalid, ask for a correct one.

            /////////////////////
            ///   USER INFO   ///
            /////////////////////

            // To do, get first name, last name and date of birth.
            SocketStream.SendMessage("Please enter your personal information\nWhat is your first name? ", clientConnection.GetStream());

            string firstName = SocketStream.RecieveMessage(clientConnection.GetStream());

            SocketStream.SendMessage("What is your last name? ", clientConnection.GetStream());

            string lastName = SocketStream.RecieveMessage(clientConnection.GetStream());

            // Here we're getting the user's date of birth, going to implement age restrictions for 13 on this messaging service.
            SocketStream.SendMessage("Please enter in format dd/mm/yyyy, dd.mm.yyyy, dd-mm-yyyy.\nWhat is your date of birth? ", clientConnection.GetStream());

            // while a valid dob has not been input.
            bool dateValid = false;
            DateTime realDOB = new DateTime();

            while (!dateValid)
            {
                string dob = SocketStream.RecieveMessage(clientConnection.GetStream());

                // If dob is in form dd/mm/yyyy or dd.mm.yyyy or dd-mm-yyyy, it is correct.
                // TODO: validate against the actual roman calander.(only dates that might not be correct is february 29th or something.) 
                if (Regex.IsMatch(dob, "^([0]?[1-9]|[1|2][0-9]|[3][0|1])[./-]([0]?[1-9]|[1][0-2])[./-]([0-9]{4}|[0-9]{2})$"))
                {
                    try
                    {
                        // convert the dob into a date time object, use this to convert to an sql date object,
                        // then leave the loop to insert data into database.
                        realDOB = DateTime.Parse(dob);
                        break;
                        
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("Unable to parse date, incorrect format.");
                        SocketStream.SendMessage("Try again.", clientConnection.GetStream());
                        continue;
                    }

                }
            }

            try
            {
                // Inserting new user into database
                using (var databaseConnection = new MySqlConnection(Database.Instance.ConnectionString))
                {
                    string insertUserQuery = "INSERT INTO user_accounts(username, password, first_name, last_name, dob, user_since, user_type) VALUES (?username, ?password, ?firstname, ?lastname, ?dob, CURRENT_TIMESTAMP(), 1)";
                    databaseConnection.Open();

                    using (var command = databaseConnection.CreateCommand())
                    {
                        command.CommandText = insertUserQuery;
                        command.Parameters.Add("?username", MySqlDbType.VarChar).Value = username;
                        command.Parameters.Add("?password", MySqlDbType.VarChar).Value = encryptedPassword;
                        command.Parameters.Add("?firstname", MySqlDbType.VarChar).Value = firstName;
                        command.Parameters.Add("?lastname", MySqlDbType.VarChar).Value = lastName;
                        command.Parameters.Add("?dob", MySqlDbType.Date).Value = realDOB;

                        command.ExecuteNonQuery();
                    }

                    databaseConnection.Close();
                    SocketStream.SendMessage("\nUser Registered.\n", clientConnection.GetStream());
                }
            }

            catch (Exception e)
            {
                Console.WriteLine("Error inputting to database - " + e);
            }
        }

        /// <summary>
        /// Gets the new username.
        /// </summary>
        /// <returns>The new username.</returns>
        /// <param name="clientConnection">Client connection.</param>
        private static string GetNewUsername(TcpClient clientConnection)
        {

            /////////////////////////////
            ///  USERNAME VALIDATION  ///
            /////////////////////////////

            bool usernameValid = false;
            bool usernameExists = false;
            string username = null;
          
            
            while (!usernameValid)
            {
                SocketStream.SendMessage("Please enter username: ", clientConnection.GetStream());

                string inputUsername = SocketStream.RecieveMessage(clientConnection.GetStream());
                int usernameLength = inputUsername.Length;

                // We must also check to see if that username already exists for another user.

                SocketStream.SendMessage("Checking username...\n", clientConnection.GetStream());
                using (MySqlConnection connection = new MySqlConnection(Database.Instance.ConnectionString))
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
                    connection.Close();
                }

                // Here we put our username validity conditionals, space for more can be added
                // if there are more constraints to be added at a later date
                // such as certain strings not being accepted.

                if (usernameLength >= 3 && usernameLength <= 20)
                {
                    // Satisfies our length requirement
                    usernameValid = true;
                    username = inputUsername;
                    SocketStream.SendMessage("Username Accepted!\n", clientConnection.GetStream());
                }
                // Username exists already for a user
                else if (usernameExists)
                {
                    SocketStream.SendMessage("Username already exists! Please choose another.", clientConnection.GetStream());
                    continue;
                }
                // Username doesn't fall into our criteria
                else
                {
                    SocketStream.SendMessage("Username does not satisfy requirements, please insert a new one.", clientConnection.GetStream());
                    continue;
                }
            }

            return username;
        }

        public static string GetNewPassword(TcpClient clientConnection)
        { 

            /////////////////////////////
            ///  PASSWORD VALIDATION  ///
            /////////////////////////////

            Console.WriteLine("Username validated");

            // Used to decide if the password is correctly validated.

            bool passwordValidated = false;
            string encryptedPassword = null;

            SocketStream.SendMessage("Password must be between 8 and 15 characters, contain 1 lowercase, 1 uppercase letter and a digit.\n", clientConnection.GetStream());

            // While the user has not input a valid password, we must ask and check for a correct one.
            while (!passwordValidated)
            {

                SocketStream.SendMessage("Enter Password: ", clientConnection.GetStream());
                string userInputPassword = SocketStream.RecieveMessage(clientConnection.GetStream());

                // IF the password has one upper case, one lower case, a number and between 8 and 15 characters
                if (Regex.IsMatch(userInputPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,15}$"))
                {
                    // If the password contains a lower case and uppercase and at least 1 number.
                    passwordValidated = true;
                    encryptedPassword = PasswordEncryptor.GenerateNewPassword(userInputPassword);

                }
                else
                {
                    // The password is invalid
                    SocketStream.SendMessage("\nPassword invalid. Try again.\n", clientConnection.GetStream());
                    continue;
                }
            }

            return encryptedPassword;

        }
    }
}
