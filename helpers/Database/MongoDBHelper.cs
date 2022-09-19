using MongoDB.Driver;
using System;
using System.Text;

namespace helpers.Database
{
    public class MongoDBHelper : IMongoDBHelper
    {
        private readonly MongoClient _client;
        private readonly IMongoDatabase _db;
        public MongoDBHelper(IDBHelper dbHelper)
        {
            var mongoConnection = dbHelper.GetConnection("Mongo");
            _client = new MongoClient(mongoConnection.ConnectionString);
            _db = _client.GetDatabase(mongoConnection.Schema);
        }

        public IMongoCollection<T> GetCollection<T>(string collectionName, string sessionKey)
        {
            var number = BitConverter.ToInt32(Encoding.ASCII.GetBytes(sessionKey), 0);
            return _db.GetCollection<T>($"{collectionName}_{number % 20}");
        }
        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _db.GetCollection<T>(collectionName);
        }
    }
}
