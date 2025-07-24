using AIDaptCareAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
namespace AIDaptCareAPI.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;
        public UserService(IOptions<DatabaseSettings> dbSettings)
        {
            var client = new MongoClient(dbSettings.Value.ConnectionString);
            var database = client.GetDatabase(dbSettings.Value.DatabaseName);
            _users = database.GetCollection<User>(dbSettings.Value.UserCollectionName);
        }
        public User GetByUsername(string username)
        {
            return _users.Find(user => user.Username == username).FirstOrDefault();
        }
        public void Create(User user)
        {
            _users.InsertOne(user);
        }
        public bool ValidateUser(string username, string password)
        {
            // You should hash and salt passwords in production!
            return _users.Find(user => user.Username == username && user.Password == password).Any();
        }
    }
}