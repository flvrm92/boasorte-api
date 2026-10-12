using BoaSorte.Api.IntegrationTests.Infrastructure;
using BoaSorte.Application.Abstractions;
using BoaSorte.Domain.Draws;
using BoaSorte.Infrastructure.Randomness;
using Microsoft.Extensions.DependencyInjection;

namespace BoaSorte.Api.IntegrationTests.Persistence;

public sealed class DrawRepositoryTests(PostgresFixture postgres) : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 10, 12, 18, 0, 0, TimeSpan.Zero);
    private static readonly CryptoRandomSource Random = new();
    private readonly BoaSorteApiFactory _factory = new(postgres.NewDatabase());

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public ValueTask InitializeAsync()
    {
        _ = _factory.Server; // start the host: applies migrations
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync() => _factory.DisposeAsync();

    private async Task<T> InScope<T>(Func<IDrawRepository, Task<T>> action)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        return await action(scope.ServiceProvider.GetRequiredService<IDrawRepository>());
    }

    private async Task Save(Draw draw)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<IDrawRepository>().AddAsync(draw, Ct);
    }

    private static Draw ListDraw(DateTimeOffset now, bool showParticipants = true)
    {
        var pool = ListPool.From(["Ana", "ana", "Bruno", "", "Carla", "Daniel"], removeDuplicates: true).Value;
        var settings = new DrawSettings(WinnersCount.Create(3).Value, RemoveDuplicates: true, showParticipants);
        return Draw.Conduct(DrawTitle.Create("Rifa do churrasco").Value, pool, settings, Random, now).Value;
    }


    [Fact]
    public async Task A_list_draw_survives_a_round_trip_unchanged()
    {
        var draw = ListDraw(Now);
        await Save(draw);

        var loaded = await InScope(r => r.GetByPublicIdAsync(draw.PublicId, Ct));

        Assert.NotNull(loaded);
        Assert.Equal(draw.Id, loaded.Id);
        Assert.Equal(draw.PublicId, loaded.PublicId);
        Assert.Equal(draw.Title, loaded.Title);
        Assert.Equal(DrawKind.List, loaded.Kind);
        Assert.Equal(draw.Settings, loaded.Settings);
        Assert.Null(loaded.Range);
        Assert.Equal(6, loaded.SubmittedCount);
        Assert.Equal(4, loaded.EligibleCount);
        Assert.Equal(draw.Participants, loaded.Participants);
        Assert.Equal(draw.Winners, loaded.Winners); // same values in the same order
        Assert.Equal(draw.CreatedAt, loaded.CreatedAt);
        Assert.Equal(draw.ExpiresAt, loaded.ExpiresAt);
    }

    [Fact]
    public async Task A_range_draw_survives_a_round_trip_unchanged()
    {
        var range = NumberRange.Create(NumberRange.MinValue, NumberRange.MaxValue).Value;
        var settings = new DrawSettings(WinnersCount.Create(5).Value, RemoveDuplicates: false, ShowParticipants: true);
        var draw = Draw.Conduct(null, new RangePool(range), settings, Random, Now).Value;
        await Save(draw);

        var loaded = await InScope(r => r.GetByPublicIdAsync(draw.PublicId, Ct));

        Assert.NotNull(loaded);
        Assert.Equal(DrawKind.Range, loaded.Kind);
        Assert.Equal(range, loaded.Range);
        Assert.Null(loaded.Title);
        Assert.Empty(loaded.Participants);
        Assert.Equal(2_000_000_001L, loaded.EligibleCount);
        Assert.Equal(draw.Winners, loaded.Winners);
    }

    [Fact]
    public async Task Unknown_public_id_returns_null()
    {
        Assert.Null(await InScope(r => r.GetByPublicIdAsync(PublicId.Parse("zzzzzzzzzzzz").Value, Ct)));
    }

    [Fact]
    public async Task Deletes_only_draws_that_expired_before_the_cutoff()
    {
        var old = ListDraw(Now.AddYears(-3));
        var recent = ListDraw(Now);
        await Save(old);
        await Save(recent);

        var deleted = await InScope(r => r.DeleteExpiredBeforeAsync(Now, Ct));

        Assert.Equal(1, deleted);
        Assert.Null(await InScope(r => r.GetByPublicIdAsync(old.PublicId, Ct)));
        Assert.NotNull(await InScope(r => r.GetByPublicIdAsync(recent.PublicId, Ct)));
    }
}
