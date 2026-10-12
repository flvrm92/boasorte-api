using BoaSorte.Infrastructure.Randomness;

namespace BoaSorte.Api.IntegrationTests.Persistence;

public class CryptoRandomSourceTests
{
    private readonly CryptoRandomSource _random = new();

    [Theory]
    [InlineData(1L)]
    [InlineData(7L)]
    [InlineData(int.MaxValue + 1L)]
    [InlineData(2_000_000_001L)]
    [InlineData(1L << 62)]
    [InlineData(long.MaxValue)]
    public void Int64_values_stay_within_bounds(long maxExclusive)
    {
        for (var i = 0; i < 2_000; i++)
        {
            Assert.InRange(_random.NextInt64(maxExclusive), 0, maxExclusive - 1);
        }
    }

    [Fact]
    public void Int32_values_stay_within_bounds()
    {
        for (var i = 0; i < 2_000; i++)
        {
            Assert.InRange(_random.NextInt32(62), 0, 61);
        }
    }

    [Fact]
    public void Every_value_is_reachable()
    {
        var seen = new HashSet<long>();
        for (var i = 0; i < 1_000 && seen.Count < 5; i++)
        {
            seen.Add(_random.NextInt64(5));
        }

        Assert.Equal(5, seen.Count);
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    public void Rejects_empty_ranges(long maxExclusive)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _random.NextInt64(maxExclusive));
        Assert.Throws<ArgumentOutOfRangeException>(() => _random.NextInt32((int)maxExclusive));
    }
}
