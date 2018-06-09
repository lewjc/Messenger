using System;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Data.SqlTypes;

namespace Server.DatabaseLib
{
    public class Database
    {
        // Login information. for the database connection.
        private string host = "138.68.152.134";
        private string database = "messenger";
        private string user = "messenger_admin";
        private string password = "pvdZiOOvU2MgasvM";
        private uint port = 3306;

        // String to establish the connection to the database with
        public string ConnectionString { get; private set; }

        private MySqlConnection Connection;
       
            
        /// <summary>
        /// Initializes a new instance of the Database class.
        /// </summary>
        public Database()
        {
            Connection = null;
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

       

    }
}
