using IndieSphere.Domain.User;
using IndieSphere.Web.Infrastructure.ApiClient;

namespace IndieSphere.Web.Infrastructure.UserClient;

public class UserClient(IApiService api)
{
    private readonly IApiService api = api;

    public async Task<User> GetUser(int Id)
    {
        return await api.GetAsync<User>($"api/users/{Id}");
    }
}
