using IndieSphere.Domain.User;
using IndieSphere.Infrastructure.Users;
using MediatR;

namespace IndieSphere.Application.Features.Users.Queries;

public class GetUserHandler(IGetUsers getUsers) : IRequestHandler<GetUserQuery, User>
{
    private readonly IGetUsers getUsers = getUsers;
    public async Task<User> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await getUsers.GetUserByIdAsync(request.UserId);
        return user;
    }
}

public sealed record GetUserQuery(int UserId) : IQuery<User>;