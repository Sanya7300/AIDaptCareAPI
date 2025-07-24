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
            var client = new MongoClient(config["MongoDB:ConnectionString"]);
            _database = client.GetDatabase(config["MongoDB:DatabaseName"]);
        }
        public IMongoCollection<User> Users => _database.GetCollection<User>("User");
    }
}