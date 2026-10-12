namespace BoaSorte.Domain.Draws;

/// <summary>The eligible participants of a draw, addressable by index so large pools need not be materialized.</summary>
public interface IParticipantPool
{
    DrawKind Kind { get; }

    /// <summary>Entries received before normalization and de-duplication.</summary>
    long SubmittedCount { get; }

    /// <summary>Eligible participants.</summary>
    long Count { get; }

    string ValueAt(long index);
}
