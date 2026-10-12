using BoaSorte.Application.Abstractions;
using BoaSorte.Domain.Common;
using BoaSorte.Infrastructure.Jobs;
using BoaSorte.Infrastructure.Persistence;
using BoaSorte.Infrastructure.Randomness;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BoaSorte.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "Default";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' is not configured (env: ConnectionStrings__{ConnectionStringName}).");

        services.AddDbContext<AppDbContext>(options => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention());

        services.AddScoped<IDrawRepository, DrawRepository>();
        services.AddSingleton<IRandomSource, CryptoRandomSource>();

        services.Configure<PurgeOptions>(configuration.GetSection("Purge"));
        services.AddHostedService<ExpiredDrawsPurgeJob>();

        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("database");

        return services;
    }

    /// <summary>
    /// Applies pending migrations (plan: single instance on the VPS). Migrations must stay backward compatible,
    /// because during a rolling update the previous version briefly runs against the new schema.
    /// </summary>
    public static async Task MigrateDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
    }
}
