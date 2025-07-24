using System.Net.Http.Headers;
using System.Text.Json;
using AIDaptCareAPI.Services;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Collections.Generic;

namespace AIDaptCareAPI.Services
{
    public class AzureAiPredictionService : IAiPredictionService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public AzureAiPredictionService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }
        public async Task<(string Condition, List<string> Remedies)> PredictConditionAndRemediesAsync(List<string> symptoms)
        {
            var endpointBase = _configuration["AzureAI:Endpoint"];
            var deploymentName = _configuration["AzureAI:Deployment"];
            var apiKey = _configuration["AzureAI:ApiKey"];
            var requestUrl = $"{endpointBase}/openai/deployments/{deploymentName}/chat/completions?api-version=2023-05-15";
            var prompt = $@"
Given the following symptoms: {string.Join(", ", symptoms)},
respond with a JSON containing the predicted chronic condition and 3 home remedies.
Respond strictly in this JSON format:
{{
 ""condition"": ""<ConditionName>"",
 ""remedies"": [""Remedy1"", ""Remedy2"", ""Remedy3""]
}}";
            var requestBody = new
            {
                messages = new[]
                {
               new { role = "user", content = prompt }
           },
           max_tokens = 500,
            };
            var requestJson = JsonSerializer.Serialize(requestBody);
            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("api-key",apiKey);
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(responseContent);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
            var parsed = JsonDocument.Parse(content!);
            var condition = parsed.RootElement.GetProperty("condition").GetString();
            var remediesJson = parsed.RootElement.GetProperty("remedies").EnumerateArray();
            var remedies = remediesJson.Select(r => r.GetString()).Where(r => !string.IsNullOrWhiteSpace(r)).ToList();
            return (condition ?? "Unknown", remedies);
        }
    }
}