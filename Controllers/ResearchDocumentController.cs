using Microsoft.AspNetCore.Mvc;
using AIDaptCareAPI.Services;
using System.Security.Claims;

namespace AIDaptCareAPI.Controllers
{
	[ApiController]
	[Route("api/researchdocument")]
	public class ResearchDocumentController : ControllerBase
	{
		private readonly IResearchDocumentService _documentService;
		public ResearchDocumentController(IResearchDocumentService documentService)
		{
			_documentService = documentService;
		}
		[HttpPost("upload")]
		public async Task<IActionResult> Upload(IFormFile file)
		{
			try
			{
				var userId = User.FindFirst(ClaimTypes.Name)?.Value;
				var report = await _documentService.UploadAnalyzeAndSaveAsync(file, userId);
				return Ok(new
				{
					reportId = report.Id,
					fileName = report.FileName,
					extractedText = report.ExtractedText,
					diagnosis = report.Diagnosis,
					uploadedAt = report.UploadedAt
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new { error = ex.Message });
			}
		}
	}
}