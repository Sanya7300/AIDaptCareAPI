using System.Security.Claims;
using AIDaptCareAPI.Services;
using AIDaptCareAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

[Route("api/report")]
public class ReportController : ControllerBase
{
    private readonly IAzureFormRecognizerService _formRecognizerService;
    private readonly MedicalReportService _medicalReport;
    public ReportController(
        IAzureFormRecognizerService formRecognizerService,
        MedicalReportService medicalReport)
    {
        _formRecognizerService = formRecognizerService;
        _medicalReport = medicalReport;
    }
    [HttpPost("upload")]
    public async Task<IActionResult> UploadReport(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        using var stream = file.OpenReadStream();
        var extractedText = await _formRecognizerService.ExtractTextAsync(stream);
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var report = new MedicalReport
        {
            FileName = file.FileName,
            ExtractedText = extractedText,
            UserId = username
        };
        await _medicalReport.CreateAsync(report);
        return Ok(new { extractedText });
    }
}