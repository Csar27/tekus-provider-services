using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Common.Messaging;

namespace Tekus.Application;

/// <summary>
/// Composition root of the application layer. Handlers are discovered by scanning the assembly,
/// so adding a new use case is just adding a handler class: there is no growing registration list
/// to maintain by hand.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Registers the dispatcher, the pipeline behaviours, the handlers and the validators.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddValidatorsFromAssembly(assembly, ServiceLifetime.Singleton);

        RegisterHandlers(services, assembly);

        services.TryAddScoped<ISender, Sender>();

        // Order matters: the first registration is the outermost behaviour. Logging wraps
        // validation so a rejected request is still measured and logged.
        services.TryAddEnumerable(
            ServiceDescriptor.Scoped(typeof(IPipelineBehavior<,>), typeof(RequestLoggingBehavior<,>)));
        services.TryAddEnumerable(
            ServiceDescriptor.Scoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>)));

        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Assembly assembly)
    {
        var handlerRegistrations =
            from type in assembly.GetTypes()
            where type is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false }
            from implementedInterface in type.GetInterfaces()
            where implementedInterface.IsGenericType
                  && implementedInterface.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)
            select (Service: implementedInterface, Implementation: type);

        foreach (var (service, implementation) in handlerRegistrations)
        {
            services.TryAddScoped(service, implementation);
        }
    }
}
