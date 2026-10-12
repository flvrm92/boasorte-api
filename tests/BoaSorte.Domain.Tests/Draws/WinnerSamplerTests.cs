using BoaSorte.Domain.Draws;
using BoaSorte.Domain.Tests.TestDoubles;

namespace BoaSorte.Domain.Tests.Draws;

public class WinnerSamplerTests
{
    [Fact]
    public void Returns_the_requested_number_of_distinct_indexes_within_the_population()
    {
        var indexes = WinnerSampler.Sample(populationSize: 10, sampleSize: 4, new SeededRandomSource(1));

        Assert.Equal(4, indexes.Count);
        Assert.Equal(4, indexes.Distinct().Count());
        Assert.All(indexes, i => Assert.InRange(i, 0, 9));
    }

    [Fact]
    public void Sampling_the_whole_population_returns_a_permutation()
    {
        var indexes = WinnerSampler.Sample(populationSize: 6, sampleSize: 6, new SeededRandomSource(2));

        Assert.Equal([0L, 1, 2, 3, 4, 5], indexes.Order());
    }

    [Fact]
    public void Follows_floyd_selection_then_shuffles_the_order()
    {
        // Floyd for n=5, k=2: j=3 picks 1; j=4 picks 1 again (taken) so takes 4 => [1, 4].
        // Shuffle: i=1 swaps with index 0 => [4, 1].
        var random = new SequenceRandomSource(1, 1, 0);

        Assert.Equal([4L, 1], WinnerSampler.Sample(populationSize: 5, sampleSize: 2, random));
    }

    [Fact]
    public void Handles_huge_populations_without_materializing_them()
    {
        const long population = 2_000_000_001L;

        var indexes = WinnerSampler.Sample(population, WinnersCount.Max, new SeededRandomSource(3));

        Assert.Equal(WinnersCount.Max, indexes.Distinct().Count());
        Assert.All(indexes, i => Assert.InRange(i, 0, population - 1));
    }

    [Fact]
    public void Every_participant_has_the_same_chance_to_win()
    {
        const int runs = 10_000;
        var random = new SeededRandomSource(42);
        var hits = new int[5];

        for (var run = 0; run < runs; run++)
        {
            hits[WinnerSampler.Sample(5, 1, random)[0]]++;
        }

        Assert.All(hits, h => Assert.InRange(h, 1_800, 2_200)); // 2,000 expected, ±10%
    }

    [Fact]
    public void Every_participant_has_the_same_chance_to_be_first_place()
    {
        // Without the final shuffle, Floyd never puts the last index first when k < n.
        const int runs = 30_000;
        var random = new SeededRandomSource(7);
        var firstPlace = new int[3];

        for (var run = 0; run < runs; run++)
        {
            firstPlace[WinnerSampler.Sample(3, 2, random)[0]]++;
        }

        Assert.All(firstPlace, h => Assert.InRange(h, 9_000, 11_000)); // 10,000 expected, ±10%
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(5, 0)]
    [InlineData(5, 6)]
    public void Rejects_invalid_sizes(long populationSize, int sampleSize)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => WinnerSampler.Sample(populationSize, sampleSize, new SeededRandomSource(0)));
    }
}
