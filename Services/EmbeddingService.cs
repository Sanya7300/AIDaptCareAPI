//using Azure;
//using Microsoft.Extensions.Configuration;
//using Azure.AI.OpenAI;

//namespace AIDaptCareAPI.Services
//{
//    public class EmbeddingService : IEmbeddingService
//    {
//        private readonly OpenAIClient _client;

//        public EmbeddingService(IConfiguration config)
//        {
//            var endpoint = config["AzureAI:EmbeddedEndpoint"];
//            var apiKey = config["AzureAI:EmbeddedApiKey"];
//            _client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredentials(apiKey));
//        }

//        public async Task<IReadOnlyList<float>> GetEmbeddingAsync(string text)
//        {
//            var options = new EmbeddingsOptions("text-embedding-ada-002", new[] { text });
//            var response = await openAIClient.GetEmbeddingsAsync(options);
//            return response.Value.Data[0].Embedding.ToList();
//        }
//    }
//}

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
namespace AIDaptCareAPI.Services
{
    public class EmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _endpoint;
        private readonly string _apiKey;
        private readonly string _deploymentName;
        public EmbeddingService(IConfiguration config)
        {
            _httpClient = new HttpClient();
            _endpoint = config["AzureAI:EmbeddedEndpoint"];
            _apiKey = config["AzureAI:EmbeddedApiKey"];
            _deploymentName = config["AzureAI:EmbeddingDeployment"];
            if (string.IsNullOrWhiteSpace(_endpoint) || string.IsNullOrWhiteSpace(_apiKey) || string.IsNullOrWhiteSpace(_deploymentName))
            {
                throw new ArgumentException("Missing Azure OpenAI configuration.");
            }
        }
        public async Task<List<float>> GetEmbeddingAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<float>();
            string url = $"{_endpoint}/openai/deployments/{_deploymentName}/embeddings?api-version=2023-05-15";
            var payload = new
            {
                input = text
            };
            var request = new HttpRequestMessage(HttpMethod.Post, _endpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("api-key", _apiKey);
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            using var responseStream = await response.Content.ReadAsStreamAsync();
            var jsonDoc = await JsonDocument.ParseAsync(responseStream);
            var embeddingArray = jsonDoc.RootElement
                .GetProperty("data")[0]
                .GetProperty("embedding")
                .EnumerateArray()
                .Select(x => (float)x.GetDouble())
                .ToList();
            return embeddingArray;
        }
    }
}