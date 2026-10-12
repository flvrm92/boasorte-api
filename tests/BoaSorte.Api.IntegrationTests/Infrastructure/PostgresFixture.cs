using Testcontainers.PostgreSql;

[assembly: AssemblyFixture(typeof(BoaSorte.Api.IntegrationTests.Infrastructure.PostgresFixture))]

namespace BoaSorte.Api.IntegrationTests.Infrastructure;

/// <summary>One real PostgreSQL container shared by the whole test assembly.</summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18").Build();

    public string ConnectionString => _container.GetConnectionString();

    public ValueTask InitializeAsync() => new(_container.StartAsync());

    public ValueTask DisposeAsync() => _container.DisposeAsync();
}
