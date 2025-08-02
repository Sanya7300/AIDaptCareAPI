using AIDaptCareAPI.Models;

namespace AIDaptCareAPI.Services
{
    public interface IEmbeddingService
    {
        Task<IReadOnlyList<float>> GetEmbeddingAsync(string text);
    }
}