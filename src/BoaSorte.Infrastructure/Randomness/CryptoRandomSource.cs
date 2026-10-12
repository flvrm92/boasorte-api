using System.Buffers.Binary;
using System.Security.Cryptography;
using BoaSorte.Domain.Common;

namespace BoaSorte.Infrastructure.Randomness;

/// <summary>Cryptographically secure, unbiased random integers (constitution §1).</summary>
public sealed class CryptoRandomSource : IRandomSource
{
    public int NextInt32(int maxExclusive)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxExclusive, 1);
        return RandomNumberGenerator.GetInt32(maxExclusive);
    }

    public long NextInt64(long maxExclusive)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxExclusive, 1);
        if (maxExclusive <= int.MaxValue)
        {
            return RandomNumberGenerator.GetInt32((int)maxExclusive);
        }

        // Rejection sampling: drop the top 2^64 mod n values so every result is equally likely.
        var n = (ulong)maxExclusive;
        var excess = (ulong.MaxValue % n + 1) % n;
        var lastAccepted = ulong.MaxValue - excess;

        Span<byte> buffer = stackalloc byte[sizeof(ulong)];
        ulong value;
        do
        {
            RandomNumberGenerator.Fill(buffer);
            value = BinaryPrimitives.ReadUInt64LittleEndian(buffer);
        }
        while (excess != 0 && value > lastAccepted);

        return (long)(value % n);
    }
}
