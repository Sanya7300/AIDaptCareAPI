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
        private readonly ResearchDocumentService _researchDocumentService;

        public SymptomController(
            SymptomService symptomService,
            IAiPredictionService aiPredictionService,
            ResearchDocumentService researchDocumentService)
        {
            _symptomService = symptomService;
            _aiPredictionService = aiPredictionService;
            _researchDocumentService = researchDocumentService;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeSymptoms([FromBody] SymptomInputModel input)
        {
            try
            {
                if (input.Symptoms == null || !input.Symptoms.Any())
                    return BadRequest("No symptoms provided.");

                // 1. Get medical history
                var history = await _symptomService.GetHistoryAsync(input.Username);

                // 2. Get relevant research documents
                //var researchDocs = await _researchDocumentService.SearchByTagsAsync(input.Symptoms);

                // 3. Call RAG-enabled AI prediction service
                var (predictedCondition, remedies) = await _aiPredictionService
                    .PredictConditionAndRemediesAsync(input.Symptoms, history);

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
                    remedies = remedies,
                    history = history,
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

        [HttpPost("ask")]
        public async Task<IActionResult> AskAssistant([FromBody] AssistantInput input)
        {
            var prompt = $"""
   The patient has these symptoms: {string.Join(", ", input.Symptoms)}.
   Diagnosed condition: {input.Condition}.
   Now they ask: "{input.Question}"
   Please answer clearly and briefly like a virtual doctor.
   """;
            var response = await _aiPredictionService.GenerateAssistantResponseAsync(prompt);
            return Ok(new { reply = response });
        }
        public class AssistantInput
        {
            public List<string> Symptoms { get; set; }
            public string Condition { get; set; }
            public string Question { get; set; }
        }
    }

    public class SymptomInputModel
    {
        public string Username { get; set; }
        public List<string> Symptoms { get; set; }
    }
}
