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
        private readonly IResearchDocumentService _researchDocumentService;
        private readonly IEmbeddingService _embeddingService;
        private readonly MedicalReportService _medicalReportService;

        public SymptomController(
            SymptomService symptomService,
            IAiPredictionService aiPredictionService,
            IResearchDocumentService researchDocumentService,
            IEmbeddingService embeddingService,
            MedicalReportService medicalReportService)
        {
            _symptomService = symptomService;
            _aiPredictionService = aiPredictionService;
            _researchDocumentService = researchDocumentService;
            _embeddingService = embeddingService;
            _medicalReportService = medicalReportService;
        }
        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeSymptoms([FromBody] SymptomInputModel input)
        {
            try
            {
                if ((input.Symptoms == null || !input.Symptoms.Any()) && string.IsNullOrWhiteSpace(input.ReportText))
                    return BadRequest("No symptoms or medical report provided.");

                var history = await _symptomService.GetHistoryAsync(input.Username);

                string reportText = string.Empty;
                if (!string.IsNullOrWhiteSpace(input.Username))
                {
                    List<MedicalReport> report = await _medicalReportService.GetUserReportsAsync(input.Username);
                    if (report != null && report.Any())
                        reportText = report[0].ExtractedText;
                }

                var combinedText = string.Join(", ", input.Symptoms ?? new List<string>()) + " " + (reportText ?? "");
                var combinedEmbedding = await _embeddingService.GetEmbeddingAsync(combinedText);

                var similarCases = await _symptomService.FindSimilarEmbeddingsAsync(combinedEmbedding, topN: 5);

                var similarDocs = await _researchDocumentService.FindRelevantDocumentsAsync(combinedEmbedding, topN: 3);

                var result = await _aiPredictionService.PredictComprehensiveMedicalInsightAsync(
                    input.Symptoms,
                    reportText,
                    similarCases,
                    similarDocs,
                    history
                );

                var record = new SymptomRecord
                {
                    Username = input.Username,
                    Symptoms = input.Symptoms,
                    ReportText = reportText,
                    Condition = result.Condition,
                    PredictedCondition = result.PredictedCondition,
                    Diagnosis = result.Diagnosis,
                    Treatment = result.Treatment,
                    Remedies = result.Remedies,
                    Medicines = result.Medicines,
                    Embedding = combinedEmbedding,
                    Timestamp = DateTime.UtcNow
                };
                _symptomService.Create(record);
                return Ok(result);
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
        public class SymptomInputModel
        {
            public string Username { get; set; }
            public List<string> Symptoms { get; set; }
            public string ReportText { get; set; }
        }
    }
}