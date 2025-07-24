namespace AIDaptCareAPI.Services
{
    public interface IAiPredictionService
    {
        Task<(string Condition, List<string> Remedies)> PredictConditionAndRemediesAsync(List<string> symptoms);
    }
}