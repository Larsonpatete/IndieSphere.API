using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace IndieSphere.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration) =>
        services
            //.AddSingleton(new IndieSphereLink(new Uri(configuration.GetValue<string>("ustLink"))))
            .AddMediatorServices();

    private static IServiceCollection AddMediatorServices(this IServiceCollection services) =>
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
    //services.AddMediatR(_ => _
    //    .RegisterServicesFromAssembly(AssemblyMarker.Assembly)
    //    // Order matters! DomainEventsBehavior must be wrapped by TransactionBehavior for its Unit Of Work
    //    .AddOpenBehavior(typeof(TransactionBehavior<,>))
    //    .AddOpenBehavior(typeof(DomainEventsBehavior<,>))
    //);
}