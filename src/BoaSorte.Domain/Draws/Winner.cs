namespace BoaSorte.Domain.Draws;

/// <summary>A drawn winner. Created only by the domain, so an invalid position is a programming error.</summary>
public sealed record Winner
{
    public Winner(int position, string value)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(position, 1);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        Position = position;
        Value = value;
    }

    /// <summary>1 = first place.</summary>
    public int Position { get; }

    public string Value { get; }
}
