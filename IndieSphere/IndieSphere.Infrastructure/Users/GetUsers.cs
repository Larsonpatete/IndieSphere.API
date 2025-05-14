using Dapper;
using IndieSphere.Domain.User;
using System.Data;


namespace IndieSphere.Infrastructure.Users;

public interface IGetUsers
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User> GetUserByIdAsync(int userId);
    Task<User> GetUserByUsernameAsync(string username);
}

public class GetUsers(IDbConnection connection) : IGetUsers
{
    private readonly IDbConnection connection = connection;

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        var results = await connection.QueryAsync<User>(Sql.GetAllUsers);
        return results;
    }

    public async Task<User> GetUserByIdAsync(int userId)
    {
        var result = await connection.QuerySingleOrDefaultAsync<User>(Sql.GetUserById, new { Id = userId });
        return result;
    }

    public async Task<User> GetUserByUsernameAsync(string username)
    {
        var result = await connection.QuerySingleOrDefaultAsync<User>(Sql.GetUserByUsername, new { Username = username });
        return result;
    }

    private static class Sql
    {
        public const string GetAllUsers = "SELECT * FROM Users";
        public const string GetUserById = "SELECT * FROM Users WHERE Id = @Id";
        public const string GetUserByUsername = "SELECT * FROM Users WHERE Username = @Username";
    }
}
