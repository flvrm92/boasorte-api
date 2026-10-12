using BoaSorte.Domain.Draws;

namespace BoaSorte.Domain.Tests.Draws;

public class WinnersCountTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(WinnersCount.Max)]
    public void Accepts_values_within_limits(int value)
    {
        Assert.Equal(value, WinnersCount.Create(value).Value.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(WinnersCount.Max + 1)]
    public void Rejects_values_outside_limits(int value)
    {
        var error = Assert.Single(WinnersCount.Create(value).Errors);
        Assert.Equal("winners_count.out_of_range", error.Code);
        Assert.Equal("winnersCount", error.Field);
    }
}

public class DrawTitleTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Blank_title_means_no_title(string? raw)
    {
        var result = DrawTitle.Create(raw);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Title_is_trimmed()
    {
        Assert.Equal("Rifa", DrawTitle.Create("  Rifa ").Value!.Value);
    }

    [Fact]
    public void Title_longer_than_maximum_is_rejected()
    {
        var error = Assert.Single(DrawTitle.Create(new string('x', DrawTitle.MaxLength + 1)).Errors);
        Assert.Equal("title", error.Field);
    }
}

public class WinnerTests
{
    [Fact]
    public void Position_must_start_at_one()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Winner(0, "Ana"));
        Assert.Equal(1, new Winner(1, "Ana").Position);
    }
}
