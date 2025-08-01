using AIDaptCareAPI.Models;

namespace AIDaptCareAPI.Services
{
    public interface IAiPredictionService
    {
        Task<(string Condition, List<string> Remedies)> PredictConditionAndRemediesAsync(List<string> symptoms, List<Models.SymptomRecord> history);
        Task<string> GenerateAssistantResponseAsync(string prompt);
        Task<string> GenerateDiagnosisFromDocumentAsync(string prompt);
    }
}