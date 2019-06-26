using System;
using System.IO;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Server.DatabaseLib
{
    public class Database
    {
        private static Database databaseInstance = null;

        // Login information for the database connection.
        private readonly string host;
        private readonly uint port;
        private readonly string database = "messenger";
        private readonly string user =        "messenger_admin";
        private readonly string password = "pvdZiOOvU2MgasvM";

        // String to establish the connection to the database with
        public string ConnectionString { get; private set; }

        /// <summary>
        /// Initializes a new instance of the Database class.
        /// </summary>
        private Database()
        {
            MySqlConnectionStringBuilder connectionBuilder = new MySqlConnectionStringBuilder();


            // Building up the connection string for the database connection
            connectionBuilder.Server = host;
            connectionBuilder.Database = database;
            connectionBuilder.UserID = user;
            connectionBuilder.Password = password;
            connectionBuilder.Port = port;
            connectionBuilder.SslMode = MySqlSslMode.None;

            ConnectionString = connectionBuilder.ConnectionString;
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static Database Instance
        {
            get
            {
                if (databaseInstance == null)
                {
                    databaseInstance = new Database();
                }

                return databaseInstance;

            }
        }

        /// <summary>
        /// Loads the database info.
        /// </summary>
        public static void LoadDatabaseInfo()
        {
            string currentPath = Path.Combine(Directory.GetCurrentDirectory(), @"DatabaseLib/dbconfig.json");
            Console.WriteLine(currentPath);
            try
            {
                using (var file = File.OpenText(currentPath))
                {
                    List<string> jsonValues = new List<string>();

                    try
                    {
                        using (JsonTextReader jsonReader = new JsonTextReader(file))
                        {
                            while (jsonReader.Read())
                            {
                                // If we have a JSON value, store it.
                                // This catches values such as the start of an array and start object values, which
                                // we have no use for.
                                if (jsonReader.Value != null)
                                {
                                    jsonValues.Add((string) jsonReader.Value);
                                }

                            }
                        }
                    }

                    catch (JsonReaderException e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("File does not exist");
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("Directory doesn't exist");
            }
            Console.WriteLine(String.Format("Database connection established: Name - {0} Port - {1}",Database.Instance.database, Database.Instance.port));
        }

    }
} 
