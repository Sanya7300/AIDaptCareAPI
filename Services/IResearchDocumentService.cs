using AIDaptCareAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace AIDaptCareAPI.Services
{
    public interface IResearchDocumentService
    {
        Task<List<ResearchDocument>> GetAllDocumentsAsync();
        Task CreateDocumentAsync(ResearchDocument document);
        Task<List<ResearchDocument>> FindRelevantDocumentsAsync(List<float> embedding, int topN = 3);
    }
}