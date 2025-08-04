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
        public async Task<List<SymptomRecord>> FindSimilarEmbeddingsAsync(List<float> inputEmbedding, int topN = 5)
        {
            var allRecords = await _symptoms.Find(_ => true).ToListAsync();
            var recordsWithSimilarity = allRecords
                .Where(r => r.Embedding != null && r.Embedding.Count == inputEmbedding.Count)
                .Select(r => new
                {
                    Record = r,
                    Similarity = CosineSimilarity(inputEmbedding, r.Embedding)
                })
                .OrderByDescending(x => x.Similarity)
                .Take(topN)
                .Select(x => x.Record)
                .ToList();
            return recordsWithSimilarity;
        }
        private float CosineSimilarity(List<float> a, List<float> b)
        {
            float dot = 0, normA = 0, normB = 0;
            for (int i = 0; i < a.Count; i++)
            {
                dot += a[i] * b[i];
                normA += a[i] * a[i];
                normB += b[i] * b[i];
            }
            return (float)(dot / (System.Math.Sqrt(normA) * System.Math.Sqrt(normB) + 1e-10));
        }
    }
}