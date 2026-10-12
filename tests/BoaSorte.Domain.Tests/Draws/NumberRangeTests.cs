using BoaSorte.Domain.Draws;

namespace BoaSorte.Domain.Tests.Draws;

public class NumberRangeTests
{
    [Theory]
    [InlineData(1, 500, 500)]
    [InlineData(7, 7, 1)]
    [InlineData(-10, 10, 21)]
    [InlineData(NumberRange.MinValue, NumberRange.MaxValue, 2_000_000_001L)]
    public void Count_includes_both_bounds(int start, int end, long expected)
    {
        Assert.Equal(expected, NumberRange.Create(start, end).Value.Count);
    }

    [Fact]
    public void Start_greater_than_end_is_rejected()
    {
        var error = Assert.Single(NumberRange.Create(10, 1).Errors);
        Assert.Equal("range.start_after_end", error.Code);
    }

    [Theory]
    [InlineData(NumberRange.MinValue - 1, 0)]
    [InlineData(0, NumberRange.MaxValue + 1)]
    public void Bounds_outside_the_limits_are_rejected(int start, int end)
    {
        Assert.Contains(NumberRange.Create(start, end).Errors, e => e.Code == "range.out_of_bounds");
    }

    [Fact]
    public void Value_at_maps_index_to_number()
    {
        var range = NumberRange.Create(-2, 2).Value;

        Assert.Equal(-2, range.ValueAt(0));
        Assert.Equal(2, range.ValueAt(4));
        Assert.Throws<ArgumentOutOfRangeException>(() => range.ValueAt(5));
    }
}
