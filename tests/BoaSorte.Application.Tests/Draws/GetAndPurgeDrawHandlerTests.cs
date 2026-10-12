using BoaSorte.Application.Draws.GetDraw;
using BoaSorte.Domain.Common;
using static BoaSorte.Application.Tests.Draws.DrawsTestContext;

namespace BoaSorte.Application.Tests.Draws;

public class GetDrawHandlerTests
{
    private readonly DrawsTestContext _context = new();
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Returns_the_same_result_every_time()
    {
        var created = (await _context.Create.HandleAsync(ListCommand(winnersCount: 2), Ct)).Value;

        var first = (await _context.Get.HandleAsync(new GetDrawQuery(created.PublicId), Ct)).Value;
        var second = (await _context.Get.HandleAsync(new GetDrawQuery(created.PublicId), Ct)).Value;

        Assert.Equivalent(created, first, strict: true);
        Assert.Equivalent(first, second, strict: true);
    }

    [Theory]
    [InlineData("aZ3k9QpL0xYt")] // well formed, unknown
    [InlineData("not-an-id")] // malformed
    public async Task Unknown_or_malformed_ids_are_not_found(string publicId)
    {
        var error = Assert.Single((await _context.Get.HandleAsync(new GetDrawQuery(publicId), Ct)).Errors);

        Assert.Equal(ErrorType.NotFound, error.Type);
    }

    [Fact]
    public async Task Expired_draws_are_reported_as_expired()
    {
        var created = (await _context.Create.HandleAsync(ListCommand(), Ct)).Value;
        _context.Time.SetUtcNow(created.ExpiresAt);

        var error = Assert.Single((await _context.Get.HandleAsync(new GetDrawQuery(created.PublicId), Ct)).Errors);

        Assert.Equal(ErrorType.Expired, error.Type);
    }

    [Fact]
    public async Task Hidden_participants_stay_hidden()
    {
        var created = (await _context.Create.HandleAsync(ListCommand(showParticipants: false), Ct)).Value;

        Assert.Null((await _context.Get.HandleAsync(new GetDrawQuery(created.PublicId), Ct)).Value.Participants);
    }
}

public class PurgeExpiredDrawsHandlerTests
{
    private readonly DrawsTestContext _context = new();
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Deletes_only_draws_expired_for_longer_than_the_grace_period()
    {
        await _context.Create.HandleAsync(ListCommand(), Ct); // expires at Start + 12 months
        _context.Time.Advance(TimeSpan.FromDays(40));
        await _context.Create.HandleAsync(ListCommand(), Ct); // expires 40 days later

        _context.Time.SetUtcNow(Start.AddMonths(12) + PurgeExpiredDrawsHandlerGrace() + TimeSpan.FromDays(1));
        var deleted = await _context.Purge.HandleAsync(Ct);

        Assert.Equal(1, deleted);
        Assert.Single(_context.Repository.Draws);
    }

    private static TimeSpan PurgeExpiredDrawsHandlerGrace() =>
        BoaSorte.Application.Draws.PurgeExpiredDraws.PurgeExpiredDrawsHandler.GracePeriod;
}
