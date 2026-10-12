using BoaSorte.Domain.Draws;

namespace BoaSorte.Domain.Tests.Draws;

public class ListPoolTests
{
    [Theory]
    [MemberData(nameof(NormalizationCases.All), MemberType = typeof(NormalizationCases))]
    public void Normalizes_according_to_the_shared_cases(NormalizationCase @case)
    {
        var pool = ListPool.From(@case.Input, @case.RemoveDuplicates).Value;

        Assert.Equal(@case.Expected, pool.Participants.Select(p => p.Value));
        Assert.Equal(@case.Input.Length, pool.SubmittedCount);
        Assert.Equal(@case.Expected.Length, pool.Count);
    }

    [Fact]
    public void Value_at_returns_eligible_participants_in_original_order()
    {
        var pool = ListPool.From(["  Ana", "", "Bruno"], removeDuplicates: false).Value;

        Assert.Equal("Ana", pool.ValueAt(0));
        Assert.Equal("Bruno", pool.ValueAt(1));
    }

    [Fact]
    public void Every_entry_that_is_too_long_is_reported_with_its_index()
    {
        var tooLong = new string('x', Participant.MaxLength + 1);

        var result = ListPool.From(["Ana", tooLong, "Bruno", tooLong], removeDuplicates: false);

        Assert.Equal(["participants[1]", "participants[3]"], result.Errors.Select(e => e.Field));
        Assert.All(result.Errors, e => Assert.Equal("participant.too_long", e.Code));
    }

    [Fact]
    public void More_entries_than_the_maximum_are_rejected()
    {
        var entries = Enumerable.Range(0, ListPool.MaxEntries + 1).Select(i => $"P{i}").ToArray();

        var error = Assert.Single(ListPool.From(entries, removeDuplicates: false).Errors);

        Assert.Equal("participants.too_many", error.Code);
        Assert.Equal("participants", error.Field);
    }

    [Fact]
    public void Exactly_the_maximum_number_of_entries_is_accepted()
    {
        var entries = Enumerable.Range(0, ListPool.MaxEntries).Select(i => $"P{i}").ToArray();

        Assert.Equal(ListPool.MaxEntries, ListPool.From(entries, removeDuplicates: false).Value.Count);
    }
}

public class RangePoolTests
{
    [Fact]
    public void Exposes_the_range_without_materializing_it()
    {
        var pool = new RangePool(NumberRange.Create(NumberRange.MinValue, NumberRange.MaxValue).Value);

        Assert.Equal(2_000_000_001L, pool.Count);
        Assert.Equal("-1000000000", pool.ValueAt(0));
        Assert.Equal("1000000000", pool.ValueAt(pool.Count - 1));
    }
}
