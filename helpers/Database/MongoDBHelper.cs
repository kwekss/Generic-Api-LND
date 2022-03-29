using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace helpers.Database
{
    public class MongoDBHelper
    {
        private readonly string _collectionName;
        private readonly MongoClient _client;
        private readonly IMongoDatabase _db;
        public MongoDBHelper(IConfiguration config)
        {
            string dataBaseName = config["MongoDatabase"];
            string mongoServer = config["MongoServer"];
            _collectionName = config["MongoCollection"];

            _client = new MongoClient(mongoServer);
            _db = _client.GetDatabase(dataBaseName);
        }
    }
}
