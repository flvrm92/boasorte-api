namespace BoaSorte.Domain.Common;

/// <summary>
/// Source of uniform random integers. Production uses a cryptographically secure generator (constitution §1);
/// tests use deterministic sources.
/// </summary>
public interface IRandomSource
{
    /// <summary>Uniform integer in [0, <paramref name="maxExclusive"/>).</summary>
    int NextInt32(int maxExclusive);

    /// <summary>Uniform integer in [0, <paramref name="maxExclusive"/>).</summary>
    long NextInt64(long maxExclusive);
}
