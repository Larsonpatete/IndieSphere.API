using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.Extensions.Configuration;

namespace IndieSphere.Infrastructure.NLP;

public interface INlpService
{
    Task<NlpResult> AnalyzeTextAsync(string query);
}
public class NlpService : INlpService
{
    private readonly TextAnalyticsClient _client;
    private readonly string? projectName;
    private readonly string? deploymentName;
    public NlpService(IConfiguration config)
    {
        var endpoint = config["AzureTextAnalytics:Endpoint"];
        var key = config["AzureTextAnalytics:Key"];
        projectName = config["AzureTextAnalytics:ProjectName"];
        deploymentName = config["AzureTextAnalytics:DeploymentName"];
        _client = new TextAnalyticsClient(new Uri(endpoint), new AzureKeyCredential(key));
    }
    public async Task<NlpResult> AnalyzeTextAsync(string query)
    {
        List<string> batchedDocuments = new()
        {
            query
        };

        ClassifyDocumentOperation operation = await _client.SingleLabelClassifyAsync(WaitUntil.Completed, batchedDocuments, projectName, deploymentName);

        await foreach (ClassifyDocumentResultCollection documentsInPage in operation.Value)
        {
            foreach (ClassifyDocumentResult documentResult in documentsInPage)
            {
                if (documentResult.HasError)
                {
                    throw new InvalidOperationException($"Document error code: {documentResult.Error.ErrorCode}, Message: {documentResult.Error.Message}");
                }

                var bestCategory = documentResult.ClassificationCategories
                    .OrderByDescending(c => c.ConfidenceScore)
                    .FirstOrDefault();

                if (!EqualityComparer<ClassificationCategory>.Default.Equals(bestCategory, default))
                {
                    return new NlpResult(bestCategory.Category, (decimal)bestCategory.ConfidenceScore);
                }
            }
        }

        throw new InvalidOperationException("No classification result found.");
    }
}

public record NlpResult(string queryType, decimal confidence);