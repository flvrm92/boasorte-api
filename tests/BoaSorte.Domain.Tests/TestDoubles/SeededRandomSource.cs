using BoaSorte.Domain.Common;

namespace BoaSorte.Domain.Tests.TestDoubles;

/// <summary>Deterministic pseudo-random source for statistical tests. Never used in production code.</summary>
public sealed class SeededRandomSource(int seed) : IRandomSource
{
    private readonly Random _random = new(seed);

    public int NextInt32(int maxExclusive) => _random.Next(maxExclusive);

    public long NextInt64(long maxExclusive) => _random.NextInt64(maxExclusive);
}
