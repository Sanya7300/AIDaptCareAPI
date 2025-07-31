using System.Net.Http.Headers;
using System.Text.Json;
using AIDaptCareAPI.Services;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Collections.Generic;
using AIDaptCareAPI.Models;
using Newtonsoft.Json;

namespace AIDaptCareAPI.Services
{
    public class AzureAiPredictionService : IAiPredictionService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        public AzureAiPredictionService(HttpClient httpClient, IConfiguration configuration, IHttpClientFactory _httpClientFactory)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpClientFactory = _httpClientFactory;
        }

        public async Task<string> GenerateAssistantResponseAsync(string prompt)
        {
            var endpointBase = _configuration["AzureAI:Endpoint"];
            var deploymentName = _configuration["AzureAI:Deployment"];
            var apiKey = _configuration["AzureAI:ApiKey"];
            var requestUrl = $"{endpointBase}/openai/deployments/{deploymentName}/chat/completions?api-version=2023-05-15";
            var requestBody = new
            {
                messages = new[]
                {
           new { role = "system", content = "You are a friendly medical AI assistant." },
           new { role = "user", content = prompt }
       },
                temperature = 0.7,
                max_tokens = 300
            };

            var requestJson = JsonConvert.SerializeObject(requestBody);
            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("api-key", apiKey);
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonDoc = await response.Content.ReadAsStringAsync();
            var parsed = JsonDocument.Parse(jsonDoc);
            var result = parsed.RootElement
                              .GetProperty("choices")[0]
                              .GetProperty("message")
                              .GetProperty("content")
                              .GetString();
            return result ?? "Sorry, I couldn’t understand that.";
        }

        public async Task<(string Condition, List<string> Remedies)> PredictConditionAndRemediesAsync(
            List<string> symptoms, List<SymptomRecord> history, List<ResearchDocument> researchDocs)
        {
            var endpointBase = _configuration["AzureAI:Endpoint"];
            var deploymentName = _configuration["AzureAI:Deployment"];
            var apiKey = _configuration["AzureAI:ApiKey"];
            var requestUrl = $"{endpointBase}/openai/deployments/{deploymentName}/chat/completions?api-version=2023-05-15";

            // Format history
            var historyText = history != null && history.Any()
                ? string.Join("\n", history.Select(h =>
                    $"- Date: {h.Timestamp:yyyy-MM-dd}, Symptoms: {string.Join(", ", h.Symptoms)}, Condition: {h.PredictedCondition}"))
                : "No prior medical history.";

            // Format research docs (use hyperlink if available, else content)
            var researchText = researchDocs != null && researchDocs.Any()
                ? string.Join("\n", researchDocs.Select(d =>
                    $"- {d.Title}: {(d.GetType().GetProperty("Content") != null ? d.GetType()?.GetProperty("Content")?.GetValue(d) : d.Content)}"))
                : "No relevant research documents found.";

            var prompt = $@"
    Given the following:
    Symptoms: {string.Join(", ", symptoms)}
    Medical History:
    {historyText}

    Relevant Research:
    {researchText}

            //    Based on the above, respond with a JSON containing the predicted chronic condition and 3 home remedies.
            //Respond strictly in this JSON format:
            //{{
            // ""condition"": ""<ConditionName>"",
            // ""remedies"": [""Remedy1"", ""Remedy2"", ""Remedy3""]
            //}}";
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
            var requestJson = JsonConvert.SerializeObject(requestBody);
            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("api-key", apiKey);
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