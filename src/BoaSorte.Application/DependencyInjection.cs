using BoaSorte.Application.Draws.CreateDraw;
using BoaSorte.Application.Draws.GetDraw;
using BoaSorte.Application.Draws.PurgeExpiredDraws;
using Microsoft.Extensions.DependencyInjection;

namespace BoaSorte.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);

        services.AddScoped<CreateDrawHandler>();
        services.AddScoped<GetDrawHandler>();
        services.AddScoped<PurgeExpiredDrawsHandler>();

        return services;
    }
}
