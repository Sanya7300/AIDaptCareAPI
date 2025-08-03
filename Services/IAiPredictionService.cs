using AIDaptCareAPI.Models;

namespace AIDaptCareAPI.Services
{
    public interface IAiPredictionService
    {
        Task<(string PredictedCondition, List<string> Remedies)> PredictConditionAndRemediesAsync(
          List<string> symptoms,
          List<SymptomRecord> similarCases);
        Task<string> GenerateAssistantResponseAsync(string prompt);
        Task<string> GenerateDiagnosisFromDocumentAsync(string prompt);
    }
}