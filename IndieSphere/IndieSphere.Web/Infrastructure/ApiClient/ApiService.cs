namespace IndieSphere.Web.Infrastructure.ApiClient;

public interface IApiService
{
    Task<T> GetAsync<T>(string endpoint);
    Task GetAsync(string endpoint);
    Task Post(string endpoint);
    //Task<HttpResponseMessage> Post<T>(string endpoint, T data, CancellationToken token);
}
public class ApiService(HttpClient httpClient) : IApiService
{
    private readonly HttpClient httpClient = httpClient;
    public async Task<T> GetAsync<T>(string endpoint)
    {
        return await httpClient.GetAsync(endpoint).ExecuteRequest<T>();
    }

    public async Task GetAsync(string endpoint)
    {
        await httpClient.GetAsync(endpoint).ExecuteRequest();
    }

    public async Task Post<T>(string endpoint, T data, CancellationToken token = default) where T : HttpContent =>
        await httpClient.PostAsync(endpoint, data, token).ExecuteRequest();

    public async Task Post(string endpoint) =>
        await httpClient.PostAsync(endpoint, null).ExecuteRequest();
}
