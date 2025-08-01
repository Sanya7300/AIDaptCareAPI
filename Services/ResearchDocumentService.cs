using AIDaptCareAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIDaptCareAPI.Services
{
    //public class ResearchDocumentService
    //{
    //    private readonly IMongoCollection<ResearchDocument> _documents;
    //    public ResearchDocumentService(IOptions<DatabaseSettings> dbSettings)
    //    {
    //        var client = new MongoClient(dbSettings.Value.ConnectionString);
    //        var database = client.GetDatabase(dbSettings.Value.DatabaseName);
    //        _documents = database.GetCollection<ResearchDocument>(dbSettings.Value.ResearchCollectionName);
    //    }

    //    public async Task<List<ResearchDocument>> SearchByTagsAsync(List<string> tags)
    //    {
    //        return await _documents.Find(d => d.Tags.Any(tag => tags.Contains(tag))).ToListAsync();
    //    }
    //}
    public class ResearchDocumentService : IResearchDocumentService
    {
        private readonly IAzureFormRecognizerService _recognizerService;
        private readonly IAiPredictionService _aiPredictionService;
        private readonly MongoService _mongoService;
        public ResearchDocumentService(
           IAzureFormRecognizerService recognizerService,
           IAiPredictionService aiPredictionService,
           MongoService mongoService)
        {
            _recognizerService = recognizerService;
            _aiPredictionService = aiPredictionService;
            _mongoService = mongoService;
        }
        public async Task<MedicalReport> UploadAnalyzeAndSaveAsync(IFormFile file, string userId = null)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Invalid file");
            using var stream = file.OpenReadStream();
            var text = await _recognizerService.ExtractTextAsync(stream);
            var diagnosis = await _aiPredictionService.GenerateDiagnosisFromDocumentAsync(text);
            var report = new MedicalReport
            {
                FileName = file.FileName,
                ExtractedText = text,
                Diagnosis = diagnosis,
                UserId = userId
            };
            await _mongoService.InsertAsync("MedicalReports", report);
            return report;
        }
    }
}