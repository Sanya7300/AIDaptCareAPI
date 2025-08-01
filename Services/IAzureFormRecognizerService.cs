using AIDaptCareAPI.Models;

namespace AIDaptCareAPI.Services
{
    public interface IAzureFormRecognizerService
    {
        Task<string> ExtractTextAsync(Stream fileStream);
    }
}