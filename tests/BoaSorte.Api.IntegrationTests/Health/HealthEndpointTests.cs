using System.Net;
using BoaSorte.Api.IntegrationTests.Infrastructure;

namespace BoaSorte.Api.IntegrationTests.Health;

public class HealthEndpointTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Health_returns_200_when_database_is_reachable()
    {
        await using var factory = new BoaSorteApiFactory(postgres.ConnectionString);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/health", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Health_returns_503_when_database_is_unreachable()
    {
        await using var factory = new BoaSorteApiFactory(
            "Host=localhost;Port=1;Database=none;Username=none;Password=none;Timeout=2");
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/health", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }
}
