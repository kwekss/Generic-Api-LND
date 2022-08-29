using MongoDB.Driver;

namespace helpers.Database
{
    public interface IMongoDBHelper
    {
        IMongoCollection<T> GetCollection<T>(string collectionName, string sessionKey);
        IMongoCollection<T> GetCollection<T>(string collectionName);
    }
}