using System.Net;
using BoaSorte.Api.IntegrationTests.Infrastructure;
using Testcontainers.PostgreSql;

namespace BoaSorte.Api.IntegrationTests.Health;

public class HealthEndpointTests(PostgresFixture postgres)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Health_returns_200_when_database_is_reachable()
    {
        await using var factory = new BoaSorteApiFactory(postgres.NewDatabase());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/health", Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Health_returns_503_when_database_goes_away()
    {
        // A dedicated container: the API starts (and migrates) normally, then the database disappears.
        await using var database = new PostgreSqlBuilder("postgres:18").Build();
        await database.StartAsync(Ct);
        await using var factory = new BoaSorteApiFactory(database.GetConnectionString() + ";Timeout=2");
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/health", Ct)).StatusCode);

        await database.StopAsync(Ct);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, (await client.GetAsync("/api/health", Ct)).StatusCode);
    }
}
