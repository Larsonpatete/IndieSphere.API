using System.Data;
using IndieSphere.Infrastructure.ContentModeration;
using IndieSphere.Infrastructure.Data;
using IndieSphere.Infrastructure.LastFm;
using IndieSphere.Infrastructure.NLP;
using IndieSphere.Infrastructure.Search;
using IndieSphere.Infrastructure.Security;
using IndieSphere.Infrastructure.Spotify;
using IndieSphere.Infrastructure.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IndieSphere.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("IndieSphereDb");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'IndieSphereDb' not found.");
        }

        // Register DbContext for Entity Framework Core with the connection string.
        services.AddDbContext<IndieSphereDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly("IndieSphere.Infrastructure");
            }));

        // Register a transient IDbConnection for Dapper.
        // A new connection will be created each time it's requested.
        services.AddTransient<IDbConnection>(provider => new SqlConnection(connectionString));

        TypeHandlers.Register(); // add mapping to domain specific types

        services
            // .AddTransient<INlpService, NlpService>()
            .AddTransient<ISearchService, SearchService>()
            .AddTransient<ISpotifyService, SpotifyService>()
            .AddTransient<ILastFmService, LastFmService>()
            // .AddTransient<IContentModerationService, ContentModerationService>()
            .AddScoped<ITokenService, TokenService>()
            .AddScoped<IUserRepository, UserRepository>()
            ;
        return services;
    }
}
