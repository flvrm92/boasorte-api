using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace BoaSorte.Api.IntegrationTests.Infrastructure;

public sealed class BoaSorteApiFactory(string connectionString, Action<IWebHostBuilder>? configure = null)
    : WebApplicationFactory<Program>
{
    /// <summary>Starts the API with a controllable clock.</summary>
    public static BoaSorteApiFactory WithClock(string connectionString, FakeTimeProvider clock, Action<IWebHostBuilder>? configure = null) =>
        new(connectionString, builder =>
        {
            builder.ConfigureTestServices(services => services.AddSingleton<TimeProvider>(clock));
            configure?.Invoke(builder);
        });

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", connectionString);

        // Requests from the test server share one client address; only the rate limit tests use the real limit.
        builder.UseSetting("RateLimiting:CreateDraw:PermitLimit", "10000");

        // Same container validation as Development, so a missing registration fails here and not at startup.
        builder.UseDefaultServiceProvider(options =>
        {
            options.ValidateOnBuild = true;
            options.ValidateScopes = true;
        });

        configure?.Invoke(builder);
    }
}
