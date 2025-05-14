using Newtonsoft.Json;

namespace IndieSphere.Web.Infrastructure.ApiClient;

public static class HttpRequestHandler
{
    public static async Task ExecuteRequest(this Task<HttpResponseMessage> executable)
    {
        try
        {
            var message = await Task.Run(() => executable);

        }
        catch (Exception ex)
        {
            // Log the exception here if needed
            throw new Exception("An error occurred while executing the request.", ex);
        }
    }

    public static async Task<T> ExecuteRequest<T>(this Task<HttpResponseMessage> executable)
    {
        try
        {
            var message = await Task.Run(() => executable);
            return JsonConvert.DeserializeObject<T>(await message.Content.ReadAsStringAsync());

        }
        catch (Exception e)
        {
            throw new Exception("An error occurred while executing the request.", e);
        }
    }
}

