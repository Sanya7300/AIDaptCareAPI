using AIDaptCareAPI.Models;
using AIDaptCareAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace AIDaptCareAPI.Controllers
{
    [Route("api/research")]
    [ApiController]
    [Authorize]
    public class ResearchDocumentController : ControllerBase
    {
        private readonly IResearchDocumentService _researchDocumentService;
        private readonly IEmbeddingService _embeddingService;
        public ResearchDocumentController(
            IResearchDocumentService researchDocumentService,
            IEmbeddingService embeddingService)
        {
            _embeddingService = embeddingService;
            _researchDocumentService = researchDocumentService;
        }
        [HttpGet]
        public async Task<ActionResult<List<ResearchDocument>>> GetAll()
        {
            var documents = await _researchDocumentService.GetAllDocumentsAsync();
            return Ok(documents);
        }

        [HttpPost]
        public async Task<IActionResult> UploadDocument([FromBody] ResearchDocument document)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            document.Embedding = await _embeddingService.GetEmbeddingAsync(document.Content);
            await _researchDocumentService.CreateDocumentAsync(document);
            return Ok(new { message = "Research document uploaded successfully." });
        }
    }
}