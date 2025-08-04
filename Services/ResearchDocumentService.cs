using AIDaptCareAPI.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace AIDaptCareAPI.Services
{
    public class ResearchDocumentService : IResearchDocumentService
    {
        private readonly IMongoCollection<ResearchDocument> _researchDocumentCollection;
        public ResearchDocumentService(IConfiguration config, IOptions<DatabaseSettings> dbSettings)
        {
            var client = new MongoClient(dbSettings.Value.ConnectionString);
            var database = client.GetDatabase(dbSettings.Value.DatabaseName);
            _researchDocumentCollection = database.GetCollection<ResearchDocument>(dbSettings.Value.ResearchCollectionName);
        }
        public async Task<List<ResearchDocument>> GetAllDocumentsAsync()
        {
            return await _researchDocumentCollection.Find(_ => true).ToListAsync();
        }

        public async Task CreateDocumentAsync(ResearchDocument document)
        {
            await _researchDocumentCollection.InsertOneAsync(document);
        }
        public async Task<List<ResearchDocument>> FindRelevantDocumentsAsync(List<float> embedding, int topN = 3)
        {
            var allDocs = await _researchDocumentCollection.Find(_ => true).ToListAsync();
            var matches = allDocs
                .Where(d => d.Embedding != null && d.Embedding.Count == embedding.Count)
                .Select(d => new
                {
                    Doc = d,
                    Score = CosineSimilarity(embedding, d.Embedding)
                })
                .OrderByDescending(x => x.Score)
                .Take(topN)
                .Select(x => x.Doc)
                .ToList();
            return matches;
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
            return (float)(dot / (Math.Sqrt(normA) * Math.Sqrt(normB) + 1e-10));
        }
    }
}