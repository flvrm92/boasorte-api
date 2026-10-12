using BoaSorte.Domain.Common;

namespace BoaSorte.Domain.Draws;

/// <summary>
/// Participants typed or imported as a list. Applies the normalization rules of spec 001 (FR-003, FR-004):
/// trim, drop blank entries and optionally drop case-insensitive duplicates, keeping the first spelling.
/// </summary>
public sealed class ListPool : IParticipantPool
{
    public const int MaxEntries = 10_000;

    private ListPool(IReadOnlyList<Participant> participants, int submittedCount)
    {
        Participants = participants;
        SubmittedCount = submittedCount;
    }

    public DrawKind Kind => DrawKind.List;

    public IReadOnlyList<Participant> Participants { get; }

    public long SubmittedCount { get; }

    public long Count => Participants.Count;

    public string ValueAt(long index) => Participants[checked((int)index)].Value;

    public static Result<ListPool> From(IReadOnlyList<string?> entries, bool removeDuplicates)
    {
        ArgumentNullException.ThrowIfNull(entries);

        if (entries.Count > MaxEntries)
        {
            return Error.Validation(
                "participants.too_many",
                $"A lista pode ter no máximo {MaxEntries:N0} participantes.",
                "participants");
        }

        var participants = new List<Participant>(entries.Count);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var errors = new List<Error>();

        for (var i = 0; i < entries.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(entries[i]))
            {
                continue;
            }

            var participant = Participant.Create(entries[i]);
            if (participant.IsFailure)
            {
                errors.AddRange(participant.Errors.Select(e => e with { Field = $"participants[{i}]" }));
                continue;
            }

            if (!removeDuplicates || seen.Add(participant.Value.DedupKey))
            {
                participants.Add(participant.Value);
            }
        }

        return errors.Count > 0
            ? Result.Failure<ListPool>(errors)
            : new ListPool(participants, entries.Count);
    }
}
