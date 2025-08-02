using AIDaptCareAPI.Models;

namespace AIDaptCareAPI.Services
{
    public interface IEmbeddingService
    {
        Task<List<float>> GetEmbeddingAsync(string text);
    }
}