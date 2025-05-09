using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace IndieSphere.Web.Infrastructure.ApiClient;

public abstract class ApiClientBase
{
    protected readonly HttpClient HttpClient;
    protected readonly JsonSerializerOptions JsonOptions;

    protected ApiClientBase(HttpClient httpClient)
    {
        HttpClient = httpClient;
        JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    protected async Task<T> GetFromJsonAsync<T>(string url)
    {
        return await HttpClient.GetFromJsonAsync<T>(url, JsonOptions);
    }

    protected async Task<HttpResponseMessage> PostAsJsonAsync<T>(string url, T data)
    {
        return await HttpClient.PostAsJsonAsync(url, data, JsonOptions);
    }
}