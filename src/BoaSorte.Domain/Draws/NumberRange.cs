using BoaSorte.Domain.Common;

namespace BoaSorte.Domain.Draws;

/// <summary>Inclusive integer interval for numeric draws. Never materialized as a list.</summary>
public sealed record NumberRange
{
    public const int MinValue = -1_000_000_000;
    public const int MaxValue = 1_000_000_000;

    private NumberRange(int start, int end)
    {
        Start = start;
        End = end;
    }

    public int Start { get; }

    public int End { get; }

    public long Count => (long)End - Start + 1;

    public static Result<NumberRange> Create(int start, int end)
    {
        if (start < MinValue || end > MaxValue || start > MaxValue || end < MinValue)
        {
            return Error.Validation(
                "range.out_of_bounds",
                $"O intervalo deve estar entre {MinValue:N0} e {MaxValue:N0}.",
                "range");
        }

        if (start > end)
        {
            return Error.Validation("range.start_after_end", "O início deve ser menor ou igual ao fim.", "range");
        }

        return new NumberRange(start, end);
    }

    public int ValueAt(long index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);

        return (int)(Start + index);
    }
}
