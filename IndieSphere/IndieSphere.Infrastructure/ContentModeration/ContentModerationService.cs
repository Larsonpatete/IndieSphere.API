using Microsoft.Extensions.Configuration;

namespace IndieSphere.Infrastructure.ContentModeration;

public interface IContentModerationService 
{
    Task<bool> IsImageExplicitAsync(string imageUrl);
}

public class ContentModerationService : IContentModerationService
{
    private readonly HttpClient _httpClient;
    private readonly string _endpoint;
    private readonly string _apiKey;

    public ContentModerationService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _endpoint = config["AzureContentSafety:Endpoint"];
        _apiKey = config["AzureContentSafety:ApiKey"];
    }

    public async Task<bool> IsImageExplicitAsync(string imageUrl)
    {

        //// Await the download and convert to base64 string
        //var imageBytes = await DownloadImageAsync(imageUrl);
        //string base64Content = imageBytes != null ? Convert.ToBase64String(imageBytes) : null;

        //var requestBody = new
        //{
        //    image = new
        //    {
        //        content = base64Content,
        //        // blobUrl = imageUrl 
        //    },
        //    categories = new[] { "Sexual" },
        //    outputType = "FourSeverityLevels"
        //};

        //var jsonPayload = JsonSerializer.Serialize(requestBody);
        //Console.WriteLine($"Request payload: {jsonPayload}"); // Debug output

        //var content = new StringContent(
        //    JsonSerializer.Serialize(requestBody),
        //    Encoding.UTF8,
        //    "application/json"
        //);

        //_httpClient.DefaultRequestHeaders.Clear();
        //_httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _apiKey);

        //var response = await _httpClient.PostAsync(
        //    $"{_endpoint}/contentsafety/image:analyze?api-version=2023-10-01",
        //    content
        //);

        //if (!response.IsSuccessStatusCode)
        //{
        //    throw new Exception($"Azure Content Safety failed: {response.StatusCode}");
        //}

        //var json = await response.Content.ReadAsStringAsync();

        //using var doc = JsonDocument.Parse(json);
        //var root = doc.RootElement;

        //if (root.TryGetProperty("categoriesAnalysis", out var categories))
        //{
        //    foreach (var category in categories.EnumerateArray())
        //    {
        //        var name = category.GetProperty("category").GetString();
        //        var severity = category.GetProperty("severity").GetInt32();

        //        if (name == "Sexual" && severity >= 2) // medium or high severity
        //        {
        //            return true;
        //        }
        //    }
        //}

        return false;

    }


    private async Task<byte[]> DownloadImageAsync(string imageUrl)
    {
        try
        {
            byte[] imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
            return imageBytes;
        }
        catch (Exception ex)
        {
            // Handle errors (e.g., network failure, invalid URL)
            Console.WriteLine($"Error downloading image: {ex.Message}");
            return null;
        }
    }
}


