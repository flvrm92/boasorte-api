using Microsoft.Extensions.DependencyInjection;

namespace BoaSorte.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);

        // Use case handlers are registered together with their adapters and endpoints (001, Fase 3 / T033).

        return services;
    }
}
