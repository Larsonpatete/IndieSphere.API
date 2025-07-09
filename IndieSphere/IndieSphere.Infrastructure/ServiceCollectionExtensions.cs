using System.Data;
using IndieSphere.Infrastructure.ContentModeration;
using IndieSphere.Infrastructure.LastFm;
using IndieSphere.Infrastructure.NLP;
using IndieSphere.Infrastructure.Search;
using IndieSphere.Infrastructure.Spotify;
using IndieSphere.Infrastructure.Users;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace IndieSphere.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string connectionString)
    {
        TypeHandlers.Register(); // add mapping to domain specific types

        services
            .AddScoped<IDbConnection>(container => container.GetRequiredService<SqlConnection>())
            .AddScoped(_ => new SqlConnection(connectionString))
            .AddTransient<IGetUsers, GetUsers>()
            .AddTransient<INlpService, NlpService>()
            .AddTransient<ISearchService, SearchService>()
            .AddTransient<ISpotifyService, SpotifyService>()
            .AddTransient<ILastFmService, LastFmService>()
            .AddTransient<IContentModerationService, ContentModerationService>()
            ;

        //var reposWithKeys = typeof(IRepository<,>);
        //services
        //    .AddImplementations(
        //        AssemblyMarker.Assembly,
        //        (t) => t.IsInterface && t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == reposWithKeys),
        //        ServiceLifetime.Transient)
        //    .AddImplementations(AssemblyMarker.Assembly, typeof(IRepository<>), ServiceLifetime.Transient) // register all implementations of IRepository<T>
        //;
        return services;
    }
}
