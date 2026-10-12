using BoaSorte.Domain.Common;
using BoaSorte.Domain.Draws;
using BoaSorte.Domain.Tests.TestDoubles;

namespace BoaSorte.Domain.Tests.Draws;

public class DrawTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 12, 18, 0, 0, TimeSpan.Zero);

    private static ListPool Pool(params string[] names) => ListPool.From(names, removeDuplicates: false).Value;

    private static DrawSettings Settings(int winners, bool removeDuplicates = false, bool showParticipants = true) =>
        new(WinnersCount.Create(winners).Value, removeDuplicates, showParticipants);

    private static SeededRandomSource AnyRandom() => new(123);

    [Fact]
    public void Conduct_draws_the_winner_picked_by_the_random_source()
    {
        // Floyd for n=3, k=1 asks for one value in [0, 3); then 12 values build the public id.
        var random = new SequenceRandomSource([1, .. Enumerable.Repeat(0L, PublicId.Length)]);

        var draw = Draw.Conduct(null, Pool("Ana", "Bruno", "Carla"), Settings(1), random, Now).Value;

        var winner = Assert.Single(draw.Winners);
        Assert.Equal(new Winner(1, "Bruno"), winner);
        Assert.Equal("000000000000", draw.PublicId.Value);
    }

    [Fact]
    public void Conduct_records_an_auditable_list_draw()
    {
        var title = DrawTitle.Create("Rifa").Value;
        var pool = ListPool.From(["Ana", "ana", "Bruno", "", "Carla"], removeDuplicates: true).Value;

        var draw = Draw.Conduct(title, pool, Settings(2, removeDuplicates: true), AnyRandom(), Now).Value;

        Assert.Equal(DrawKind.List, draw.Kind);
        Assert.Equal(title, draw.Title);
        Assert.Equal(5, draw.SubmittedCount);
        Assert.Equal(3, draw.EligibleCount);
        Assert.Equal(["Ana", "Bruno", "Carla"], draw.Participants.Select(p => p.Value));
        Assert.Null(draw.Range);
        Assert.Equal(Now, draw.CreatedAt);
        Assert.Equal(Now.AddMonths(12), draw.ExpiresAt);
        Assert.NotEqual(Guid.Empty, draw.Id);
    }

    [Fact]
    public void Winners_are_distinct_ordered_and_come_from_the_pool()
    {
        var names = Enumerable.Range(1, 50).Select(i => $"P{i}").ToArray();

        var draw = Draw.Conduct(null, Pool(names), Settings(10), AnyRandom(), Now).Value;

        Assert.Equal(Enumerable.Range(1, 10), draw.Winners.Select(w => w.Position));
        Assert.Equal(10, draw.Winners.Select(w => w.Value).Distinct().Count());
        Assert.All(draw.Winners, w => Assert.Contains(w.Value, names));
    }

    [Fact]
    public void Conduct_draws_numbers_from_a_range_without_storing_participants()
    {
        var range = NumberRange.Create(1, 500).Value;

        var draw = Draw.Conduct(null, new RangePool(range), Settings(2, removeDuplicates: true), AnyRandom(), Now).Value;

        Assert.Equal(DrawKind.Range, draw.Kind);
        Assert.Equal(range, draw.Range);
        Assert.Empty(draw.Participants);
        Assert.Equal(500, draw.EligibleCount);
        Assert.False(draw.Settings.RemoveDuplicates);
        Assert.All(draw.Winners, w => Assert.InRange(int.Parse(w.Value, System.Globalization.CultureInfo.InvariantCulture), 1, 500));
    }

    [Fact]
    public void More_winners_than_eligible_participants_is_rejected_with_the_eligible_count()
    {
        var result = Draw.Conduct(null, Pool("Ana", "Bruno"), Settings(3), AnyRandom(), Now);

        var error = Assert.Single(result.Errors);
        Assert.Equal("draw.not_enough_participants", error.Code);
        Assert.Equal("winnersCount", error.Field);
        Assert.Contains("2", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void A_pool_without_eligible_participants_is_rejected()
    {
        var result = Draw.Conduct(null, Pool("", "  "), Settings(1), AnyRandom(), Now);

        var error = Assert.Single(result.Errors);
        Assert.Equal("draw.no_eligible_participants", error.Code);
        Assert.Equal("participants", error.Field);
    }

    [Fact]
    public void Draw_expires_after_twelve_months()
    {
        var draw = Draw.Conduct(null, Pool("Ana"), Settings(1), AnyRandom(), Now).Value;

        Assert.False(draw.IsExpiredAt(draw.ExpiresAt.AddTicks(-1)));
        Assert.True(draw.IsExpiredAt(draw.ExpiresAt));
    }
}
