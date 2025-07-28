using AIDaptCareAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
namespace AIDaptCareAPI.Services
{
    public class SymptomService
    {
        private readonly IMongoCollection<SymptomRecord> _symptoms;
        public SymptomService(IOptions<DatabaseSettings> dbSettings)
        {
            var client = new MongoClient(dbSettings.Value.ConnectionString);
            var database = client.GetDatabase(dbSettings.Value.DatabaseName);
            _symptoms = database.GetCollection<SymptomRecord>(dbSettings.Value.SymptomCollectionName);
        }
        public List<SymptomRecord> GetAll()
        {
            return _symptoms.Find(record => true).ToList();
        }
        public async Task<List<SymptomRecord>> GetHistoryAsync(string username)
        {
            return await _symptoms
                .Find(r => r.Username == username)
                .SortByDescending(r => r.Timestamp)
                .Limit(10)
                .ToListAsync();
        }
        public void Create(SymptomRecord record)
        {
            _symptoms.InsertOne(record);
        }
    }
}