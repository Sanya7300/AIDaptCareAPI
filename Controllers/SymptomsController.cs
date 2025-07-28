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
                var researchDocs = await _researchDocumentService.SearchByTagsAsync(input.Symptoms);

                // 3. Call RAG-enabled AI prediction service
                var (predictedCondition, remedies) = await _aiPredictionService
                    .PredictConditionAndRemediesAsync(input.Symptoms, history, researchDocs);

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
                    research = researchDocs.Select(d => new { d.Title, d.Content })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        
    }
    public class SymptomInputModel
    {
        public string Username { get; set; }
        public List<string> Symptoms { get; set; }
    }
}