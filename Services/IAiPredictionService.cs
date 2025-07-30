using AIDaptCareAPI.Models;

namespace AIDaptCareAPI.Services
{
    public interface IAiPredictionService
    {
        Task<(string Condition, List<string> Remedies)> PredictConditionAndRemediesAsync(List<string> symptoms, List<Models.SymptomRecord> history, List<Models.ResearchDocument> researchDocs);
        Task<string> GenerateAssistantResponseAsync(string prompt);
    }
}