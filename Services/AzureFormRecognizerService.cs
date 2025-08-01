using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using System.Text;

namespace AIDaptCareAPI.Services
{
    public class AzureFormRecognizerService : IAzureFormRecognizerService
    {
        private readonly string _endpoint;
        private readonly string _apiKey;

        public AzureFormRecognizerService(IConfiguration configuration)
        {
            _endpoint = configuration["FormRecognizer:Endpoint"];
            _apiKey = configuration["FormRecognizer:ApiKey"];
        }
        public async Task<string> ExtractTextAsync(Stream fileStream)
        {
            var client = new DocumentAnalysisClient(new Uri(_endpoint), new AzureKeyCredential(_apiKey));
            var operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-document", fileStream);
            var result = operation.Value;
            var textBuilder = new StringBuilder();
            foreach (var page in result.Pages)
            {
                foreach (var line in page.Lines)
                {
                    textBuilder.AppendLine(line.Content);
                }
            }
            return textBuilder.ToString();
        }
    }
}