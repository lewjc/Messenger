using System;
using System.Text;
using MongoDB.Driver;

namespace Server.DatabaseLib
{
    public class MongoConnectBase : IMongoConnectBase
    {

        private static MongoConnectBase _instance;

        protected MongoClient MongoClient;

        private static readonly object Instancelock = new object();

        private const int PORT = 27101;

        private const string URL = "localhost";

        public static MongoConnectBase Instance
        {
            get{
                lock (Instancelock)
                {
                    return _instance ?? (_instance = new MongoConnectBase());
                }
            }
        }

        private MongoConnectBase()
        {
            StringBuilder urlBuilder = new StringBuilder();
            urlBuilder.Append("mongodb://");
            urlBuilder.Append(URL);
            urlBuilder.Append(PORT);
            MongoClient = new MongoClient(new MongoUrl(urlBuilder.ToString()));
        }
    }
}
