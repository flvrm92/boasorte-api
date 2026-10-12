using Npgsql;
using Testcontainers.PostgreSql;

[assembly: AssemblyFixture(typeof(BoaSorte.Api.IntegrationTests.Infrastructure.PostgresFixture))]

namespace BoaSorte.Api.IntegrationTests.Infrastructure;

/// <summary>One real PostgreSQL container shared by the whole test assembly.</summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18").Build();

    public string ConnectionString => _container.GetConnectionString();

    /// <summary>
    /// Connection string to a fresh database in the shared container, so test classes running in parallel never
    /// see each other's rows. The API's startup migration creates it.
    /// </summary>
    public string NewDatabase() =>
        new NpgsqlConnectionStringBuilder(ConnectionString) { Database = $"t_{Guid.NewGuid():N}" }.ConnectionString;

    public ValueTask InitializeAsync() => new(_container.StartAsync());

    public ValueTask DisposeAsync() => _container.DisposeAsync();
}
