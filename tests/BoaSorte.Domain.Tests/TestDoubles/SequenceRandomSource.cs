using BoaSorte.Domain.Common;

namespace BoaSorte.Domain.Tests.TestDoubles;

/// <summary>Returns pre-defined values, failing loudly when a value is out of the requested range.</summary>
public sealed class SequenceRandomSource(params long[] values) : IRandomSource
{
    private readonly Queue<long> _values = new(values);

    public int NextInt32(int maxExclusive) => (int)Next(maxExclusive);

    public long NextInt64(long maxExclusive) => Next(maxExclusive);

    private long Next(long maxExclusive)
    {
        if (!_values.TryDequeue(out var value))
        {
            throw new InvalidOperationException("SequenceRandomSource ran out of values.");
        }

        if (value < 0 || value >= maxExclusive)
        {
            throw new InvalidOperationException($"Queued value {value} is outside [0, {maxExclusive}).");
        }

        return value;
    }
}
