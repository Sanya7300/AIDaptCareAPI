using AIDaptCareAPI.Models;

namespace AIDaptCareAPI.Services
{
    public interface IResearchDocumentService
    {
        Task<MedicalReport> UploadAnalyzeAndSaveAsync(IFormFile file, string userId = null);
    }
}