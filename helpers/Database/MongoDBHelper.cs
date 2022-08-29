using helpers.Database.Models;
using MongoDB.Driver;
using System;
using System.Text;

namespace helpers.Database
{
    public class MongoDBHelper : IMongoDBHelper
    {
        private readonly MongoClient _client;
        private readonly IMongoDatabase _db;
        public MongoDBHelper(DatabaseConnections connections)
        {
            _client = new MongoClient(connections.Mongo.Server);
            _db = _client.GetDatabase(connections.Mongo.Database);
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
