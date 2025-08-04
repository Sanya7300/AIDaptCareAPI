using AIDaptCareAPI.Models;

namespace AIDaptCareAPI.Services
{
    public interface IAiPredictionService
    {
        Task<ComprehensivePredictionResult> PredictComprehensiveMedicalInsightAsync(
   List<string> symptoms,
   string reportText,
   List<SymptomRecord> similarCases,
   List<ResearchDocument> researchDocs,
   List<SymptomRecord> history);
        Task<string> GenerateAssistantResponseAsync(string prompt);
        //Task<string> GenerateDiagnosisFromDocumentAsync(string prompt);
    }
}