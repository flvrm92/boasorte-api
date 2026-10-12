using BoaSorte.Domain.Common;

namespace BoaSorte.Domain.Draws;

/// <summary>
/// Picks <c>k</c> distinct indexes out of <c>[0, n)</c> uniformly and in uniformly random order.
/// Robert Floyd's algorithm selects the set in O(k) without materializing the population;
/// a Fisher–Yates shuffle of the k picks then makes every ordering equally likely.
/// </summary>
public static class WinnerSampler
{
    public static IReadOnlyList<long> Sample(long populationSize, int sampleSize, IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(random);
        ArgumentOutOfRangeException.ThrowIfLessThan(populationSize, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(sampleSize, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(sampleSize, populationSize);

        var selected = new HashSet<long>(sampleSize);
        var picks = new long[sampleSize];
        var next = 0;

        for (var j = populationSize - sampleSize; j < populationSize; j++)
        {
            var candidate = random.NextInt64(j + 1);
            var pick = selected.Add(candidate) ? candidate : j;
            selected.Add(pick);
            picks[next++] = pick;
        }

        for (var i = picks.Length - 1; i > 0; i--)
        {
            var swap = random.NextInt32(i + 1);
            (picks[i], picks[swap]) = (picks[swap], picks[i]);
        }

        return picks;
    }
}
