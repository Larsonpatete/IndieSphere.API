using Dapper;
using IndieSphere.Domain.Users;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace IndieSphere.Infrastructure.Users;
public interface IUserRepository
{
    Task<User?> FindBySpotifyIdAsync(string spotifyId);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
}
public class UserRepository : IUserRepository
{
    private readonly IDbConnection _connection;

    public UserRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<User?> FindBySpotifyIdAsync(string spotifyId)
    {
        const string sql = "SELECT * FROM Users WHERE SpotifyId = @SpotifyId";
        return await _connection.QuerySingleOrDefaultAsync<User>(sql, new { SpotifyId = spotifyId });
    }

    public async Task AddAsync(User user)
    {
        const string sql = @"
            INSERT INTO Users (Id, SpotifyId, DisplayName, Email, SpotifyAccessToken, SpotifyRefreshToken, AccessTokenExpiresAt, CreatedAt, LastLogin)
            VALUES (@Id, @SpotifyId, @DisplayName, @Email, @SpotifyAccessToken, @SpotifyRefreshToken, @AccessTokenExpiresAt, @CreatedAt, @LastLogin);
        ";
        await _connection.ExecuteAsync(sql, user);
    }

    public async Task UpdateAsync(User user)
    {
        const string sql = @"
            UPDATE Users SET
                DisplayName = @DisplayName,
                Email = @Email,
                SpotifyAccessToken = @SpotifyAccessToken,
                SpotifyRefreshToken = @SpotifyRefreshToken,
                AccessTokenExpiresAt = @AccessTokenExpiresAt,
                LastLogin = @LastLogin
            WHERE Id = @Id;
        ";
        await _connection.ExecuteAsync(sql, user);
    }
}
