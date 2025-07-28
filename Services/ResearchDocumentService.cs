using AIDaptCareAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIDaptCareAPI.Services
{
    public class ResearchDocumentService
    {
        private readonly IMongoCollection<ResearchDocument> _documents;
        public ResearchDocumentService(IOptions<DatabaseSettings> dbSettings)
        {
            var client = new MongoClient(dbSettings.Value.ConnectionString);
            var database = client.GetDatabase(dbSettings.Value.DatabaseName);
            _documents = database.GetCollection<ResearchDocument>(dbSettings.Value.ResearchCollectionName);
        }

        public async Task<List<ResearchDocument>> SearchByTagsAsync(List<string> tags)
        {
            return await _documents.Find(d => d.Tags.Any(tag => tags.Contains(tag))).ToListAsync();
        }
    }
}