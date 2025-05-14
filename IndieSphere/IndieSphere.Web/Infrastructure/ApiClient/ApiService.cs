namespace IndieSphere.Web.Infrastructure.ApiClient;

public interface IApiService
{
    Task<T> GetAsync<T>(string endpoint);
    //Task<HttpResponseMessage> Post<T>(string endpoint, T data, CancellationToken token);
}
public class ApiService(HttpClient httpClient) : IApiService
{
    private readonly HttpClient httpClient = httpClient;
    public async Task<T> GetAsync<T>(string endpoint)
    {
        return await httpClient.GetAsync(endpoint).ExecuteRequest<T>();
    }

    public async Task Post<T>(string endpoint, T data, CancellationToken token = default) where T : HttpContent =>
        await httpClient.PostAsync(endpoint, data, token).ExecuteRequest();
}
