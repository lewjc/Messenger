using System;
using System.Data.SqlTypes;
namespace Server.DatabaseLib
{
    public class Database
    {
        private static Database databaseInstance = null;
        // Use this object when wrapping the lock on the database istance grab.
        private static readonly Object obj = new Object();


        private Database()
        {
            // Login information. for the database connection.
            string host = "138.68.152.134";
            string database = "messenger";
            string user = "messenger_admin";
            string password = "pvdZiOOvU2MgasvM";

            // String to establish the connection to the database with
            string connectionString = String.Format(@"Data Source={0};Initial Catalog={1};User ID={2};Password={3}",host, database, user, password);

            try
            {

            }

            // Sql connection exceptions will be thrown here.
            catch(Exception e)
                

        }

        public static Database instance
        {
            get
            {
                // Lock this to make it thread freindly. We might have multiple users
                // Trying to access the database at any one time.
                // Using the Singleton patterna as we only want one connection open to the server at a time.

                {
                    if (databaseInstance == null)
                    {
                        databaseInstance = new Database();
                    }

                    return databaseInstance;
                }

            }
        }


    }
}
