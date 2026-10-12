using System.Globalization;

namespace BoaSorte.Domain.Draws;

/// <summary>Every integer of a <see cref="NumberRange"/>, computed on demand.</summary>
public sealed class RangePool(NumberRange range) : IParticipantPool
{
    public NumberRange Range { get; } = range ?? throw new ArgumentNullException(nameof(range));

    public DrawKind Kind => DrawKind.Range;

    public long SubmittedCount => Range.Count;

    public long Count => Range.Count;

    public string ValueAt(long index) => Range.ValueAt(index).ToString(CultureInfo.InvariantCulture);
}
