using Azure;
using Microsoft.Extensions.Configuration;
using Azure.AI.OpenAI;

namespace AIDaptCareAPI.Services
{
    public class EmbeddingService : IEmbeddingService
    {
        private readonly OpenAIClient openAIClient;

        public EmbeddingService(IConfiguration config)
        {
            var endpoint = _configuration["AzureAI:EmbeddedEndpoint"];
            var apiKey = _configuration["AzureAI:EmbeddedApiKey"];
            openAIClient = new OpenAIClient(new Uri(endpoint), new AzureKeyCredentials(apiKey));
        }

        public async Task<IReadOnlyList<float>> GetEmbeddingAsync(string text)
        {
            var options = new EmbeddingsOptions("text-embedding-ada-002", new[] { text });
            var response = await openAIClient.GetEmbeddingsAsync(options);
            return response.Value.Data[0].Embedding.ToList();
        }
    }
}