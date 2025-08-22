using IndieSphere.Infrastructure.Users;
using MediatR;

namespace IndieSphere.Application.Features.Users;

public sealed record CreateOrUpdateUserCommand(string SpotifyId, string DisplayName, string Email, string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAt) : ICommand<Guid>;

public class CreateOrUpdateUserHandler(IUserRepository userRepository) : IRequestHandler<CreateOrUpdateUserCommand, Guid>
{
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<Guid> Handle(CreateOrUpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindBySpotifyIdAsync(request.SpotifyId);

        if (user is null)
        {
            user = new Domain.Users.User
            {
                SpotifyId = request.SpotifyId,
                DisplayName = request.DisplayName,
                Email = request.Email,
                SpotifyAccessToken = request.AccessToken,
                SpotifyRefreshToken = request.RefreshToken,
                AccessTokenExpiresAt = request.AccessTokenExpiresAt,
                LastLogin = DateTime.UtcNow
            };
            await _userRepository.AddAsync(user);
        }
        else
        {
            user.DisplayName = request.DisplayName;
            user.Email = request.Email;
            user.SpotifyAccessToken = request.AccessToken;
            user.SpotifyRefreshToken = request.RefreshToken;
            user.AccessTokenExpiresAt = request.AccessTokenExpiresAt;
            user.LastLogin = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
        }

        return user.Id; 
    }
}
