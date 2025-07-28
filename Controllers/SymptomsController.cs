using AIDaptCareAPI.Models;
using AIDaptCareAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace AIDaptCareAPI.Controllers
{
    [Route("api/symptom")]
    [ApiController]
    [Authorize]
    public class SymptomController : ControllerBase
    {
        private readonly SymptomService _symptomService;
        private readonly IAiPredictionService _aiPredictionService;
        public SymptomController(SymptomService symptomService, IAiPredictionService aiPredictionService)
        {
            _symptomService = symptomService;
            _aiPredictionService = aiPredictionService;
        }
        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeSymptoms([FromBody] SymptomInputModel input)
        {
            try
            {
                if (input.Symptoms == null || !input.Symptoms.Any())
                    return BadRequest("No symptoms provided.");
                var (predictedCondition, remedies) = await _aiPredictionService.PredictConditionAndRemediesAsync(input.Symptoms);
                var record = new SymptomRecord
                {
                    Username = input.Username,
                    Symptoms = input.Symptoms,
                    PredictedCondition = predictedCondition,
                    Remedies = remedies,
                    Timestamp = DateTime.UtcNow
                };
                _symptomService.Create(record);
                return Ok(new
                {
                    condition = predictedCondition,
                    remedies = remedies
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("history/{username}")]
        public async Task<IActionResult> GetHistory(string username)
        {
            var records = await _symptomService.GetHistoryAsync(username);
            return Ok(records);
        }
        private List<string> GetRemedies(string condition)
        {
            return condition.ToLower() switch
            {
                "diabetes" => new List<string> { "Eat a balanced diet", "Exercise regularly", "Monitor blood sugar" },
                "hypertension" => new List<string> { "Reduce salt intake", "Regular physical activity", "Limit alcohol" },
                "kidney disorder" => new List<string> { "Stay hydrated", "Limit protein intake", "Avoid NSAIDs" },
                _ => new List<string> { "Please consult a medical professional" }
            };
        }
    }
    public class SymptomInputModel
    {
        public string Username { get; set; }
        public List<string> Symptoms { get; set; }
    }
}