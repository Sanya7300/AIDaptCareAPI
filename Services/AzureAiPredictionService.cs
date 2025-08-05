using AIDaptCareAPI.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Globalization;

namespace AIDaptCareAPI.Services
{
    public class AzureAiPredictionService : IAiPredictionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _endpoint;
        private readonly string _deployment;
        private readonly string _apiKey;
        public AzureAiPredictionService(IConfiguration config)
        {
            _httpClient = new HttpClient();
            _endpoint = config["AzureAI:Endpoint"];
            _deployment = config["AzureAI:Deployment"];
            _apiKey = config["AzureAI:ApiKey"];
        }
        public async Task<string> GenerateAssistantResponseAsync(string prompt)
        {
            var url = $"{_endpoint}/openai/deployments/{_deployment}/chat/completions?api-version=2023-05-15";
            var requestBody = new
            {
                messages = new[]
                {
                   new { role = "system", content = "You are a helpful medical assistant." },
                   new { role = "user", content = prompt }
               },
                temperature = 0.4,
                max_tokens = 1024
            };
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("api-key", _apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var resultJson = await response.Content.ReadAsStringAsync();
            var root = JsonDocument.Parse(resultJson).RootElement;
            return root
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
        }
        public async Task<ComprehensivePredictionResult> PredictComprehensiveMedicalInsightAsync(
            List<string> symptoms,
            string reportText,
            List<SymptomRecord> similarCases,
            List<ResearchDocument> researchDocs,
            List<SymptomRecord> history)
        {
            var prompt = new StringBuilder();
            prompt.AppendLine($"Patient symptoms: {string.Join(", ", symptoms ?? new List<string>())}");
            if (!string.IsNullOrWhiteSpace(reportText))
                prompt.AppendLine($"Extracted from medical report: {reportText}");
            if (similarCases.Any())
            {
                prompt.AppendLine("\nSimilar past cases:");
                foreach (var item in similarCases)
                {
                    prompt.AppendLine($"- Symptoms: {string.Join(", ", item.Symptoms)} → Condition: {item.PredictedCondition}");
                }
            }
            if (researchDocs.Any())
            {
                prompt.AppendLine("\nRelevant research findings:");
                foreach (var doc in researchDocs)
                {
                    prompt.AppendLine($"- {doc.Title}: {doc.Summary} (link: {doc.Url})");
                }
            }
            prompt.AppendLine("\nBased on all of the above, predict and explain:");
            prompt.AppendLine("1. In the 'PredictedDiagnosisDisease' field, provide the specific disease name in ONE WORD only (e.g., 'Diabetes', 'Asthma', 'Hypertension').");
            prompt.AppendLine("2. In the 'SuggestedMedicines' field, provide only medicine names as a list of ONE WORD each (e.g., 'Paracetamol', 'Ibuprofen', 'Metformin'). Do not include explanations or full sentences.");

            prompt.AppendLine("3. Strictly give JSON format which can be deserialized.");
            prompt.AppendLine("4. Respond with only valid JSON output — no markdown, no code block, no explanations, no surrounding text.");
            prompt.AppendLine("5. Use standard double quotes (\") for all keys and values. Ensure output is valid JSON that can be pasted directly into a JSON visualizer.");
            prompt.AppendLine(@"
            Respond ONLY in the following JSON object format, where each property is an array of strings with detailed points for each  section:
            {
                ""ComprehensivePredictionResult"": {
                ""LikelyMedicalCondition"": [""..."", ""...""],
                ""DiagnosisSummary"": [""..."", ""...""],
                ""RecommendedTreatment"": [""..."", ""...""],
                ""SuggestedMedicines"": [""..."", ""...""],
                ""HomeRemedies"": [""..."", ""...""],
                ""RecommendedDiagnosticTests"": [""..."", ""...""],
                ""PredictedDiagnosisDisease"": ""
                 }
             }
            ");
            var response = await GenerateAssistantResponseAsync(prompt.ToString());
            using var resp = JsonDocument.Parse(response);
            var root = resp.RootElement.GetProperty("ComprehensivePredictionResult");

            return new ComprehensivePredictionResult
            {
                Condition = root.GetProperty("LikelyMedicalCondition").EnumerateArray().Select(x => x.GetString()).ToList(),
                Diagnosis = root.GetProperty("DiagnosisSummary").EnumerateArray().Select(x => x.GetString()).ToList(),
                Treatment = root.GetProperty("RecommendedTreatment").EnumerateArray().Select(x => x.GetString()).ToList(),
                Medicines = root.GetProperty("SuggestedMedicines").EnumerateArray().Select(x => x.GetString()).ToList(),
                Remedies = root.GetProperty("HomeRemedies").EnumerateArray().Select(x => x.GetString()).ToList(),
                RecommendedTests = root.GetProperty("RecommendedDiagnosticTests").EnumerateArray().Select(x => x.GetString()).ToList(),
                PredictedCondition = root.GetProperty("PredictedDiagnosisDisease").GetString(),
                ResearchLinks = researchDocs.Select(d => d.Url).ToList(),
                History = history
            };
        }
        private string ExtractSection(string text, string sectionTitle)
        {
            var lines = text.Split('\n');
            var sectionHeader = lines.FirstOrDefault(line =>
                line.Trim().ToLower().Contains(sectionTitle.ToLower()));
            if (sectionHeader == null) return "Not specified";
            int index = Array.IndexOf(lines, sectionHeader);
            var sb = new StringBuilder();
            for (int i = index + 1; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("1.") || lines[i].StartsWith("2.") || lines[i].StartsWith("3.") ||
                    lines[i].StartsWith("4.") || lines[i].StartsWith("5."))
                    break;
                sb.AppendLine(lines[i].Trim());
            }
            return sb.ToString().Trim();
        }
        private List<string> ExtractList(string text, string sectionTitle)
        {
            var section = ExtractSection(text, sectionTitle);
            if (string.IsNullOrEmpty(section)) return new List<string>();
            return section.Split(new[] { '\n', '-', '*' }, StringSplitOptions.RemoveEmptyEntries)
                          .Select(s => s.Trim())
                          .Where(s => s.Length > 2)
                          .ToList();
        }

        private string ExtractOneWordCondition(string text)
        {
            var section = ExtractSection(text, "Likely Medical Condition");
            var knownConditions = new[] { "diabetes", "hypertension", "kidney disease", "thyroid", "asthma", "anemia", "infection", "obesity" };
            foreach (var condition in knownConditions)
            {
                if (section.ToLower().Contains(condition))
                    return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(condition);
            }
            var words = section.Split(new[] { ' ', '.', ',', ';', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return words.Length > 0 ? words[0] : "Unknown";
        }
    }
}