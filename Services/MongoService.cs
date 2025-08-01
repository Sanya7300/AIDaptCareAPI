using Microsoft.Extensions.Options;
using MongoDB.Driver;
using AIDaptCareAPI.Models;
namespace AIDaptCareAPI.Services
{
    public class MongoService
    {
        private readonly IMongoDatabase _database;
        public MongoService(IConfiguration config)
        {
            var client = new MongoClient(config["DatabaseSettings:ConnectionString"]);
            _database = client.GetDatabase(config["DatabaseSettings:DatabaseName"]);
        }

        public async Task InsertAsync<T>(string collectionName, T document)
        {
            var collection = _database.GetCollection<T>(collectionName);
            await collection.InsertOneAsync(document);
        }
        public IMongoCollection<User> Users => _database.GetCollection<User>("User");
    }
}